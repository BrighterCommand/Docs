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
services.AddBrighter()
    .AddProducers(options =>
    {
        options.ProducerRegistry = new RmqProducerRegistryFactory(...).Create();
    })
    .AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.ChannelFactory = new ChannelFactory(...);
    });
```

### Complete code blocks

- Every fence carries a language tag: ```` ```csharp ````, ```` ```yaml ````,
  ```` ```bash ````, ```` ```text ```` for output dumps. Never a bare ```` ``` ````
- C# blocks carry the `using` directives a reader needs to compile them
- Mark genuine omissions with `// ...`, so an incomplete block is visibly incomplete

See [Code Example Best Practices](#code-example-best-practices) for the fuller
treatment; these three are the parts a tool can check.

### llms.txt

`llms.txt` at the repository root indexes the documentation for retrieval systems.
The format is defined here; Spec 010 owns the generator, which builds it from
`SUMMARY.md` and reads each page's banner for the type — which is why the banner has
to land first.

```
# Brighter and Darker Documentation

> CQRS and Messaging frameworks for .NET. Brighter handles Commands and Events;
> Darker handles Queries.

## Get Started
- [Your First Command](/contents/TutorialFirstCommand.md): Tutorial — send a command in-process, no broker required.

## Transports
- [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md): Reference — connection, publication and subscription parameters.
```

Sections mirror `SUMMARY.md`. Each line is `- [Title](path): Type — one sentence.`

### Enforcement

```bash
python3 tools/pagelint.py                          # whole repo
python3 tools/pagelint.py contents/Glossary.md     # specific pages
python3 tools/pagelint.py --changed origin/master  # strict on changed blocks
```

Exit code is 1 when anything is an error, 0 when clean or warnings only, 2 on bad
arguments — the same contract as `linkcheck.py`, which it runs beside in
`.github/workflows/docs.yml`.

Two strictness levels. Repo-wide, missing `using` directives are a **warning with a
count**, so the existing debt stays visible without blocking unrelated work. On a pull
request, `--changed` makes them an **error for code blocks that overlap your diff** —
block granularity, not file. Fixing a typo on a 700-line page therefore obliges
nothing beyond the typo, which is the point: a rule that makes small corrections
expensive stops people making them.

**The ledger.** Every convention above maps to a rule, and every rule maps back. A
rule in only one of the two places is how the next round of decay begins:

| Convention | Rule | Repo-wide | `--changed` |
|---|---|---|---|
| Banner present as the first non-blank line after the H1 | 1 | error | error |
| Banner matches `BANNER_RE` — type in vocabulary, *Applies to* present | 2 | error | error |
| Heading qualification, across pages (`##`, allowlist exempt) | 3a | error | error |
| Heading qualification, within a page (`##`–`####`, allowlist exempt) | 3b | error | error |
| Language tag on every fence | 4 | warning → error once the backfill lands | error |
| "Dispatcher", not "ServiceActivator" or "Service Activator", in prose | 5 | error | error |
| `using` directives in C# blocks | 6 | warning, counted | error |
| Version markers on code (❌/✅) | — | **review only** | **review only** |

Version markers are the one convention with no rule, and deliberately so: whether two
code blocks differ *by version* is a judgement about meaning, and a regex that guessed
at it would fire on every before/after pair in the repo. It is checked in review.

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
the published docs. It reports four faults:

- **MISSING FILE** — the target does not exist
- **MISSING ANCHOR** — the file exists, but no heading slugifies to the anchor
- **WRONG CASE** — the target exists only under different capitalisation. macOS
  and Windows resolve these; GitBook does not, so they 404 once published
- **ORPHAN** — a page under `contents/` that `SUMMARY.md` never links to

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
