#!/usr/bin/env ruby

require "pathname"

repo_root = Pathname.new(__dir__).join("..").realpath
tests_root = repo_root.join("tests", "Plugin.Firebase.IntegrationTests")
coverage_file = tests_root.join("ACCEPTANCE_COVERAGE.md")

PACKAGE_LABELS = {
  "Analytics" => "Analytics",
  "AppCheck" => "App Check",
  "Auth" => "Auth",
  "Bundled" => "Bundled initializer",
  "CloudMessaging" => "Cloud Messaging",
  "Crashlytics" => "Crashlytics",
  "Firestore" => "Firestore",
  "Functions" => "Functions",
  "Installations" => "Installations",
  "PerformanceMonitoring" => "Performance Monitoring",
  "RemoteConfig" => "Remote Config",
  "Storage" => "Storage"
}.freeze

def coverage_packages(coverage_file)
  packages = {}
  File.readlines(coverage_file).each do |line|
    next unless line.start_with?("|")

    cells = line.split("|").map(&:strip)
    package = cells[1]
    next if package.nil? || package.empty? || package == "Package" || package.start_with?("---")

    packages[package] = true
  end
  packages
end

def class_declarations(content)
  declarations = []
  class_pattern = /^\s*(?:public|internal|private)?\s*(?:sealed\s+)?(?:partial\s+)?class\s+(\w+Fixture)\b/
  content.to_enum(:scan, class_pattern).each do
    match = Regexp.last_match
    prefix = content[0...match.begin(0)].lines.last(10).join
    declarations << {
      name: match[1],
      package: prefix[/IntegrationTestFixture\(IntegrationTestPackage\.(\w+)\)/, 1],
      ignored: prefix.include?("IntegrationTestCoverageIgnore(")
    }
  end
  declarations
end

def test_case_count(content)
  content.scan(/^\s*\[(?:\w*(?:Fact|Theory)|Fact|Theory)(?:\(|\])/).size
end

coverage = coverage_packages(coverage_file)
errors = []
fixtures = {}
test_cases = 0

Dir.glob(tests_root.join("**", "*.cs")).sort.each do |file|
  next if file.include?("/bin/") || file.include?("/obj/")

  content = File.read(file)
  test_cases += test_case_count(content)
  class_declarations(content).each do |declaration|
    fixture = fixtures[declaration[:name]] ||= {
      files: [],
      packages: [],
      ignored: false
    }
    fixture[:files] << Pathname.new(file).relative_path_from(repo_root).to_s
    fixture[:packages] << declaration[:package] if declaration[:package]
    fixture[:ignored] ||= declaration[:ignored]
  end
end

fixtures.each do |name, fixture|
  next if fixture[:ignored]

  packages = fixture[:packages].uniq
  if packages.empty?
    errors << "#{name} is missing IntegrationTestFixture metadata."
    next
  end

  packages.each do |package|
    label = PACKAGE_LABELS[package]
    if label.nil?
      errors << "#{name} references unknown IntegrationTestPackage.#{package}."
    elsif !coverage.key?(label)
      errors << "#{name} maps to #{label}, but #{coverage_file.relative_path_from(repo_root)} does not list that package."
    end
  end
end

if coverage.key?("Dynamic Links")
  errors << "Dynamic Links is intentionally excluded and must not be listed in #{coverage_file.relative_path_from(repo_root)}."
end

if errors.any?
  warn "Integration coverage metadata audit failed:"
  errors.each { |error| warn " - #{error}" }
  exit 1
end

tracked_packages = fixtures
  .values
  .flat_map { |fixture| fixture[:packages] }
  .compact
  .uniq
  .sort
  .map { |package| PACKAGE_LABELS.fetch(package, package) }

ignored_fixtures = fixtures.count { |_, fixture| fixture[:ignored] }

puts "Integration coverage metadata audit passed."
puts "Fixtures: #{fixtures.length} (#{ignored_fixtures} ignored harness fixture)"
puts "Packages: #{tracked_packages.join(", ")}"
puts "Test cases discovered: #{test_cases}"
