---
description: "Use when adding or reviewing logging in backend services. Covers when to log, log levels, structured message templates, and what NOT to log."
applyTo: "**/*.cs"
---

# Logging

- Logging is largely handled by Neo, Microsoft, and other foundational packages. Additional logging in services should be minimal and intentional.
- If specific data is important, private, or sensitive, it should be represented as part of the domain model and persisted to the database rather than logged.
- Long-running or complex processes should be tracked via dedicated entities that capture progress, key events, and errors. These should be stored in the database and exposed via the UI or API where appropriate.
- Introduce custom logging only when it adds clear diagnostic or operational value. Typical cases include:
  - Complex logic flows where a trace of execution or decision points is necessary to understand behaviour
  - Handling expected exceptions where logging at a lower severity (e.g. warning or information) avoids unnecessary noise while preserving useful context
  - Integrations with external systems or APIs where:
    - Requests and responses need to be logged for monitoring or debugging
    - Logged data is explicitly non-confidential and safe to persist

## Adding logging

- If you do need to add logging, inject an `ILogger<T>` into your class and use that to write log messages.
- Use appropriate log levels (e.g. `LogInformation`, `LogWarning`, `LogError`) based on the severity and nature of the event being logged.
- Include relevant context in log messages to make them more useful for troubleshooting, such as IDs of affected entities, key variable values, or descriptions of the operation being performed.
- Use structured message templates with named placeholders instead of string interpolation. This preserves structured log properties for querying in log sinks like Serilog:
  - Correct: `logger.LogInformation("Processing order {OrderId}", orderId);`
  - Wrong: `logger.LogInformation($"Processing order {orderId}");`
