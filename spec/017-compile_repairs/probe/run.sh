#!/usr/bin/env bash
# Spec 017 requirements probe. Re-derives the failing-block classification that
# 016 task 3.3 measured with an uncommitted type dump, then asks how many
# "import" blocks build once given their using directives. Scratch only: it
# writes to $1 and changes nothing in the tree. Run from the Docs root.
set -euo pipefail
W=${1:?usage: run.sh <work-dir>}; P=spec/017-compile_repairs/probe
DLL=tools/blockcheck/bin/Release/net9.0/blockcheck.dll
REFS=tools/blockcheck/refs/bin/Release/net9.0/refs.txt
mkdir -p "$W"; rm -rf "$W/stage"
python3 tools/blockcheck.py --report "$W/r.tsv" > "$W/report.out"
python3 tools/blockcheck.py --stage "$W/stage" > /dev/null
ids=($(awk -F"\t" '$1=="FAILED"{print $4}' "$W/r.tsv"))
dotnet "$DLL" --explain "$W/stage" "$REFS" "${ids[@]}" > "$W/explain.txt" 2> /dev/null
dotnet run -c Release --project "$P/typedump" -- "$PWD/$REFS" > "$W/types.tsv"
python3 "$P/classify.py" "$W" "$W/r.tsv"
python3 "$P/usings.py" "$W"
