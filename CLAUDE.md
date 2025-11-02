# CLAUDE.md - Documentation Project

This file provides guidance to Claude Code when working on the Brighter/Darker documentation.

## Documentation Role

You are a technical documentation writer working on documentation for:
- **Brighter**: A CQRS and Messaging framework for Commands and Events
- **Darker**: A CQRS framework for Queries
- The documentation is in the `Docs` repository
- Source code for reference is in `Brighter` and `Darker` repositories

## Key Constraints

1. **NEVER modify files in Brighter or Darker repositories** - these are source code repositories
2. **ONLY modify files in the Docs repository** - this is the documentation repository
3. Reference source code and ADRs (Architecture Decision Records) in Brighter/docs/adr for understanding features
4. Release notes are in Brighter/release_notes.md

## Documentation Structure

The Docs repository follows GitBook structure:
- `SUMMARY.md` - Table of contents
- `contents/` - All documentation markdown files
- Documentation uses GitHub-flavored markdown
- Code examples use C# with syntax highlighting

## Documentation Goals

### Primary Objectives
1. Make documentation accessible to newcomers
2. Provide depth for experienced users
3. Add documentation for new V10 features
4. Improve clarity and reduce complexity

### Areas for Improvement Identified
- Show Me the Code.md may need simplification (use simpler examples without transactions)
- Terminology: Consider "Dispatcher" instead of "ServiceActivator" for better clarity
- Ensure all V10 features are documented

## V10 New Features to Document

Based on release_notes.md, the following V10 features need documentation:
- Cloud Events Support (full specification support)
- OpenTelemetry Integration (OTel Semantic Conventions)
- Default Message Mappers (no longer need explicit IAmAMessageMapper)
- Dynamic Message Deserialization (content-based routing)
- Agreement Dispatcher (dynamic handler resolution)
- Request Context Improvements (OriginatingMessage, PartitionKey, Custom Headers)
- Reactor and Proactor (terminology change from blocking/non-blocking)
- Scheduled Requests/Messaging (integration with schedulers)
- InMemory Options (for development and testing)
- Nullable Reference Types (breaking change)
- Simplified Configuration (AddProducers/AddConsumers)
- Polly Resilience Pipeline (replaces legacy timeout policies)
- AWS SDK v4 Support
- PostgreSQL Message Broker
- RabbitMQ Enhancements (Quorum queues, v7.x support)
- Kafka Improvements
- Sweeper Circuit Breaking

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
