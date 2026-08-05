#!/usr/bin/env python3
"""Tasks 4.1/4.2 — qualify the 262 cross-page duplicate H2 headings (rule 3a).

Run with no argument to print the proposal; run with `--apply` to write it.

The default qualifier is "<subject> <heading>", where <subject> is the page's
H1 with filler removed (SUBJECT below). OVERRIDE carries the cases where that
reads badly, and KEEP carries headings deliberately left alone because the
collision is better fixed on the *other* page — usually because the anchor
here has inbound links worth preserving.
"""
import io
import os
import re
import sys
import collections

sys.path.insert(0, 'tools')
import pagelint  # noqa: E402

# --------------------------------------------------------------------------
# The subject each page qualifies with.
# --------------------------------------------------------------------------
SUBJECT = {
    'AgreementDispatcher.md': 'Agreement Dispatcher',
    'AnalyzerSupport.md': 'Analyzer',
    'AsyncAPISupport.md': 'AsyncAPI',
    'AsyncDispatchARequest.md': 'Async',
    'AwsScheduler.md': 'AWS Scheduler',
    'AWSSQSConfiguration.md': 'SQS',
    'AzureBlobArchiveProvider.md': 'Azure Blob Archive Provider',
    'AzureBlobConfiguration.md': 'Azure Archive Provider',
    'AzureBlobDistributedLock.md': 'Azure Blob Distributed Lock',
    'AzureScheduler.md': 'Azure Scheduler',
    'AzureServiceBusConfiguration.md': 'Azure Service Bus',
    'BasicConcepts.md': 'Basic Concepts',
    'BoxProvisioning.md': 'Box Provisioning',
    'BoxProvisioningConfiguration.md': 'Box Provisioning',
    'BoxProvisioningUpgrade.md': 'Upgrade',
    'BrighterBasicConfiguration.md': 'Brighter',
    'BrighterSchedulerSupport.md': 'Brighter Scheduler',
    'CloudEventsSupport.md': 'CloudEvents',
    'CommandsCommandDispatcherandProcessor.md': 'Command Patterns',
    'CQRSWithBrighterAndDarker.md': 'CQRS',
    'DapperOutbox.md': 'Dapper Outbox',
    'DarkerBasicConfiguration.md': 'Darker Configuration',
    'DefaultMessageMappers.md': 'Default Message Mapper',
    'DispatchingARequest.md': 'Request Dispatch',
    'DynamicMessageDeserialization.md': 'Dynamic Deserialization',
    'DynamoDbDistributedLock.md': 'DynamoDB Distributed Lock',
    'DynamoInbox.md': 'Dynamo Inbox',
    'DynamoOutbox.md': 'DynamoDb Outbox',
    'EFCoreOutbox.md': 'EF Core Outbox',
    'EventCarriedStateTransfer.md': 'ECST',
    'EventDrivenCollaboration.md': 'Event Driven Collaboration',
    'FAQ.md': None,      # handled wholesale — see FAQ_SECTIONS
    'FirestoreDistributedLock.md': 'Firestore Distributed Lock',
    'Glossary.md': None,  # handled by OVERRIDE
    'HangfireScheduler.md': 'Hangfire',
    'HowConfiguringTheDispatcherWorks.md': None,  # OVERRIDE
    'HowServiceActivatorWorks.md': 'Dispatcher',
    'ImplementAQueryHandler.md': 'Query Handler',
    'InMemoryOptions.md': 'InMemory Options',
    'InMemoryScheduler.md': 'InMemory Scheduler',
    'KafkaConfiguration.md': 'Kafka',
    'Microservices.md': None,  # only `## Next`, handled as navigation
    'MongoDbDistributedLock.md': 'MongoDB Distributed Lock',
    'MongoDBInbox.md': 'MongoDB Inbox',
    'MongoDBOutbox.md': 'MongoDB Outbox',
    'MsSqlDistributedLock.md': 'MS SQL Distributed Lock',
    'MSSQLInbox.md': 'MSSQL Inbox',
    'MSSQLOutbox.md': 'MSSQL Outbox',
    'MySqlDistributedLock.md': 'MySQL Distributed Lock',
    'MySQLInbox.md': 'MySQL Inbox',
    'MySQLOutbox.md': 'MySQL Outbox',
    'NullableReferenceTypes.md': 'Nullable Reference Type',
    'PipelineValidation.md': 'Pipeline Validation',
    'PolicyFallback.md': 'Fallback',
    'PolicyRetryAndCircuitBreaker.md': 'Retry and Circuit Breaker',
    'PostgresDistributedLock.md': 'Postgres Distributed Lock',
    'PostgresInbox.md': 'Postgres Inbox',
    'PostgresOutbox.md': 'PostgreSQL Outbox',
    'PostgreSQLMessageBroker.md': 'PostgreSQL Message Broker',
    'QuartzScheduler.md': 'Quartz',
    'QueriesAndQueryObjects.md': 'Query Object',
    'QueryPatterns.md': 'Query Pattern',
    'QueryPipeline.md': 'Query Pipeline',
    'RabbitMQConfiguration.md': 'RabbitMQ',
    'ReactorAndProactor.md': 'Reactor and Proactor',
    'ReplayOnSeen.md': 'Replay On Seen',
    'RequestValidation.md': 'Request Validation',
    'Routing.md': 'Routing',
    'ShowMeTheCode.md': None,   # keeps `## Brighter and Darker`; Glossary moves
    'SqliteInbox.md': 'Sqlite Inbox',
    'SqliteOutbox.md': 'SQLite Outbox',
    'SweeperCircuitBreaking.md': 'Sweeper Circuit Breaking',
    'Telemetry.md': 'Telemetry',
    'TestDoubleOptions.md': 'Test Double',
    'TickerQScheduler.md': 'TickerQ',
    'UsingTheContextBag.md': 'Context Bag',
    'V10MigrationGuide.md': 'V10 Migration',
}

