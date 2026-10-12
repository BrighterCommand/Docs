#!/bin/zsh
# Kafka rung, steps 5-8, against 10.8.0. $1 = "after" (rebalance after the 30 s sweep, the page's order) or "within" (rebalance inside it).
source ${0:A:h}/env.sh
cd $S/rung4
X=$S/k8-$1; rm -rf $X; mkdir -p $X
dc() { docker compose -f docker-compose-kafka.yml -f $S/kafka-override.yml "$@"; }
grp() { docker exec replay234-kafka /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server localhost:9092 --describe --group greeting.readers 2>&1 | grep -E 'greeting.event|CURRENT|Consumer group'; }
recvd() { cat $X/recv*.log 2>/dev/null | grep -c 'Received:'; }
waitfor() { for i in $(seq 1 120); do [ $(recvd) -ge $1 ] && return; sleep 1; done; echo "TIMEOUT waiting for $1"; }
dc down -v >/dev/null 2>&1; dc up -d >/dev/null 2>&1
for i in $(seq 1 60); do docker exec replay234-kafka /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server localhost:9092 >/dev/null 2>&1 && break; sleep 2; done
dotnet run --no-build --project GreetingsReceiver > $X/recv1.log 2>&1 & R1=$!
for i in $(seq 1 90); do grep -q 'Partition Added' $X/recv1.log && break; sleep 1; done
dotnet run --no-build --project GreetingsSender > $X/send1.log 2>&1
waitfor 9; echo "== D1: immediately after the first nine"; grp
if [ $1 = after ]; then sleep 40; echo "== D2: 40 s later"; grp; fi
dotnet run --no-build --project GreetingsReceiver > $X/recv2.log 2>&1 & R2=$!
for i in $(seq 1 90); do grep -q 'Partition Added' $X/recv2.log && break; sleep 1; done; sleep 8
echo "== received after the rebalance, before the second send: $(recvd) (9 = no duplicates)"
T=$(( $(recvd) + 9 )); dotnet run --no-build --project GreetingsSender > $X/send2.log 2>&1
waitfor $T; echo "== D3: immediately after the second nine"; grp
sleep 40; echo "== D4: 40 s later"; grp
echo "== total Received lines: $(recvd); distinct: $(cat $X/recv*.log | grep 'Received:' | sort -u | wc -l | tr -d ' ')"
kill $R1 $R2 2>/dev/null; sleep 3; pkill -f 'GreetingsReceiver' 2>/dev/null
dc down -v >/dev/null 2>&1
