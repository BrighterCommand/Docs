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
2. **ONLY modify files in the Docs repository** - this is the documentation repository
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
2. Introduction - Brief overview (2-3 sentences)
3. Key Concepts - What users need to understand
4. How-to/Usage - Practical examples with code
5. Configuration - Detailed configuration options
6. Best Practices - Recommendations and patterns
7. Common Pitfalls - What to avoid
8. Further Reading - Links to related topics
9. Sample Code - References to working examples in Brighter/samples/

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
- [ ] All cross-links work (no broken links)
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

- ❌ Don't use "ServiceActivator" - prefer "Dispatcher" for V10
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
