#!/usr/bin/env bash
# Spec 018 design probe. Re-runs 017's second-compile probe at the current pin, then joins it
# to the 64 pages no 017 tranche reached and sorts their hard blocks by repair rule.
# Scratch only: writes under $1, changes nothing in the tree. Run from the Docs root, after
# building refs.csproj and blockcheck.csproj.
#   bash spec/018-compile_residual/probe/run.sh <work-dir>
set -euo pipefail
W=${1:?usage: run.sh <work-dir>}; P=spec/018-compile_residual/probe; P17=spec/017-compile_repairs/probe
bash "$P17/run.sh" "$W"                                      # E1: report, stage, classes, types
cp "$W/types.tsv" "$W/types.full.tsv"                       # 017 § The tranches: Order is a page type
awk -F'\t' '!($1=="type" && $2=="Order" && $3=="StackExchange.Redis")' "$W/types.full.tsv" > "$W/types.tsv"
python3 "$P17/pages.py" "$W" "$W/r.tsv"                      # verdicts.tsv: BUILT STUB PARSE DEFECT
python3 "$P17/stubs.py" "$W"                                 # stubs.tsv: BUILT MEMBERS HIDDEN SAME-PAGE
python3 tools/blockcheck.py --classify > "$W/cls.tsv"
python3 tools/blockcheck.py --list > "$W/list.tsv"
python3 "$P/tranche_pages.py" spec/017-compile_repairs/tasks.md > "$W/tranche.txt"
python3 "$P/offtable.py" "$W" > "$W/offtable.tsv"            # one row per off-tranche page
python3 "$P/hardcat.py" "$W"                                 # defcat.tsv, parcat.tsv, and the counts
