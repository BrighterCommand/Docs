#!/bin/zsh
# Case: the receiver is killed (SIGKILL) inside the 30 s sweep window, then a fresh receiver starts. Expect re-delivery.
source ${0:A:h}/env.sh
cd $S/rung4
X=$S/k8-kill; rm -rf $X; mkdir -p $X
dc() { docker compose -f docker-compose-kafka.yml -f $S/kafka-override.yml "$@"; }
grp() { docker exec replay234-kafka /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server localhost:9092 --describe --group greeting.readers 2>&1 | grep -E '^greeting.readers' | awk '{print $3, $4, $5, $6}'; }
dc down -v >/dev/null 2>&1; dc up -d >/dev/null 2>&1
for i in $(seq 1 60); do docker exec replay234-kafka /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server localhost:9092 >/dev/null 2>&1 && break; sleep 2; done
dotnet build GreetingsReceiver -o $X/bin >/dev/null 2>&1
dotnet $X/bin/GreetingsReceiver.dll > $X/recv1.log 2>&1 & R1=$!
for i in $(seq 1 90); do grep -q 'Partition Added' $X/recv1.log && break; sleep 1; done
dotnet run --no-build --project GreetingsSender > $X/send1.log 2>&1
for i in $(seq 1 60); do [ $(grep -c 'Received:' $X/recv1.log) -ge 9 ] && break; sleep 1; done
echo "recv1 handled: $(grep -c 'Received:' $X/recv1.log)"; kill -9 $R1; echo "killed recv1 (SIGKILL)"; grp
sleep 50   # past the broker's session timeout, so the dead member's partitions are reassigned
dotnet $X/bin/GreetingsReceiver.dll > $X/recv2.log 2>&1 & R2=$!
for i in $(seq 1 90); do grep -q 'Partition Added' $X/recv2.log && break; sleep 1; done; sleep 10
echo "recv2 handled again: $(grep -c 'Received:' $X/recv2.log)"; grp
kill $R2; sleep 2; dc down -v >/dev/null 2>&1
