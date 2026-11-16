#!/usr/bin/env bash
# Checks the log review agent's output before anything leaves the job (Migration.md 9.5). The repository is public, so:
# - titles and bodies must not contain ids (GUIDs), IP addresses or e-mail addresses;
# - the patch must not add IP or e-mail addresses, and must not touch the workflows, the version or the release notes.
#
# Usage: log-review-check.sh <output folder>   (pr-title.txt, pr-body.md, issue-title.txt, issue-body.md, changes.patch; all optional)
set -euo pipefail

out="$1"
failed=0
fail() { echo "::error::$1"; failed=1; }

guid='[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}'
ipv4='(^|[^0-9.])([0-9]{1,3}\.){3}[0-9]{1,3}([^0-9.]|$)'
email='[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}'

for text in pr-title.txt pr-body.md issue-title.txt issue-body.md; do
    file="$out/$text"
    [ -f "$file" ] || continue
    grep -Eq "$guid" "$file" && fail "$text contains an id (GUID)."
    grep -Eq "$ipv4" "$file" && fail "$text contains an IP address."
    grep -Eq "$email" "$file" && fail "$text contains an e-mail address."
    [ "$(wc -c < "$file")" -le 30000 ] || fail "$text is longer than 30000 characters."
done
for title in pr-title.txt issue-title.txt; do
    [ -f "$out/$title" ] || continue
    [ "$(wc -l < "$out/$title")" -le 1 ] && [ "$(wc -c < "$out/$title")" -le 120 ] || fail "$title must be one line of at most 120 characters."
done
[ -f "$out/pr-title.txt" ] && [ ! -f "$out/pr-body.md" ] && fail "pr-title.txt without pr-body.md."
[ -f "$out/issue-title.txt" ] && [ ! -f "$out/issue-body.md" ] && fail "issue-title.txt without issue-body.md."

patch="$out/changes.patch"
if [ -s "$patch" ]; then
    [ -f "$out/pr-title.txt" ] || fail "There are code changes but no pull request text."
    if grep -E '^(\+\+\+|---) [ab]/' "$patch" | sed -E 's:^(\+\+\+|---) [ab]/::' \
        | grep -Eq '^(\.github/|Directory\.Build\.props$|RELEASENOTES\.md$|SourceCode/App/Content/Markdown/TermsOfUse)'; then
        fail "The patch changes a protected file (.github, Directory.Build.props, RELEASENOTES.md or the Terms of Use)."
    fi
    added=$(grep -E '^\+[^+]' "$patch" || true)
    echo "$added" | grep -Eq "$ipv4" && fail "The patch adds an IP address."
    echo "$added" | grep -Eq "$email" && fail "The patch adds an e-mail address."
elif [ -f "$out/pr-title.txt" ]; then
    fail "There is pull request text but no code changes."
fi

exit $failed