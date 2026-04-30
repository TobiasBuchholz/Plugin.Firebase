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

lines = ["### #{title}"]

unless Dir.exist?(results_directory)
  lines << "No xUnit XML results were found because the results directory does not exist: `#{results_directory}`."
  lines << ""
  append_summary(lines)
  exit 0
end

seen_contents = {}
assemblies = []

Dir.glob(File.join(results_directory, "**", "*.xml")).sort.each do |result_file|
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
end

if assemblies.empty?
  lines << "No xUnit assembly summaries were found under `#{results_directory}`."
  lines << ""
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

source_files = assemblies.map { |assembly| assembly[:file] }.uniq.map do |file|
  Pathname.new(file).relative_path_from(Pathname.pwd).to_s
rescue ArgumentError
  file
end

lines << "| Total | Passed | Failed | Skipped | Errors | Time |"
lines << "| ---: | ---: | ---: | ---: | ---: | ---: |"
lines << format(
  "| %<total>d | %<passed>d | %<failed>d | %<skipped>d | %<errors>d | %<time>.1fs |",
  totals
)
lines << ""
lines << "Results: #{source_files.map { |file| "`#{file}`" }.join(", ")}"
lines << ""

append_summary(lines)
