# Kafka offsets at Brighter 10.8.0 (TutorialStreamingWithKafka.md, Step 8)

Run from a directory holding a replay of rungs 2–4: `env.sh` exporting `S` (the replay root) and
the four NuGet cache variables, with rung 4's projects built under `$S/rung4`.
`kafka-override.yml` renames the broker container to `replay234-kafka`, because the page's
`container_name: kafka` collides with Brighter's own compose stack.

- `k8.sh after`: the page's order, with the rebalance after the 30 s sweep. Control: 0 re-handled
- `k8.sh within`: the rebalance inside the sweep window. Control: 0 re-handled, `3 6 3` then `6 6 0`
- `k8kill.sh`: a receiver killed with SIGKILL inside the window. Case: all 9 re-handled
