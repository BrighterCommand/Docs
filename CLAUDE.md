# CLAUDE.md - Documentation Project

This file provides guidance to Claude Code when working on the Brighter/Darker documentation.

## Documentation Role

You are a technical documentation writer working on documentation for:

- **Brighter**: A CQRS and Messaging framework for Commands and Events
- **Darker**: A CQRS framework for Queries
- The documentation is in this, the `Docs` repository
- Source code for reference is in `../Brighter` and `../Darker` repositories

## Key Constraints

1. **NEVER modify files in Brighter or Darker repositories** - these are source code repositories
   - **Sole exception (approved 2026-08-03): tutorial sample code.** Documentation
     tutorials may add to or extend `../Brighter/samples/` (and the `../Darker/`
     equivalent) so tutorial code is compiled and kept honest by the source repo's CI.
     This exception is narrow:
     - **Always via a pull request** against the source repository, reviewed as normal.
       Never a direct commit.
     - **Preference order:** reuse an existing sample → extend an existing sample →
       write a new one.
     - **Samples directories only.** `src/`, `tests/`, `docs/adr/`, release notes and
       every other directory remain strictly read-only.
2. **ONLY modify files in the Docs repository** - this is the documentation repository,
   subject to the tutorial-samples exception above
3. Reference source code and ADRs (Architecture Decision Records) in `../Brighter/docs/adr` for understanding features
4. Release notes are in `..Brighter/release_notes.md`
5. Reference source code and README.MD files in `../Darker' for understanding features.
6. **ALWAYS update SUMMARY.md** when creating or reorganizing documentation files
7. **Test all code examples** before finalizing documentation

## Documentation Structure

The Docs repository follows GitBook structure:

- `SUMMARY.md` - Table of contents (must be updated when adding/reorganizing files)
- `contents/` - All documentation markdown files
- Documentation uses GitHub-flavored markdown
- Code examples use C# with syntax highlighting

### File Naming and Organization

**File Naming Conventions:**

- Use PascalCase for file names: `DefaultMessageMappers.md`, `BasicConcepts.md`
- Use descriptive names that match the primary concept or feature
- Avoid abbreviations unless widely understood

**When to Create New Files:**

- Create new files for distinct features or major concepts
- Prefer editing existing files when adding related content
- If a file exceeds ~500 lines, consider splitting into logical sub-topics
- Each file should have a single, clear purpose

**File Organization Pattern:**

Follow this structure within documentation files:

1. Title (H1) - Clear, descriptive
2. **Page banner** - see [Page Conventions](#page-conventions)
3. Introduction - Brief overview (2-3 sentences)
4. Key Concepts, How-to/Usage, Configuration, Best Practices, Common Pitfalls,
   Sample Code - in that order, and **each heading qualified by its subject**:
   write `## Kafka Subscription Configuration`, not `## Configuration`
5. Further Reading - **unqualified**, one of the five allowlisted navigation
   headings

