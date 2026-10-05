#!/usr/bin/env bash
# Spec 018 design probe E3: the pin with the seven AWS packages swapped for their V4 twins,
# and the six CS0234 blocks compiled against both pins -- the case, and the control. Also every
# BUILT block on an AWS-family page against the V4 pin, the other direction.
#   bash spec/018-compile_residual/probe/v4pin.sh <work-dir>      (after run.sh, which stages)
set -euo pipefail
W=${1:?usage: v4pin.sh <work-dir>}; D=tools/blockcheck/bin/Release/net9.0/blockcheck.dll
MAIN=tools/blockcheck/refs/bin/Release/net9.0/refs.txt
mkdir -p "$W/refsv4"
for p in DynamoDb Inbox.DynamoDB Locking.DynamoDB MessageScheduler.AWS MessagingGateway.AWSSQS Outbox.DynamoDB Transformers.AWS; do
  printf 's#"Paramore.Brighter.%s" Version#"Paramore.Brighter.%s.V4" Version#\n' "$p" "$p"
done > "$W/refsv4/swap.sed"
sed -f "$W/refsv4/swap.sed" tools/blockcheck/refs/refs.csproj > "$W/refsv4/refsv4.csproj"
echo "V4 packages swapped in: $(grep -c '\.V4" Version' "$W/refsv4/refsv4.csproj")"
dotnet build "$W/refsv4/refsv4.csproj" -c Release > "$W/refsv4.log"; tail -3 "$W/refsv4.log"
V4="$W/refsv4/bin/Release/net9.0/refs.txt"
SIX=(AwsScheduler_2 AwsScheduler_3 DistributedLock_2 DynamoDbDistributedLock_1 DynamoDbDistributedLock_2 S3LuggageStore_1)
count() { awk -F'\t' 'NF>=4{c[$1]++; if($2=="CS0234")v[$1]++} END{for(k in c) print "  "k": "c[k]" diagnostics, "(v[k]+0)" CS0234"}' | sort; }
echo "case: the six against the V4 pin";    dotnet "$D" --explain "$W/stage" "$V4"   "${SIX[@]}" 2>/dev/null | count
echo "control: the six against the main pin"; dotnet "$D" --explain "$W/stage" "$MAIN" "${SIX[@]}" 2>/dev/null | count
AWS=($(awk -F'\t' '$1=="BUILT" && $2 ~ /AwsScheduler|\/DistributedLock\.md|DynamoDb|S3LuggageStore|AWSSQS|DynamoOutbox|DynamoInbox/{print $4}' "$W/r.tsv"))
echo "reverse: ${#AWS[@]} BUILT AWS-family blocks against the V4 pin -- each line is one that breaks"
dotnet "$D" --explain "$W/stage" "$V4" "${AWS[@]}" 2>/dev/null | awk -F'\t' 'NF>=4{print "  "$1}' | sort | uniq -c
