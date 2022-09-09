# Basic Concepts

## Command

A command is an instruction to carry out work. It exercises the domain and results in a change of state.

- Query
- Query Handler (includes request and response types)
- Request (includes Command and Event)

## Request Handler 

A handler is the entry point to domain code. It receives a request, which may be a [Command](#command) or an Event. A handler may be the target of an Internal Bus or an External Bus

## Internal Bus 

(includes middleware pipeline)

## External Bus (includes MoM and Transport)