#!/usr/bin/env ruby

require "digest"
require "pathname"
require "rexml/document"
require "time"

title = ARGV[0]
results_directory = ARGV[1]

if title.nil? || results_directory.nil?
  warn "Usage: write-trx-github-summary.rb <title> <results-directory>"
  exit 1
end

def append_summary(lines)
  summary_path = ENV["GITHUB_STEP_SUMMARY"]
  if summary_path.nil? || summary_path.empty?
    puts lines.join("\n")
    return
  end

  File.open(summary_path, "a") do |file|
    file.puts lines.join("\n")
  end
end

def relative_path(path)
  Pathname.new(path).relative_path_from(Pathname.pwd).to_s
rescue ArgumentError
  path
end

def text_at(node, path, namespaces)
  child = REXML::XPath.first(node, path, namespaces)
  child&.text.to_s.strip
rescue StandardError
  ""
end

def duration_seconds(value)
  match = value.to_s.match(/\A(?:(\d+)\.)?(\d{2}):(\d{2}):(\d+(?:\.\d+)?)\z/)
  return 0.0 unless match

  (match[1] || "0").to_i * 86_400 \
    + match[2].to_i * 3_600 \
    + match[3].to_i * 60 \
    + match[4].to_f
end

def run_duration(times)
  return 0.0 unless times

  Time.iso8601(times.attributes["finish"]) - Time.iso8601(times.attributes["start"])
rescue ArgumentError, TypeError
  0.0
end

def trx_files(results_directory)
  Dir.glob(File.join(results_directory, "**", "*.trx")).sort
rescue StandardError
  []
end

def log_files(results_directory)
  Dir.glob(File.join(results_directory, "**", "*")).select { |path| File.file?(path) }.sort
rescue StandardError
  []
end

def latest_test_breadcrumbs(results_directory)
  breadcrumbs = []
  log_files(results_directory).each do |file|
    next if File.extname(file).casecmp(".trx").zero?

    begin
      File.foreach(file) do |line|
        next unless line.include?("[TEST START]") || line.include?("[TEST END]")

        breadcrumbs << "#{relative_path(file)}: #{line.strip}"
        breadcrumbs.shift while breadcrumbs.length > 10
      end
    rescue StandardError
      next
    end
  end
  breadcrumbs
end

lines = ["### #{title}"]

unless Dir.exist?(results_directory)
  lines << "No TRX results were found because the results directory does not exist: `#{results_directory}`."
  lines << ""
  append_summary(lines)
  exit 0
end

seen_contents = {}
runs = []
tests = []

trx_files(results_directory).each do |result_file|
  content = File.read(result_file)
  digest = Digest::SHA256.hexdigest(content)
  next if seen_contents.key?(digest)

  seen_contents[digest] = true

  begin
    document = REXML::Document.new(content)
    namespace = document.root&.namespace
    next if namespace.nil? || namespace.empty?

    namespaces = { "t" => namespace }
    counters = REXML::XPath.first(document, "/t:TestRun/t:ResultSummary/t:Counters", namespaces)
    next unless counters

    result_nodes = REXML::XPath.match(
      document,
      "/t:TestRun/t:Results/t:UnitTestResult",
      namespaces
    )
    skipped = result_nodes.count { |test| test.attributes["outcome"] == "NotExecuted" }
    times = REXML::XPath.first(document, "/t:TestRun/t:Times", namespaces)

    runs << {
      file: result_file,
      total: counters.attributes["total"].to_i,
      passed: counters.attributes["passed"].to_i,
      failed: counters.attributes["failed"].to_i,
      skipped: skipped,
      errors: counters.attributes["error"].to_i,
      time: run_duration(times)
    }

    result_nodes.each do |test|
      outcome = test.attributes["outcome"].to_s
      failure = outcome == "Failed" ? text_at(test, "t:Output/t:ErrorInfo/t:Message", namespaces) : ""
      skip_reason = outcome == "NotExecuted" ? text_at(test, "t:Output/t:ErrorInfo/t:Message", namespaces) : ""

      tests << {
        file: result_file,
        name: test.attributes["testName"].to_s,
        result: outcome,
        time: duration_seconds(test.attributes["duration"]),
        failure: failure,
        skip_reason: skip_reason
      }
    end
  rescue REXML::ParseException
    next
  end
rescue StandardError
  next
end

if runs.empty?
  lines << "No DeviceRunners TRX summaries were found under `#{results_directory}`."
  lines << ""
  breadcrumbs = latest_test_breadcrumbs(results_directory)
  unless breadcrumbs.empty?
    lines << "#### Latest Test Breadcrumbs"
    breadcrumbs.each { |breadcrumb| lines << "- `#{breadcrumb}`" }
    lines << ""
  end
  append_summary(lines)
  exit 0
end

totals = runs.each_with_object(Hash.new(0)) do |run, memo|
  memo[:total] += run[:total]
  memo[:passed] += run[:passed]
  memo[:failed] += run[:failed]
  memo[:skipped] += run[:skipped]
  memo[:errors] += run[:errors]
  memo[:time] += run[:time]
end

source_files = runs.map { |run| relative_path(run[:file]) }.uniq
failed_tests = tests.select { |test| test[:result] == "Failed" }
skipped_tests = tests.select { |test| test[:result] == "NotExecuted" }
slow_tests = tests.select { |test| test[:time].positive? }.sort_by { |test| -test[:time] }.first(10)
breadcrumbs = latest_test_breadcrumbs(results_directory)

lines << "| Total | Passed | Failed | Skipped | Errors | Time |"
lines << "| ---: | ---: | ---: | ---: | ---: | ---: |"
lines << format(
  "| %<total>d | %<passed>d | %<failed>d | %<skipped>d | %<errors>d | %<time>.1fs |",
  totals
)
lines << ""
lines << "Results: #{source_files.map { |file| "`#{file}`" }.join(", ")}"
lines << ""

unless failed_tests.empty?
  lines << "#### Failed Tests"
  lines << "| Test | Message |"
  lines << "| --- | --- |"
  failed_tests.first(20).each do |test|
    message = test[:failure].lines.first.to_s.strip
    lines << "| `#{test[:name]}` | #{message.empty? ? "-" : message} |"
  end
  lines << ""
end

unless skipped_tests.empty?
  lines << "#### Skipped Tests"
  lines << "| Test | Reason |"
  lines << "| --- | --- |"
  skipped_tests.first(20).each do |test|
    reason = test[:skip_reason].empty? ? "-" : test[:skip_reason]
    lines << "| `#{test[:name]}` | #{reason} |"
  end
  lines << ""
end

unless slow_tests.empty?
  lines << "#### Slowest Tests"
  lines << "| Test | Time |"
  lines << "| --- | ---: |"
  slow_tests.each do |test|
    lines << format("| `%<name>s` | %<time>.1fs |", name: test[:name], time: test[:time])
  end
  lines << ""
end

unless breadcrumbs.empty?
  lines << "#### Latest Test Breadcrumbs"
  breadcrumbs.each { |breadcrumb| lines << "- `#{breadcrumb}`" }
  lines << ""
end

append_summary(lines)