# --------------------------------------------------------------------------
# Headings whose collision is fixed on the other page, so this one is left as
# it is. Every entry names the page that moves instead.
# --------------------------------------------------------------------------
KEEP = {
    # 8 inbound links; `Command Patterns` renames its pattern sections instead.
    ('BasicConcepts.md', 'Command'),
    ('BasicConcepts.md', 'Command Processor'),
    # 6 inbound links, 4 of them cross-page; HowConfiguringTheDispatcherWorks moves.
    ('BrighterBasicConfiguration.md', '**Configuring The Dispatcher**'),
    # Glossary takes "Terms" instead.
    ('ShowMeTheCode.md', 'Brighter and Darker'),
    ('EventDrivenCollaboration.md', 'Messaging'),
    # `## Dispatching Requests` on the page actually titled that; the async
    # page qualifies.
    ('DispatchingARequest.md', 'Dispatching Requests'),
}

# --------------------------------------------------------------------------
# Where "<subject> <heading>" reads badly, or the fix is a different idea.
# --------------------------------------------------------------------------
OVERRIDE = {
    # Glossary sections group terms; say so.
    ('Glossary.md', 'Messaging'): 'Messaging Terms',
    ('Glossary.md', 'Scheduling'): 'Scheduler Terms',
    ('Glossary.md', 'Brighter and Darker'): 'Brighter and Darker Terms',

    # The page whose whole subject is this; name the pattern, not the page.
    ('CommandsCommandDispatcherandProcessor.md', 'Command'): 'The Command Pattern',
    ('CommandsCommandDispatcherandProcessor.md', 'Command Processor'):
        'The Command Processor Pattern',

    # Takes the collision so BrighterBasicConfiguration's linked anchor survives.
    ('HowConfiguringTheDispatcherWorks.md', 'Configuring the Dispatcher'):
        'Configuring a Dispatcher for an External Bus',

    # Reads as a sentence rather than a prefix.
    ('AsyncDispatchARequest.md', 'Dispatching Requests'):
        'Dispatching Requests Asynchronously',
    ('AsyncDispatchARequest.md', 'Registering a Handler'):
        'Registering an Async Handler',
    ('AsyncDispatchARequest.md', 'Usage'): 'Async Dispatch Usage',
    ('DispatchingARequest.md', 'Registering a Handler'):
        'Registering a Sync Handler',
    ('DispatchingARequest.md', 'Usage'): 'Request Dispatch Usage',
    ('BoxProvisioning.md', 'How it works'): 'How Box Provisioning Works',
    ('RequestValidation.md', 'How It Works'): 'How Request Validation Works',
    ('BoxProvisioningConfiguration.md', 'NuGet packages'):
        'Box Provisioning NuGet Packages',
    ('BoxProvisioningConfiguration.md', 'Common pitfalls'):
        'Box Provisioning Common Pitfalls',

    # "Additional Resources" is the navigation section under another name. All
    # five pages lack any allowlisted heading, so they gain the standard one
    # rather than a qualified variant of a near-synonym.
    ('NullableReferenceTypes.md', 'Additional Resources'): 'Further Reading',
    ('PolicyFallback.md', 'Additional Resources'): 'Further Reading',
    ('PolicyRetryAndCircuitBreaker.md', 'Additional Resources'): 'Further Reading',
    ('PostgreSQLMessageBroker.md', 'Additional Resources'): 'Further Reading',
    ('Telemetry.md', 'Additional Resources'): 'Further Reading',

    # `## Next` is navigation: each is one line reading "See X for guidance".
    ('EventCarriedStateTransfer.md', 'Next'): 'Next Steps',
    ('EventDrivenCollaboration.md', 'Next'): 'Next Steps',
    ('Microservices.md', 'Next'): 'Next Steps',

    # "Darker Configuration Configuration Options" says it twice.
    ('DarkerBasicConfiguration.md', 'Configuration Options'):
        'Darker Configuration Options',

    # "<subject> Provisioning the <X> Table" is grammatical but graceless;
    # putting the subject inside the phrase reads as English.
    ('MSSQLInbox.md', 'Provisioning the Inbox Table'):
        'Provisioning the MSSQL Inbox Table',
    ('MySQLInbox.md', 'Provisioning the Inbox Table'):
        'Provisioning the MySQL Inbox Table',
    ('PostgresInbox.md', 'Provisioning the Inbox Table'):
        'Provisioning the Postgres Inbox Table',
    ('SqliteInbox.md', 'Provisioning the Inbox Table'):
        'Provisioning the Sqlite Inbox Table',
    ('MSSQLOutbox.md', '**Provisioning the Outbox Table**'):
        'Provisioning the MSSQL Outbox Table',
    ('MySQLOutbox.md', '**Provisioning the Outbox Table**'):
        'Provisioning the MySQL Outbox Table',
    ('PostgresOutbox.md', '**Provisioning the Outbox Table**'):
        'Provisioning the PostgreSQL Outbox Table',
    ('SqliteOutbox.md', '**Provisioning the Outbox Table**'):
        'Provisioning the SQLite Outbox Table',
}

