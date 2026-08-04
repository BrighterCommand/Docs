#!/usr/bin/env python3
"""Task 4.3 — qualify the 31 within-page duplicate headings (rule 3b).

Line-targeted so that repeated texts on one page can be told apart; every
edit asserts the line it is replacing, so a stale line number stops the run
rather than corrupting a page.
"""
import io
import sys

# (file, lineno, expected, replacement)
EDITS = [
    # --- InMemoryOptions.md: five components, each with the same four H3s ---
    ('InMemoryOptions.md', 40, '### When to Use', '### When to Use the InMemory Transport'),
    ('InMemoryOptions.md', 55, '### Configuration', '### InMemory Transport Configuration'),
    ('InMemoryOptions.md', 142, '### Limitations', '### InMemory Transport Limitations'),
    ('InMemoryOptions.md', 168, '### When to Use', '### When to Use the InMemory Outbox'),
    ('InMemoryOptions.md', 182, '### Configuration', '### InMemory Outbox Configuration'),
    ('InMemoryOptions.md', 222, '### Limitations', '### InMemory Outbox Limitations'),
    ('InMemoryOptions.md', 233, '### When to Use', '### When to Use the InMemory Inbox'),
    ('InMemoryOptions.md', 246, '### Configuration', '### InMemory Inbox Configuration'),
    ('InMemoryOptions.md', 267, '### Example Usage', '### InMemory Inbox Example Usage'),
    ('InMemoryOptions.md', 290, '### Limitations', '### InMemory Inbox Limitations'),
    ('InMemoryOptions.md', 301, '### When to Use', '### When to Use the InMemory Scheduler'),
    ('InMemoryOptions.md', 317, '### Configuration', '### InMemory Scheduler Configuration'),
    ('InMemoryOptions.md', 328, '### Example Usage', '### InMemory Scheduler Example Usage'),
    ('InMemoryOptions.md', 354, '### When to Use', '### When to Use the InMemory Archive'),
    ('InMemoryOptions.md', 364, '### Configuration', '### InMemory Archive Configuration'),
    ('InMemoryOptions.md', 375, '### Example Usage', '### InMemory Archive Example Usage'),

    # --- HandlerFailure.md: four error-handling strategies, same two H3s each ---
    ('HandlerFailure.md', 58, '### What It Does', '### What Requeue with Delay Does'),
    ('HandlerFailure.md', 71, '### When to Use It', '### When to Use Requeue with Delay'),
    ('HandlerFailure.md', 172, '### What It Does', '### What Reject to Dead Letter Queue Does'),
    ('HandlerFailure.md', 178, '### When to Use It', '### When to Use Reject to Dead Letter Queue'),
    ('HandlerFailure.md', 238, '### What It Does', "### What Don't Acknowledge Does"),
    ('HandlerFailure.md', 244, '### When to Use It', "### When to Use Don't Acknowledge"),
    ('HandlerFailure.md', 340, '### What It Does', '### What Invalid Message Handling Does'),
    ('HandlerFailure.md', 350, '### When to Use It', '### When to Use Invalid Message Handling'),

    # --- ReactorAndProactor.md: the same three H4s under each pattern ---
    ('ReactorAndProactor.md', 72, '#### Handlers', '#### Reactor Handlers'),
    ('ReactorAndProactor.md', 88, '#### Message Mappers', '#### Reactor Message Mappers'),
    ('ReactorAndProactor.md', 112, '#### Middleware/Attributes', '#### Reactor Middleware/Attributes'),
    ('ReactorAndProactor.md', 132, '#### Handlers', '#### Proactor Handlers'),
    ('ReactorAndProactor.md', 150, '#### Message Mappers', '#### Proactor Message Mappers'),
    ('ReactorAndProactor.md', 177, '#### Middleware/Attributes', '#### Proactor Middleware/Attributes'),

    # --- AWSSQSConfiguration.md: an H3 repeating its own H2's title. What it
    #     actually adds is the package list, so it is named for that. Its
    #     trailing link pointed back into the section it sits in; the migration
    #     guidance it promises is the next H2.
    ('AWSSQSConfiguration.md', 404, '### AWS SDK v4 Support', '### Available Packages for SDK v3 and v4'),
    ('AWSSQSConfiguration.md', 418,
     'See [AWS SDK v4 Support](#aws-sdk-v4-support) for migration guidance.',
     'See [V10 Migration Path](#v10-migration-path) for migration guidance.'),

    # --- BrighterSchedulerSupport.md ---
    ('BrighterSchedulerSupport.md', 135, '### Configuration', '### Configuring a Scheduler'),
    ('BrighterSchedulerSupport.md', 207, '### Configuration', '### Requeue Delay Configuration'),

    # --- CustomScheduler.md: the same steps heading under each scheduler kind ---
    ('CustomScheduler.md', 13, '### Implementation Steps', '### Message Scheduler Implementation Steps'),
    ('CustomScheduler.md', 81, '### Implementation Steps', '### Request Scheduler Implementation Steps'),

    # --- ImplementAQueryHandler.md: one example per handler pattern ---
    ('ImplementAQueryHandler.md', 240, '### Complete Example', '### Complete Synchronous Handler Example'),
    ('ImplementAQueryHandler.md', 346, '### Complete Example', '### Complete Direct Implementation Example'),

    # --- KafkaConfiguration.md: publication and subscription each take a hook.
    #     Line 227 says "below" but #configuration-callback resolves to the
    #     publication section above it; qualifying makes the link land right.
    ('KafkaConfiguration.md', 184, '### Configuration Callback', '### Publication Configuration Callback'),
    ('KafkaConfiguration.md', 275, '### Configuration Callback', '### Subscription Configuration Callback'),
    ('KafkaConfiguration.md', 227,
     '- **ConfigHook**: Allows you to modify the Kafka client configuration before a consumer is created. Used to set properties that Brighter does not expose. See [Configuration Callback](#configuration-callback) below.',
     '- **ConfigHook**: Allows you to modify the Kafka client configuration before a consumer is created. Used to set properties that Brighter does not expose. See [Configuration Callback](#subscription-configuration-callback) below.'),

    # --- PostgreSQLMessageBroker.md ---
    ('PostgreSQLMessageBroker.md', 11, '### How It Works', '### How the PostgreSQL Broker Works'),
    ('PostgreSQLMessageBroker.md', 30, '### Transactional Messaging', '### Transactional Guarantees'),
    ('PostgreSQLMessageBroker.md', 317, '### How It Works', '### How Message Visibility Works'),
    ('PostgreSQLMessageBroker.md', 350, '### Configuration', '### JSONB Configuration'),

    # --- RabbitMQConfiguration.md: three sections each ending in best practices ---
    ('RabbitMQConfiguration.md', 254, '### Best Practices', '### Quorum Queue Best Practices'),
    ('RabbitMQConfiguration.md', 413, '### Best Practices', '### Persistent Message Best Practices'),
    ('RabbitMQConfiguration.md', 556, '### Best Practices', '### Best Practices for Blocked Connections'),
]


def main():
    by_file = {}
    for name, lineno, expected, replacement in EDITS:
        by_file.setdefault(name, []).append((lineno, expected, replacement))

    # Verify every edit against every file before writing any of them, so a
    # stale line number cannot leave the tree half-rewritten.
    failures = []
    staged = {}
    for name, edits in sorted(by_file.items()):
        path = 'contents/' + name
        lines = io.open(path, encoding='utf-8').read().split('\n')
        for lineno, expected, replacement in edits:
            actual = lines[lineno - 1]
            if actual.rstrip() != expected:
                failures.append(
                    f'{path}:{lineno}\n  expected: {expected!r}\n  actual:   {actual!r}')
            else:
                lines[lineno - 1] = replacement
        staged[path] = lines

    if failures:
        print('NOTHING WRITTEN — line numbers are stale:\n')
        print('\n'.join(failures))
        return 1

    for path, lines in sorted(staged.items()):
        io.open(path, 'w', encoding='utf-8').write('\n'.join(lines))
        print(f'{len(by_file[path.split("/")[-1]]):3d}  {path}')

    print(f'\n{len(EDITS)} lines rewritten across {len(by_file)} files')
    return 0


if __name__ == '__main__':
    sys.exit(main())
