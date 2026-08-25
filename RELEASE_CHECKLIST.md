# Documentation Release Checklist

> **Reference** · Applies to **Brighter V10**

Run this on every Brighter release, before the docs are re-published. It is the maintainer's
list, not a published page — it is deliberately absent from `SUMMARY.md`.

`tools/versioncheck.py` proves the *numbers* on the tutorial pages are current. Only running
a tutorial proves the *code* still works. That gap is real: the samples reference Brighter by
`ProjectReference` into `../../../src/`, so Brighter's CI compiles them against tip-of-tree
source and never against the released package a reader installs. This checklist is what
closes it.

## On Every Brighter Release

1. **Run `python3 tools/versioncheck.py`. Expect it to FAIL — that failure is the signal.**
   It exits 1 and names every file, line and package pinned behind the new release. Exit 2
   means it could not reach NuGet, which is not a pass: retry, or pass
   `--release-notes ../Brighter/release_notes.md` to check against a local checkout.
2. **Update the pinned versions it names**, in both the `dotnet add package` line and the
   `.csproj` block on each page.
3. **Run each shipped tutorial end to end on a clean machine** — see the definition below,
   and record the result in the table.
4. **Re-time each one.** If the stated duration has drifted, change the page. A tutorial that
   claims ten minutes and takes forty is a defect a reader meets before anything else.
5. **Run `python3 tools/linkcheck.py` and `python3 tools/pagelint.py`.**

**What this does not cover.** `versioncheck.py` reads only lines naming a
`Paramore.Brighter*` package, by design — a broader match would start policing pages that
quote an old version deliberately. Non-Brighter pins on a tutorial page
(`Microsoft.Extensions.Hosting`, the .NET SDK version, a Docker image tag) are step 3's job,
not the gate's: you meet them when you run the tutorial.

## The Clean-Machine Definition

A fresh clone, an empty NuGet cache, and no Docker volumes left from a previous run.

**"Empty NuGet cache" means four locations, not one.** `NUGET_PACKAGES` moves the global
packages folder and nothing else, so a run that redirects only that variable still reads
downloaded bytes from the machine's HTTP cache and reports a warm restore as cold. Redirect
all four, and verify with `dotnet nuget locals all --list` **before** the run:

```bash
export NUGET_PACKAGES="$SCRATCH/packages"
export NUGET_HTTP_CACHE_PATH="$SCRATCH/http-cache"
export NUGET_SCRATCH="$SCRATCH/scratch"
export NUGET_PLUGINS_CACHE_PATH="$SCRATCH/plugins"
dotnet nuget locals all --list   # every path must be under $SCRATCH, and empty
```

**Afterwards, assert the HTTP cache filled.** An isolated cache that is still empty and a
cache that was bypassed look identical from the outside; files appearing under
`$NUGET_HTTP_CACHE_PATH` is the only positive evidence that bytes crossed the network.

## The Tutorial Run Table

A blank or stale cell is the honest signal that a tutorial is unverified — which is a worse
state than not having the tutorial at all. Add a row when a rung ships.

| Rung | Page | Sample | Against | Last run | Machine time |
|---|---|---|---|---|---|
| 1 | `contents/TutorialFirstCommand.md` | `samples/CommandProcessor/HelloWorld` | 10.7.0 | 2026-08-25 (`faeac68`) | 11s |
| 2 | `contents/TutorialFirstMessage.md` | `samples/Tutorials/02-FirstMessage` | — | not yet written | — |
| 3 | `contents/TutorialDurableOutbox.md` | `samples/Tutorials/03-DurableOutbox` | — | not yet written | — |
| 4 | `contents/TutorialStreamingWithKafka.md` | `samples/Tutorials/04-Kafka` | — | not yet written | — |

**Machine time is create-restore-build-run**, not the duration the page quotes a reader. The
page's figure is mostly reading and typing; this column is the part a slow feed or a broken
sample would move.

## When a Tutorial Fails

**It is a release blocker for the docs, not a backlog item.** A tutorial that does not run is
worse than no tutorial: it costs a newcomer their first hour and their confidence in
everything else here.

If it cannot be fixed before the docs are re-published, unlink the rung from
`contents/GetStarted.md` and ship the prefix of the ladder that still works. A two-rung
ladder is a coherent published state; a four-rung ladder with a broken third rung is not.
