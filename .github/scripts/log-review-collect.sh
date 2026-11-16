#!/usr/bin/env bash
# Collects the input for the daily log review (Migration.md 9.5) into a folder:
# one JSON file per query in .github/log-review/queries, and the open and recently closed log review issues and pull requests.
#
# Usage: log-review-collect.sh <output folder>
# Needs: az signed in with read access to the workspace, LOG_ANALYTICS_WORKSPACE_ID, gh with GH_TOKEN (issues and pull requests: read).
#
# The results can hold text users typed (exception messages). They are written to files only, never to the log:
# the repository is public, and so are its workflow logs.
set -euo pipefail

out="$1"
mkdir -p "$out"
: "${LOG_ANALYTICS_WORKSPACE_ID:?LOG_ANALYTICS_WORKSPACE_ID is not set}"

for query in .github/log-review/queries/*.kql; do
    name=$(basename "$query" .kql)
    # Comment lines are stripped: the Azure CLI fails to parse some queries that start with one.
    if az monitor log-analytics query --workspace "$LOG_ANALYTICS_WORKSPACE_ID" --timespan P8D \
        --analytics-query "$(grep -v '^[[:space:]]*//' "$query")" --output json > "$out/$name.json" 2> "$out/$name.error"; then
        rm "$out/$name.error"
        echo "$name: $(jq length "$out/$name.json") rows"
    else
        echo "::warning::The query $name failed; the review runs without it."
        rm -f "$out/$name.json"
    fi
done

since=$(date -u -d '30 days ago' +%Y-%m-%d)
gh issue list --label log-review --state all --search "updated:>=$since" --limit 100 \
    --json number,title,state,stateReason,closedAt,body > "$out/issues.json"
gh pr list --label log-review --state all --search "updated:>=$since" --limit 100 \
    --json number,title,state,closedAt,mergedAt,body,headRefName > "$out/pull-requests.json"
echo "Earlier reports: $(jq length "$out/issues.json") issues, $(jq length "$out/pull-requests.json") pull requests"