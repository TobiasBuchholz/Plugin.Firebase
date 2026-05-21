#!/usr/bin/env ruby

require "digest"
require "pathname"
require "rexml/document"

title = ARGV[0]
results_directory = ARGV[1]

if title.nil? || results_directory.nil?
  warn "Usage: write-xunit-github-summary.rb <title> <results-directory>"
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

def text_at(node, path)
  child = REXML::XPath.first(node, path)
  child&.text.to_s.strip
rescue StandardError
  ""
end

def test_name(test)
  test.attributes["name"].to_s.empty? ? "#{test.attributes["type"]}.#{test.attributes["method"]}" : test.attributes["name"].to_s
end

def xml_files(results_directory)
  Dir.glob(File.join(results_directory, "**", "*.xml")).sort
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
    next if File.extname(file).casecmp(".xml").zero?

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
  lines << "No xUnit XML results were found because the results directory does not exist: `#{results_directory}`."
  lines << ""
  append_summary(lines)
  exit 0
end

seen_contents = {}
assemblies = []
tests = []

xml_files(results_directory).each do |result_file|
  content = File.read(result_file)
  digest = Digest::SHA256.hexdigest(content)
  next if seen_contents.key?(digest)

  seen_contents[digest] = true

  begin
    document = REXML::Document.new(content)
  rescue REXML::ParseException
    next
  end

  REXML::XPath.each(document, "/assemblies/assembly") do |assembly|
    next unless assembly.attributes["total"]

    assemblies << {
      file: result_file,
      total: assembly.attributes["total"].to_i,
      passed: assembly.attributes["passed"].to_i,
      failed: assembly.attributes["failed"].to_i,
      skipped: assembly.attributes["skipped"].to_i,
      errors: assembly.attributes["errors"].to_i,
      time: assembly.attributes["time"].to_f
    }
  end

  REXML::XPath.each(document, "//test") do |test|
    tests << {
      file: result_file,
      name: test_name(test),
      result: test.attributes["result"].to_s,
      time: test.attributes["time"].to_f,
      failure: text_at(test, "failure/message"),
      skip_reason: test.attributes["reason"].to_s.empty? ? text_at(test, "reason") : test.attributes["reason"].to_s
    }
  end
rescue StandardError
  next
end

if assemblies.empty?
  lines << "No xUnit assembly summaries were found under `#{results_directory}`."
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

totals = assemblies.each_with_object(Hash.new(0)) do |assembly, memo|
  memo[:total] += assembly[:total]
  memo[:passed] += assembly[:passed]
  memo[:failed] += assembly[:failed]
  memo[:skipped] += assembly[:skipped]
  memo[:errors] += assembly[:errors]
  memo[:time] += assembly[:time]
end

source_files = assemblies.map { |assembly| relative_path(assembly[:file]) }.uniq

failed_tests = tests.select { |test| test[:result].casecmp("Fail").zero? || !test[:failure].empty? }
skipped_tests = tests.select { |test| test[:result].casecmp("Skip").zero? }
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
