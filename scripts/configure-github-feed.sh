#!/bin/bash
set -e

# Script to configure GitHub Packages feed for private NuGet packages.
# 
# This script auto-detects the fork owner from the git remote URL and adds
# the corresponding GitHub Packages feed, allowing contributors to resolve
# packages published from their fork.
#
# Usage:
#   ./scripts/configure-github-feed.sh [--gh]
#
# Options:
#   --gh       Use GitHub CLI (gh) to retrieve the authentication token
#              (requires: gh auth with read:packages scope)
#              If not specified, uses GITHUB_PACKAGES_PAT environment variable
#
# Example:
#   export GITHUB_PACKAGES_PAT="your_github_pat"
#   ./scripts/configure-github-feed.sh
#
# Or:
#   ./scripts/configure-github-feed.sh --gh

use_gh_token=false
if [ "${1:-}" = "--gh" ]; then
	use_gh_token=true
fi

# Auto-detect fork owner from git remote URL
detect_fork_owner() {
	local remote_url
	remote_url="$(git config --get remote.origin.url)"
	
	if [ -z "$remote_url" ]; then
		echo "ERROR: Unable to determine git remote URL (not a git repository?)" >&2
		exit 1
	fi
	
	# Extract owner from URL: github.com/Owner/repo.git or github.com:Owner/repo.git
	local owner
	owner="$(echo "$remote_url" | sed -E 's|.*[:/]([^/]+)/[^/]+\.git$|\1|')"
	
	if [ -z "$owner" ] || [ "$owner" = "$remote_url" ]; then
		echo "ERROR: Unable to extract owner from git remote URL: $remote_url" >&2
		exit 1
	fi
	
	echo "$owner"
}

# Verify GitHub token has required scope
assert_gh_packages_scope() {
	local headers
	headers="$(gh api -i /user 2>/dev/null || true)"
	if [ -z "$headers" ]; then
		return 0
	fi

	local scopes
	scopes="$(echo "$headers" | tr -d '\r' | awk -F': ' 'tolower($1)=="x-oauth-scopes"{print $2}' | head -n 1)"
	if [ -z "$scopes" ]; then
		return 0
	fi

	case ",$scopes," in
		*,read:packages,*|*,write:packages,*)
			return 0
			;;
	esac

	echo "ERROR: GitHub token is missing required scope: read:packages" >&2
	echo "Fix:" >&2
	echo "  gh auth refresh -h github.com -s read:packages" >&2
	echo "Then rerun:" >&2
	echo "  ./scripts/configure-github-feed.sh --gh" >&2
	exit 1
}

# Main
FORK_OWNER="$(detect_fork_owner)"
echo "Detected fork owner: $FORK_OWNER"

FEED_NAME="github-$(echo "$FORK_OWNER" | tr '[:upper:]' '[:lower:]')"  # Convert to lowercase
FEED_URL="https://nuget.pkg.github.com/${FORK_OWNER}/index.json"

if [ "$use_gh_token" = true ]; then
	if ! command -v gh >/dev/null 2>&1; then
		echo "ERROR: gh CLI is not installed." >&2
		echo "Install it from https://cli.github.com/ or use GITHUB_PACKAGES_PAT instead." >&2
		exit 1
	fi
	PAT="$(gh auth token)"
	if [ -z "$PAT" ]; then
		echo "ERROR: gh CLI is not authenticated." >&2
		echo "Run: gh auth login" >&2
		exit 1
	fi
	assert_gh_packages_scope
	echo "Using GitHub token from gh CLI"
else
	if [ -z "${GITHUB_PACKAGES_PAT:-}" ]; then
		echo "ERROR: GITHUB_PACKAGES_PAT environment variable is not set." >&2
		echo "" >&2
		echo "Please set your GitHub Personal Access Token (needs read:packages):" >&2
		echo "  export GITHUB_PACKAGES_PAT=\"your_github_pat_here\"" >&2
		echo "" >&2
		echo "Or use the gh CLI shortcut:" >&2
		echo "  ./scripts/configure-github-feed.sh --gh" >&2
		exit 1
	fi
	PAT="$GITHUB_PACKAGES_PAT"
fi

# Add or update the feed
echo "Checking for existing feed '$FEED_NAME'..."
if dotnet nuget list source | grep -q "$FEED_NAME"; then
	echo "Found existing feed, removing..."
	dotnet nuget remove source "$FEED_NAME" >/dev/null 2>&1 || true
fi

echo "Adding GitHub Packages feed..."
echo "  Name: $FEED_NAME"
echo "  URL:  $FEED_URL"

dotnet nuget add source "$FEED_URL" \
	--name "$FEED_NAME" \
	--username "$FORK_OWNER" \
	--password "$PAT" \
	--store-password-in-clear-text

echo ""
echo "✓ GitHub Packages feed configured successfully!"
echo ""
echo "You can now restore packages from your fork:"
echo "  dotnet restore"
echo ""
echo "Configured sources:"
dotnet nuget list source