The skeleton and its ordering are unchanged; what changed is that section
headings now carry their subject. `## Configuration` appears on 26 pages, which
makes every one of them a worse search result and every extracted chunk
unattributable. See [Heading qualification](#heading-qualification).

**Tutorial and How-to pages use `## Step N: …` headings in place of step 4's skeleton**, and
that is a convention rather than an exception to be tidied away later. Both are a
sequence a reader executes once, not a reference they consult; Key Concepts /
Configuration / Best Practices imposes a shape that fights the order the reader needs.
The steps take everything else in the pattern — H1, banner, introduction, then
*Further Reading* — and they satisfy [heading qualification](#heading-qualification)
on their own terms, because a step heading names what that step does
(`## Step 4: Wire Up Brighter`) and is unique across pages. This seeks no exemption
from any rule in the [ledger](#the-ledger); it records why these pages look different.
`contents/TutorialFirstCommand.md` is the first of them.

**Extended from tutorials to how-tos on 2026-09-06**, ruled at spec 013's design review
(design §11 Q7). The reason was never about tutorials specifically — it is about a page
a reader *executes*, and a how-to is that shape too. **It does not reach every How-to
page retroactively**: the 53 pages already typed How-to keep their headings, because
requalifying them would move published anchors for no reader benefit. It binds pages
written from here on.

### SUMMARY.md Management

**Critical:** Always update SUMMARY.md when adding or reorganizing documentation.

**Placement Guidelines:**

- Place conceptual docs early in their section
- Follow with how-to guides
- End sections with reference material
- Group related topics together
- Maintain consistent indentation (spaces, not tabs)

**Link Format:**

```markdown
* [Display Text](/contents/FileName.md)
  * [Nested Topic](/contents/NestedTopic.md)
```

**Orphaned Files:** Never create documentation files that aren't linked from SUMMARY.md

### The two kinds of nesting

`SUMMARY.md` nesting does two different jobs, and only one of them absorbs growth. The
distinction matters because **nesting a page changes its published URL** — GitBook derives
the path from the tree, so a page moved under a parent gains a segment and its old URL has
to be redirected.

**Sub-topic nesting** puts detail under the page it elaborates:

```markdown
* [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md)
  * [RabbitMQ Durability: Quorum Queues and Persistence](/contents/RabbitMQDurability.md)
```

It absorbs *detail*. It does nothing about the number of transports.

**Family nesting** puts peers under an overview page for the family:

```markdown
* [Outbox Support](/contents/BrighterOutboxSupport.md)
  * [MSSQL Outbox](/contents/MSSQLOutbox.md)
  * [MySQL Outbox](/contents/MySQLOutbox.md)
```

It absorbs *peers*, and it is the only thing that does. It is why *Outbox and Inbox*
carries **39 pages behind 9 top-level entries** while *Transports* carries 12 pages behind
7 — the outbox family has three parent pages and the transports have none.

**So, for a section that will keep growing:**

1. **Create the family parent page before the family gets big.** Retrofitting one moves a
   URL per page and needs a `.gitbook.yaml` redirect for each. Cheap at zero pages,
   expensive at ten.
2. **A parent must be a real page, not a stub.** A middle navigation layer needs something
   to hang it from; a placeholder that says "choose a transport below" wastes a click on
   every visit.
3. **Do not nest peers a reader chooses between exactly once.** Someone picks one transport
   and never reads the other nine. Nesting those behind a parent adds a click to every
   visit to buy tidiness in a sidebar. That is why the ten transports stay flat and why
   `MAX_TOP_LEVEL_ENTRIES` was raised rather than the family being re-parented.

**`MAX_TOP_LEVEL_ENTRIES` (S2, `tools/urlmap.py`) is an editorial budget, not a platform
limit, and nothing has ever established otherwise.** It was 12 on the stated grounds that
*"this is the number the navigation shows"*; measured 2026-08-27, GitBook's own 182-page
documentation has a widest section of **10**, and its 55-page *create-content* section
holds those 55 behind 10 entries by nesting. There is no evidence of a break at any
number — what the external corpus shows is the habit above, not a ceiling. Raise the
budget when a flat list of peers is the right answer; reach for a family parent when the
list is a dump nobody decided on.

## Page Conventions

Every page under `contents/` follows the conventions below, and
`python3 tools/pagelint.py` checks them — see [Enforcement](#enforcement).

Each one serves two audiences at once. A reader wants to know, before investing in a
page, what kind of page it is and whether it still applies. A retrieval system has
pulled one chunk out of that page and has to attribute it with no surrounding context.
The same banner and the same qualified heading answer both.

### Page banner

Immediately below the H1, separated by one blank line, a single-line blockquote:

```markdown
# RabbitMQ Subscription Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)
```

The separator is ` · ` — space, U+00B7 MIDDLE DOT, space. Not a hyphen, not a pipe.
It is fixed so the pattern can be strict:

```python
APPLIES_TO = ('Brighter V10 and Darker V4', 'Brighter V10', 'Darker V4')

BANNER_RE = re.compile(
    r'^> \*\*(Tutorial|How-to|Reference|Explanation)\*\*'      # page type
    r' · Applies to \*\*(' + '|'.join(APPLIES_TO) + r')\*\*'   # longest first
    r'( · Prerequisites: .+)?$'                                 # optional
)
```

**Brighter and Darker version independently.** Brighter is on V10; Darker's latest
release is 4.1.1. There is no "Darker V10" and there never has been — an earlier draft
of this vocabulary said there was, which is exactly the confident-but-wrong version
claim the banner exists to prevent. A page covering both spells out both:
`Applies to **Brighter V10 and Darker V4**`.

The vocabulary is closed rather than a version pattern, deliberately. When Brighter
goes to V11 or Darker ships its next release, every unbumped page fails the build
instead of quietly asserting last year's version. Change the versions in
`APPLIES_TO` in `tools/pagelint.py` and nowhere else — this section documents that
tuple and `apply_banners.py` imports it.

**Page type** is exactly one of four values:

| Type | Use when the page… |
|---|---|
| `Tutorial` | teaches by having the reader build something that works, start to finish |
| `How-to` | gets a reader with a stated goal from A to B, assuming they know why |
| `Reference` | describes the machinery — parameters, options, behaviour — and is consulted rather than read through |
| `Explanation` | explains why something works as it does; there are no steps to follow |

**`Reference` is the catch-all.** Some pages sit legitimately outside Diátaxis rather
than being confused about their mode — `FAQ.md`, `Glossary.md` and
`V10MigrationGuide.md` are the clear cases. They are material you *consult*, which is
what Reference means at its widest, and they are not split candidates.

A fifth value for "none of the above" was considered and rejected: such a bucket
collects everything ambiguous and dissolves the discipline the vocabulary exists to
create. A page that cannot pick a type is usually a page that needs splitting, and
that failure to choose is a useful signal rather than something to absorb. Where a
page's type is genuinely contested, the argument belongs in review, not in the
vocabulary.

**Applies to** is required — `Brighter V10`, `Darker V10`, or `Brighter and Darker V10`.
Marking the version in body text is deliberate: pre-V10 Brighter is well represented in
blog posts and Stack Overflow answers, so a model with no counter-evidence in context
will confidently emit configuration that no longer compiles. The banner is that
counter-evidence, and it survives the front-matter stripping that retrieval chunkers
apply.

**Prerequisites** is optional. Omit the segment entirely when there are none; never
write `Prerequisites: none`. Its links are ordinary internal links, so `linkcheck.py`
checks them like any other.

Worked examples:

| Page | Banner |
|---|---|
| A tutorial | `> **Tutorial** · Applies to **Brighter V10**` |
| `RabbitMQConfiguration.md` | `> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)` |
| `ReactorAndProactor.md` | `> **Explanation** · Applies to **Brighter V10**` |
| `CQRSWithBrighterAndDarker.md` | `> **Explanation** · Applies to **Brighter and Darker V10**` |
| `FAQ.md` | `> **Reference** · Applies to **Brighter V10**` |

### Heading qualification

Section headings carry their subject. Write `## Kafka Subscription Configuration`,
not `## Configuration`.

An unqualified heading fails the reader twice: as a search result it says nothing, and
as an extracted chunk it cannot be traced back to the page it came from. It also
collides — `## Configuration` and `## Best Practices` each appear on 26 pages, and
GitBook resolves such collisions into `#configuration`, `#configuration-1`,
`#configuration-2`, anchors no author would choose and no reader can interpret.

Derive the qualifier from the page's subject — its H1, with filler removed:

| Page | H1 | Heading | Becomes |
|---|---|---|---|
| `KafkaConfiguration.md` | Kafka Configuration | `## Configuration` | `## Kafka Configuration` |
| `HangfireScheduler.md` | Hangfire Scheduler | `## Best Practices` | `## Hangfire Best Practices` |
| `MsSqlDistributedLock.md` | MSSQL Distributed Lock | `## Provisioning` | `## MSSQL Distributed Lock Provisioning` |

Where that reads badly, shorten it by hand. `## Hangfire Best Practices` is better
than `## Hangfire Scheduler Best Practices`, and no rule can tell you that.

**Two scopes, deliberately different:**

- **Across pages, `##` only.** H3 headings may repeat across pages, and usually
  should: an H3 is read under its H2, so `### Basic Configuration` beneath
  `## Hangfire Scheduler Configuration` is perfectly attributable. Forcing global
  uniqueness on subsections produces long, stilted headings for no gain.
- **Within one page, `##` through `####`.** Here a duplicate is always a defect.

**The navigation allowlist** — exactly these five heading texts are exempt from
uniqueness, and **must stay uniform**:

```
Further Reading
Related Documentation
See Also
Next Steps
References
```

Their repetition is the point: it makes the end of every page predictable. This list
is canonical — `tools/pagelint.py` and this file must quote it identically. Do not
qualify them, and do not add to the list without changing both places.

### Version markers on code

Where V9 and V10 differ, show both forms and label them: ❌ for the superseded form,
✅ for the current one. Putting the discrimination in the text is more reliable than
trusting a reader — or a model — to already know which era they are looking at.

❌ **V9 — superseded**

```csharp
services.AddBrighter()
    .UseExternalBus(new RmqProducerRegistryFactory(...).Create())
    .AddServiceActivator(options =>
    {
        options.Subscriptions = subscriptions;
        options.ChannelFactory = new ChannelFactory(...);
    });
```

✅ **V10 — current**

```csharp
services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(...);
    })
    .AddProducers(options =>
    {
        options.ProducerRegistry = new RmqProducerRegistryFactory(...).Create();
    });
```

**The receiver matters and the compiler is the only thing that says so.** `AddConsumers`
extends `IServiceCollection`; `AddProducers`, `UseScheduler`, `Handlers` and
`AutoFromAssemblies` extend the `IBrighterBuilder` it returns. So a consumer registration comes
**first** and everything else chains off it —
`services.AddBrighter().AddProducers(…).AddConsumers(…)` is **`CS1929`**, and an earlier
version of this example had it. `AddBrighter` is the entry point when there are no consumers.

### Complete code blocks

- Every fence carries a language tag: ```` ```csharp ````, ```` ```yaml ````,
  ```` ```bash ````, ```` ```text ```` for output dumps. Never a bare ```` ``` ````
- C# blocks carry the `using` directives a reader needs to compile them
- Mark genuine omissions with `// ...`, so an incomplete block is visibly incomplete

See [Code Example Best Practices](#code-example-best-practices) for the fuller
treatment; these three are the parts a tool can check.

The third qualifies the second. A block whose `using` directives are genuinely
elided says so with `// ...`, and rule 6 then reports it as a **warning even under
`--changed`** — the omission is declared rather than fixed, so it is downgraded and
never silenced. It still counts towards the debt and still says so in its own words.
Without that, relocating a block verbatim is indistinguishable from writing a new
one, and a page split cannot honour "move the text, do not improve it".

### The opening sentence

Every page's first sentence after the banner has to survive being read on its own.

That sentence is not decoration and it is not only for a reader who has already
arrived. It is what GitBook's `.md` variant leads with after the banner, what the
canonical `/llms.txt` prints after the page's title, and what a search engine shows
as the page's snippet — all from the same string. A reader meets it before they meet
the page.

So it must exist, be **at most 200 characters as rendered**, end in terminal
punctuation, not end in a colon, and not be identical to another page's.

**Rendered, not as typed.** `[Configuration](/contents/BrighterBasicConfiguration.md)`
is fourteen characters to a reader and fifty-four in the source. Measuring the source
fails pages on the length of a URL nobody reads — seven of the eleven pages over the
limit on the first run were over it only in that sense.

**Uniqueness is the clause that earns its keep.** Two pages introducing themselves
with the same sentence are indistinguishable in an index, and the duplicate is almost
always a copied intro nobody rewrote. On this rule's first run it found
`AzureBlobConfiguration.md` — the *Azure Blob* archive provider page — opening with a
paragraph about *Azure Service Bus*, wrong since 2023 and green under every other
check here, because nothing about it was malformed.

**What it cannot do**, and this is the point of the three clauses it does check: it
cannot tell whether a sentence is *true*. It checks the properties that correlate
with a sentence nobody reread.

### Page descriptions

A page carries its summary to retrieval clients in YAML front matter, above the H1:

```markdown
---
description: "The Azure Archive Provider writes messages swept from your Outbox into an Azure Blob Storage container."
layout:
  description:
    visible: false
---
```

**`description:` is the opening sentence with its markdown stripped**, so the two
cannot drift — a parallel table of hand-written summaries would be stale within a
release, and a sentence that lives in the page cannot be.

**Always quote the value.** GitBook documents that an unquoted `description:`
containing `:`, `#`, `[`, `]`, `{`, `}`, `&`, `*`, `!`, `|`, `>`, `'`, `"`, `%`, `@`
or `` ` `` causes a **silent** Git Sync failure — the page imports with no title and
no description, and no error anywhere. Sentences carry colons. Quote every value.

**`layout.description.visible: false` is not optional.** It defaults to *visible*,
and GitBook then renders the description as a subtitle under the H1 — so a page whose
description was derived from its own opening sentence would print that sentence
twice. Measured on a live preview revision: the switch takes the subtitle to zero
while all three meta tags stay.

**Front matter does not carry the banner, and that has not changed.** The banner stays
a visible blockquote, because a retrieval chunker strips front matter and keeps body
text. What did change is the reason that used to sit beside it: *"GitBook renders
front matter literally into the page body"* is false — front matter is consumed and
the H1 survives. One reason of two was wrong for a year, and the conclusion it
supported was right anyway.

### llms.txt

**`/llms.txt` is GitBook's, not ours.** The platform builds it from the published tree,
serves it at the space root, and there is no way to override it — a file written in this
repository would live on GitHub only and compete with the canonical one. So this section
describes a file to *feed*, not one to generate. Measured live 2026-08-22:

```text
# Paramore Brighter Documentation

## Paramore Brighter Documentation
- [Basic Concepts](https://brightercommand.gitbook.io/paramore-brighter-documentation/get-started/basicconcepts.md): A command is an instruction to carry out work.

## V9 Paramore Brighter Documentation
- [Basic Concepts](https://brightercommand.gitbook.io/paramore-brighter-documentation/v9-paramore-brighter-documentation/overview/basicconcepts.md): Brighter V9 (superseded). A command is an instruction to carry out work.

# Agent Instructions
## Querying This Documentation
```

**56,527 bytes, 201 entries, and all 201 carry a description** — 143 from this space and
58 from `v9`. The trailing *Agent Instructions* section is the platform's own, and it
documents a query endpoint worth knowing about: `GET <page>.md?ask=<question>` answers a
natural-language question against the page.

**One line per page: `- [Title](url): description`.** There is **no type field**, and none
should be added: the type reaches a reader through the banner, which the `.md` variant
prints one line below the H1, so a prefix would only spend the snippet repeating what the
page is about to say. AC9 was narrowed to match this, deliberately.

**Three parts, three different sources, and they are easy to misremember:**

| Part | Comes from | Evidence |
|---|---|---|
| The **URL slug** | the **filename** — falling back to the `SUMMARY.md` title only when no file exists | Phase 10: repointing an entry at a real file *moved* the URL. The seven fileless entries carried title-shaped slugs; the fifty-one real pages filename-shaped ones |
| The **title** | the **`SUMMARY.md` entry text**, not the page's H1 | 32 pages spell the two differently — `AWS SNS and SQS Configuration` against an H1 of `AWS SQS Configuration` — and the index uses the `SUMMARY.md` text on all 32 |
| The **description** | the page's `description:` front matter, which is [its opening sentence](#the-opening-sentence) with the markdown stripped | 201 of 201 |

So the way to change a page's line in the index is to change its `SUMMARY.md` entry or its
opening sentence. **Nothing here is generated by a tool in this repository**, and the one
that was designed — `tools/llmstxt.py` — was cancelled once the platform was measured
rather than assumed.

**The `##` groups are GitBook *spaces*, not our information architecture.** The twelve
sections of `SUMMARY.md` do not reach this file; a reader of the index sees two groups,
one per space. That is the platform's model and it is not configurable, so the tree this
repository builds is for readers and for navigation, not for the index.

**A V9 description is the one place a marker belongs.** Those pages carry no banner, so
their version is recoverable from nothing — which is why all 58 `v9` descriptions open
with the literal prefix **`Brighter V9 (superseded).`**, and why
`layout.description.visible: false` is deliberately **omitted** on that branch: rendering
the description is the only way the marker reaches a reader who arrives from a search
engine rather than the index.

### Enforcement

```bash
python3 tools/pagelint.py                          # whole repo
python3 tools/pagelint.py contents/Glossary.md     # specific pages
python3 tools/pagelint.py --changed origin/master  # strict on changed blocks
python3 tools/pagelint.py --fix                    # repair, then report what is left
```

Exit code is 1 when anything is an error, 0 when clean or warnings only, 2 on bad
arguments — the same contract as `linkcheck.py`, which it runs beside in
`.github/workflows/docs.yml`.

**Scope is `contents/` plus `README.md`, which is 143 pages.** The README is not a
documentation page — it is the site root — but GitBook publishes it, and its
`description:` reaches a reader through `/llms.txt` exactly like any other page's. It
therefore takes **rule 7 and nothing else**: a root page carries no banner, so rules 1
and 2 would be asking it to declare a page type it does not have, and it sits outside
heading qualification, whose purpose — attributing a chunk to the page it came from — is
circular for the site root. It is excluded from the cross-page uniqueness *corpus* too,
so no page can be reported as colliding with a page that is exempt.

That boundary is load-bearing rather than tidy. The reason rule 7 could not simply be
pointed at `README.md` is that the opening-sentence extractor **skips the line after the
H1**, which on every other page is the banner. On the README that line *is* the opening
sentence, so the tool would have summarised the site root with its second paragraph and
then reported the front matter — correct front matter — as a mismatch.

Two strictness levels. Repo-wide, missing `using` directives are a **warning with a
count**, so the existing debt stays visible without blocking unrelated work. On a pull
request, `--changed` makes them an **error for code blocks that overlap your diff** —
block granularity, not file. Fixing a typo on a 700-line page therefore obliges
nothing beyond the typo, which is the point: a rule that makes small corrections
expensive stops people making them.

**The ledger.** Every convention above maps to a rule, and every rule maps back. A
rule in only one of the two places is how the next round of decay begins — and the
`NO H1` row is there because the acceptance pass found it missing, which is the exact
failure the claim in this paragraph is meant to prevent:

| Convention | Rule | Repo-wide | `--changed` |
|---|---|---|---|
| One H1 per file, before the banner | 1's precondition (`NO H1`) | error | error |
| …and no *second* H1 | 1's precondition (`EXTRA H1`) | error | error |
| Banner present as the first non-blank line after the H1 | 1 | error | error |
| Banner matches `BANNER_RE` — type in vocabulary, *Applies to* present | 2 | error | error |
| Heading qualification, across pages (`##`, allowlist exempt) | 3a | error | error |
| Heading qualification, within a page (`##`–`####`, allowlist exempt) | 3b | error | error |
| Language tag on every fence | 4 | error | error |
| "Dispatcher", not "ServiceActivator" or "Service Activator", in prose | 5 | error | error |
| `using` directives in C# blocks | 6 | warning, counted | error, unless the block marks its omission `// ...` |
| An opening sentence exists | 7 (`SUMMARY MISSING`) | error | error |
| It is ≤ 200 characters **rendered** | 7 (`SUMMARY TOO LONG`) | error | error |
| It does not end in a colon | 7 (`SUMMARY ENDS IN COLON`) | error | error |
| It ends in terminal punctuation | 7 (`SUMMARY NOT A SENTENCE`) | error | error |
| It is unique across pages | 7 (`SUMMARY NOT UNIQUE`) | error | error |
| `description:` front matter equals it | 7 (`DESCRIPTION MISMATCH`) | error | error |
| That front matter is a quoted single line | 7 (`DESCRIPTION UNREADABLE`) | error | error |
| Version markers on code (❌/✅) | — | **review only** | **review only** |

Version markers are the one convention with no rule, and deliberately so: whether two
code blocks differ *by version* is a judgement about meaning, and a regex that guessed
at it would fire on every before/after pair in the repo. It is checked in review.

**`--fix` repairs three of these and refuses the rest.** It retargets a banner whose
*Applies to* is stale against `APPLIES_TO` — which is what makes a version bump one
edit to that tuple plus one command — tags an untagged fence ```` ```text ```` when
nothing in the block looks like code, and writes a page's `description:` front matter
from its own opening sentence. It rewrites only the version segment, so
the page type and any Prerequisites are out of its reach by construction.

The description repair is what swept all 142 pages, and it refuses in two cases rather
than guessing. It refuses when the sentence it would copy **fails rule 7**, so `--fix`
cannot manufacture a description the linter then rejects — nor, worse, one it accepts.
And it refuses when **the H1 is not on line 1**: GitBook reads front matter only at the
very first byte of a file, so a block written under a leading blank line is not front
matter at all, and the page simply has no description with nothing to say so.

It **never decides a page type.** That is a judgement about what a page is *for*, it
cannot be recovered from the text, and a wrong one is invisible: a page mislabelled
`Reference` reads perfectly and misleads everyone who trusted the label. A banner with
an out-of-vocabulary type gets its version fixed and still fails rule 2, which is the
intended outcome — `--fix` cannot turn a bad page type into a green build.

Where the answer is not unique it says so and changes nothing: a version naming a
product no single `APPLIES_TO` entry covers, and a fence holding anything code-shaped,
where choosing between `csharp`, `bash`, `json` and `yaml` belongs to whoever wrote it.

Rule 5 matches **both spellings** — `ServiceActivator` and `Service Activator`. The
API surface uses the closed form, but prose here uses the open one just as often, and
both are the same V9 term.

It has three legitimate exceptions built in — the term is fine inside fenced blocks and
inline code spans (`ServiceActivatorHostedService`,
`Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection`), and a page that
discusses the name itself opts out with a comment:

```markdown
<!-- pagelint: allow-serviceactivator -->
```

## Documentation Goals

### Primary Objectives

1. Make documentation accessible to newcomers
2. Provide depth for experienced users

## Working Process

1. Read release notes and ADRs to understand features
2. Review existing documentation structure via SUMMARY.md
3. Create requirements document for review
4. Break work into discrete tasks
5. Execute tasks systematically
6. Always reference code examples from Brighter/Darker when needed

## Documentation Standards

- Use clear, concise language
- Provide code examples for all features
- Include configuration examples
- Explain both "what" and "why"
- Link related concepts
- Use consistent terminology throughout
- Follow existing documentation patterns in the Docs repository

## Questions to Ask

When unclear about a feature:

- How does this feature benefit users?
- What problem does it solve?
- What are the trade-offs?
- What are common use cases?
- Are there any gotchas or important notes?

## Markdown and Linking Standards

### Cross-Linking Documentation

**Verifying links:** `python3 tools/linkcheck.py` checks every internal link in
the published docs. It reports six faults:

- **MISSING FILE** — the target does not exist
- **MISSING ANCHOR** — the file exists, but no heading slugifies to the anchor
- **WRONG CASE** — the target exists only under different capitalisation. macOS
  and Windows resolve these; GitBook does not, so they 404 once published
- **LEGACY HTML** — a link to a `.html` path. The published site serves none:
  every page has an extensionless URL. These are survivals from the pre-GitBook
  site, and until Spec 010 Phase 11 the checker *skipped* them for not ending
  in `.md` — so 23 of them, one naming a page that has never existed, were
  green in every run it had ever made
- **EMPTY TARGET** — a link written `[text](#)`, which goes nowhere. It reaches
  a reader as a live link and reached the checker as a same-page link with no
  anchor to look up
- **ORPHAN** — a page under `contents/` that `SUMMARY.md` never links to

**A link's text may wrap across lines**, and the checker reads whole files rather
than lines so that it sees one when it does. Extracted per line, 27 links in this
corpus were invisible to it — including that dead one. Report a fault at the line
carrying the **target**, which is where you fix it, not where the text opens.

Pass file paths to check just those files; orphans are only reported on a
whole-repo run, since that check needs every page in view. It exits non-zero
when anything is broken, so it can gate CI. Run it after any change that adds or
retargets links, **and after adding a page** — the orphan check is what enforces
the "never create orphaned files" rule below.

**Internal Links (to other docs):**

```markdown
[Link Text](/contents/RelativeFileName.md)
[Link to Section](/contents/FileName.md#section-anchor)
```

**Anchors:**

- Use kebab-case for anchors: `#default-message-mappers`
- Reference existing glossary terms: `[command](#command)` in BasicConcepts.md

**External Links (source code references):**

```markdown
Reference code: `Brighter/src/Paramore.Brighter/CommandProcessor.cs:123`
Reference ADRs: `Brighter/docs/adr/0015-default-message-mappers.md`
Reference samples: `Brighter/samples/WebAPI/GreetingsApp/`
```

**Best Practices:**

- Use relative paths, not absolute paths
- Link to concepts on first mention in a document
- Create cross-references to related topics in "Further Reading" sections
- Don't duplicate content - link to the authoritative source

### Markdown Formatting

**Headings:**

- One H1 per file (the title)
- Use H2 for major sections
- Use H3-H4 for subsections
- Make headings descriptive and scannable

**Code Blocks:**

Always specify language for syntax highlighting:

````markdown
```csharp
public class OrderCreated : IRequest
{
    public string Id { get; set; }
}
```
````

**Emphasis:**

- Use **bold** for UI elements, important warnings, key terms on first use
- Use *italics* for emphasis or when defining terms
- Use `code` for code elements, file names, class names, methods

**Lists:**

- Use `-` for unordered lists (consistent with existing docs)
- Use `1.` for ordered lists
- Use consistent indentation (2 spaces)

## Code Example Best Practices

### Writing Code Examples

**Complete and Runnable:**

```csharp
// Good - Complete example with necessary context
public class GreetingCommandHandler : RequestHandler<GreetingCommand>
{
    public override GreetingCommand Handle(GreetingCommand command)
    {
        Console.WriteLine($"Hello {command.Name}");
        return command;
    }
}
```

**Use Comments for Omitted Code:**

```csharp
services.AddBrighter(options =>
{
    // ... other configuration
})
.AutoFromAssemblies([typeof(OrderCreated).Assembly]);
```

**Configuration with Usage:**
Always show configuration alongside usage:

```csharp
// Configuration
services.AddBrighter(options => { })
    .AddProducers(configure => { /* ... */ });

// Usage
await _commandProcessor.PublishAsync(new OrderCreated());
```

**Using Statements:**

Include when necessary for clarity:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
```

### Code Example Standards

1. **Test all code examples** - Ensure they compile and run
2. **Reference working samples** - Link to `Brighter/samples/` where applicable
3. **Show realistic examples** - Use domain-appropriate examples (Orders, Customers, etc.)
4. **Explain key points** - Add comments or explanatory text for complex code
5. **Avoid deprecated patterns** - Use V10 patterns, not V9 legacy code
6. **Include error handling** - Show proper exception handling where relevant

## Documentation Types and Styles

### Conceptual Documentation

**Purpose:** Explain "what" and "why"
**Example:** `BasicConcepts.md`, `EventDrivenCollaboration.md`
**Structure:**

- Define the concept clearly
- Explain why it matters
- Provide context and use cases
- Link to how-to guides for implementation

### How-To Guides

**Purpose:** Step-by-step instructions
**Example:** `DefaultMessageMappers.md`, `BrighterBasicConfiguration.md`
**Structure:**

- Clear goal statement
- Prerequisites
- Numbered steps with code examples
- Expected outcomes
- Troubleshooting common issues

### Reference Documentation

**Purpose:** Complete technical details
**Example:** `RabbitMQConfiguration.md`, `AWSSQSConfiguration.md`
**Structure:**

- Comprehensive parameter lists
- Configuration options with descriptions
- Default values
- Examples of common configurations

### Tutorials

**Purpose:** End-to-end learning
**Example:** `ShowMeTheCode.md`
**Structure:**

- Clear learning objectives
- Progressive complexity
- Working code examples
- Explanation of each step

## Style Guide

### Voice and Tone

**Voice:**

- Use second person ("you") when addressing the reader
- Use active voice: "Brighter provides" not "is provided by Brighter"
- Use present tense: "the handler receives" not "the handler will receive"

**Tone:**

- Professional but approachable
- Helpful, not condescending
- Confident but not absolute (acknowledge trade-offs)
- Direct and concise

**Examples:**

```markdown
✅ Good: "You can configure default message mappers to simplify your code."
❌ Bad: "One might configure default message mappers if one wishes to simplify."

✅ Good: "The Command Processor dispatches requests to handlers."
❌ Bad: "Requests are dispatched to handlers by the Command Processor."

✅ Good: "Use binary-mode CloudEvents for protocols with header support."
❌ Bad: "Binary-mode CloudEvents MUST ALWAYS be used."
```

### Audience Considerations

Write for varying skill levels:

- **Beginners:** Explain concepts, define terms, provide context
- **Intermediate:** Show patterns, explain trade-offs
- **Advanced:** Reference advanced scenarios, link to source code and ADRs

**Handling Jargon:**

- Define technical terms on first use
- Link to glossary for key concepts
- Provide examples to illustrate abstract concepts
- Don't assume knowledge of message-oriented middleware

### Content Guidelines

**Length:**

- Keep sentences clear and concise
- Break long paragraphs into shorter ones (3-5 lines)
- Use subheadings to break up long sections
- If content exceeds 500 lines, consider splitting

**Clarity:**

- One concept per paragraph
- Use concrete examples over abstract explanations
- Provide both "what" and "why"
- Use diagrams where appropriate (though not required)

## Source Code Integration

### Finding Relevant Code

**In Brighter Repository:**

- Source code: `Brighter/src/Paramore.Brighter/`
- Tests: `Brighter/tests/Paramore.Brighter.Tests/`
- Samples: `Brighter/samples/`
- ADRs: `Brighter/docs/adr/`
- Release notes: `Brighter/release_notes.md`

**In Darker Repository:**

- Source code: `Darker/src/Paramore.Darker/`
- Tests: `Darker/tests/Paramore.Darker.Tests/`

### Referencing Source Code

**Reference Format:**

```markdown
The CommandProcessor implementation can be found in `Brighter/src/Paramore.Brighter/CommandProcessor.cs:123`
```

**When to Reference Code:**

- For advanced users wanting to understand internals
- When explaining complex behavior
- For troubleshooting or debugging guidance

**When to Link ADRs:**

```markdown
For the design rationale behind default message mappers, see [ADR-0015](Brighter/docs/adr/0015-default-message-mappers.md).
```

**Using Sample Code:**
Always reference working samples:

```markdown
Full working examples can be found in the Brighter samples:
- **Default Mappers**: `Brighter/samples/WebAPI/`
- **ClaimCheck Transform**: `Brighter/samples/Transforms/ClaimCheck/`
```

## Quality Assurance Checklist

Before finalizing documentation, verify:

**Code Quality:**

- [ ] All code examples compile and run
- [ ] Code examples follow V10 patterns
- [ ] Examples reference real samples where applicable
- [ ] Using statements included where necessary
- [ ] Code uses proper syntax highlighting (```csharp)

**Content Quality:**

- [ ] Explains both "what" and "why"
- [ ] Provides concrete examples
- [ ] Uses consistent terminology (check Glossary)
- [ ] Appropriate for target audience
- [ ] No spelling or grammar errors

**Structure:**

- [ ] SUMMARY.md updated with any new files
- [ ] All cross-links work (no broken links) — verify with `python3 tools/linkcheck.py`
- [ ] Page banner present, with a page type you would defend — verify with
      `python3 tools/pagelint.py <path>`
- [ ] Every `##` heading qualified by its subject, except the navigation allowlist
- [ ] File follows standard organization pattern
- [ ] Headings are logical and scannable
- [ ] Further Reading section includes relevant links

**Technical Accuracy:**

- [ ] Information verified against source code/ADRs
- [ ] Release notes consulted for V10 features
- [ ] Configuration examples tested
- [ ] Deprecated features marked

## Common Pitfalls to Avoid

**Content Pitfalls:**

- ❌ Don't duplicate content across files - link to authoritative source
- ❌ Don't create orphaned files not linked from SUMMARY.md
- ❌ Don't use absolute paths for links - use relative paths
- ❌ Don't skip explaining "why" - explain both what and why
- ❌ Don't forget to test code examples
- ❌ Don't mix V9 and V10 patterns without clear distinction
- ❌ Don't assume reader knowledge - define terms and provide context

**Structure Pitfalls:**

- ❌ Don't create deeply nested subsections (limit to H4)
- ❌ Don't write walls of text - break into scannable sections
- ❌ Don't forget the "Further Reading" section
- ❌ Don't skip cross-linking related concepts

**Code Pitfalls:**

- ❌ Don't show incomplete code without indicating omissions (`// ...`)
- ❌ Don't use deprecated V9 patterns in new docs
- ❌ Don't forget syntax highlighting on code blocks
- ❌ Don't mix configuration and usage without separating them

**Terminology Pitfalls:**

- ❌ Don't use "ServiceActivator" or "Service Activator" - prefer "Dispatcher" for V10.
  If you mean the assembly or a type, put it in `backticks`, not **bold**
- ❌ Don't use inconsistent terms - check BasicConcepts.md and Glossary
- ❌ Don't introduce new terms without defining them

## GitBook Publishing Notes

**Rendering:**

- GitBook renders GitHub-flavored markdown
- Preview locally to check formatting
- Code blocks render with syntax highlighting
- Internal links resolve correctly if paths are relative

**Limitations:**

- Some GitHub markdown extensions may not work
- Complex HTML may not render correctly
- Keep to standard markdown for best compatibility

**Publishing Process:**

- Documentation syncs from GitHub to GitBook
- Changes pushed to main branch are published automatically
- Review changes in GitHub before pushing to main
- SUMMARY.md determines site navigation structure
