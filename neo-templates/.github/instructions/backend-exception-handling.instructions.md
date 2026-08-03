---
description: "Use when writing or reviewing exception handling in backend services. Covers when to handle, when NOT to handle, and the InvalidDomainOperationException pattern."
applyTo: "**/*.cs"
---

# Exception Handling

- Custom exception handling in services should be the exception, not the norm. Neo and Microsoft packages already provide robust handling and logging for unhandled exceptions.
- For expected exceptions that are part of normal control flow (e.g. validation failures), do not throw general runtime exceptions; instead, use `InvalidDomainOperationException`, a domain-specific exception that can be handled appropriately by the API pipeline and converted into meaningful HTTP 400 responses.
- Foreign Key Constraint Handling in DB Updates
  - Rely on Neo Library built‑in SQL Save Error Handling policies.
  - Do not manually check for related rows before update/delete.
  - Let EF throw `DbUpdateException` on violations.
  - Allow the exception to propagate to the API/global exception handling pipeline; any resulting HTTP status code or response body depends on the configured exception handling/mapping.
- Avoid catching broad Exception types. Only do so if you:
  - Re-throw the exception, or
  - Are operating at a top-level boundary (e.g. middleware, API layer).
- Never swallow exceptions silently (e.g. empty catch blocks), unless the action that was performed does not matter to the operation of the application and you comment / log accordingly.
- Introduce custom exception handling only when there is a clear, intentional deviation from default behavior. Typical cases include:
  - Handling a known, expected exception differently, such as:
    - Logging a failed record while allowing batch processing to continue
    - Adjusting log severity
    - Returning a specific HTTP status code or custom error message
  - Integrating with external systems or APIs where:
    - Specific exceptions need to be interpreted
    - More meaningful feedback should be returned to the user