# FAQ's H2s are question categories, and its own table of contents lists all
# eight. Qualifying only the four that collide would leave that list reading
# half one way and half the other, so all eight take the same form.
FAQ_SECTIONS = [
    'Getting Started', 'Configuration', 'Messaging', 'Handlers & Pipelines',
    'Resilience & Policies', 'Scheduling', 'Migration',
    'Performance & Concurrency',
]


def target(rel, text):
    name = os.path.basename(rel)
    if (name, text) in KEEP:
        return None
    if (name, text) in OVERRIDE:
        return OVERRIDE[(name, text)]
    if name == 'FAQ.md':
        return f'{text} Questions' if text in FAQ_SECTIONS else None
    subject = SUBJECT.get(name)
    if subject is None:
        return None
    plain = text.strip('*')
    if pagelint.slug(plain).startswith(pagelint.slug(subject)):
        return plain
    return f'{subject} {plain}'


def build():
    pages = pagelint.load_pages()
    counts = collections.Counter()
    for rel, p in pages.items():
        for lvl, text, _ in p.headings:
            if lvl == 2 and not pagelint.is_nav(text):
                counts[pagelint.slug(text)] += 1

    plan = []
    for rel, p in sorted(pages.items()):
        for lvl, text, lineno in p.headings:
            if lvl != 2 or pagelint.is_nav(text):
                continue
            collides = counts[pagelint.slug(text)] > 1
            new = target(rel, text)
            # FAQ renames all eight for uniformity, including non-colliding ones.
            if new is None or new == text:
                continue
            if not collides and os.path.basename(rel) != 'FAQ.md':
                continue
            plan.append((rel, lineno, text, new))
    return plan


def apply(plan):
    """Rewrite headings, then repoint every link whose anchor moved."""
    moved = {}   # (rel, old-slug) -> new-slug
    by_file = collections.defaultdict(list)
    for rel, lineno, old, new in plan:
        by_file[rel].append((lineno, old, new))
        moved[(rel, pagelint.slug(old))] = pagelint.slug(new)

    for rel, edits in by_file.items():
        lines = io.open(rel, encoding='utf-8').read().split('\n')
        for lineno, old, new in edits:
            assert lines[lineno - 1].rstrip() == f'## {old}', \
                f'{rel}:{lineno} is {lines[lineno - 1]!r}, expected "## {old}"'
            lines[lineno - 1] = f'## {new}'
        io.open(rel, 'w', encoding='utf-8').write('\n'.join(lines))

    link_re = re.compile(r'(\[[^\]]*\]\()([^)]+)(\))')
    repointed = []

    def fix(path, rel):
        text = io.open(path, encoding='utf-8').read()

        def sub(m):
            url = m.group(2)
            if '#' not in url:
                return m.group(0)
            file_part, _, frag = url.partition('#')
            tgt = 'contents/' + os.path.basename(file_part) if file_part else rel
            if (tgt, frag) in moved:
                repointed.append(f'{rel}: {url} -> {file_part}#{moved[(tgt, frag)]}')
                return f'{m.group(1)}{file_part}#{moved[(tgt, frag)]}{m.group(3)}'
            return m.group(0)

        new_text = link_re.sub(sub, text)
        if new_text != text:
            io.open(path, 'w', encoding='utf-8').write(new_text)

    for path in pagelint.md_files():
        fix(path, os.path.relpath(path, pagelint.ROOT))
    return repointed


def main():
    plan = build()
    if '--apply' not in sys.argv:
        cur = None
        for rel, lineno, old, new in plan:
            if rel != cur:
                print(f'\n{rel}')
                cur = rel
            print(f'  {lineno:5d}  {old:<34} ->  {new}')
        print(f'\n{len(plan)} headings')
        return 0
    repointed = apply(plan)
    print(f'{len(plan)} headings rewritten')
    for r in repointed:
        print('  link ' + r)
    print(f'{len(repointed)} links repointed')
    return 0


if __name__ == '__main__':
    sys.exit(main())
