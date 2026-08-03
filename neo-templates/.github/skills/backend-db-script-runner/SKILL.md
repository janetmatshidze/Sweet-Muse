---
name: backend-db-script-runner
description: Create and submit an auditable SQL data adjustment script via Neo.DbScriptRunner
argument-hint: "<ticket-id> <description> <environment: dev|staging|prod>"
---

Use this skill when making auditable SQL data changes in any environment, including production. Scripts are version-controlled, peer-reviewed, and executed automatically by Neo.DbScriptRunner after merge. Do **not** run data fixes directly against the database.

## Steps

### 1. Identify the correct script path

```
Server/DbScriptRunner/Scripts/{environment}/{server}/{year}/{month}/
```

Example:
```
Server/DbScriptRunner/Scripts/production/db-primary/2025/05/
```

Environments are defined per project — use the naming convention already established in the `Scripts/` folder.

### 2. Name the script file

```
{dd}_{nn}_{TICKET-ID}-{short-description}.sql
```

| Part | Meaning |
|---|---|
| `dd` | Day of month (01–31) |
| `nn` | Sequence number for that day (01, 02, …) |
| `TICKET-ID` | Jira/GitHub ticket reference |
| `short-description` | Kebab-case description |

Example: `14_01_PROJ-456-fix-crop-cycle-status.sql`

### 3. Write the script

```sql
USE [DatabaseName];  -- or use fully-qualified names: dbo.MyTable

-- Fix incorrect crop cycle statuses
-- Expected: 3 rows affected
UPDATE dbo.CropCycles
SET StatusId = 2
WHERE StatusId = 1
    AND CreatedDate < '2025-01-01';
```

**Script rules:**
- Start with `USE [DatabaseName]` or use fully-qualified schema names.
- Add `-- Expected: N rows affected` comments above every DML statement.
- No `BEGIN TRANSACTION` / `COMMIT` / `ROLLBACK` — the runner manages transactions.
- No PII (names, emails, phone numbers, etc.) in the script or comments.
- Use `UPPER` SQL keywords and consistent formatting (see `backend-sql.instructions.md`).

### 4. Submit for review

- Commit the file and raise a PR.
- The PR description should reference the ticket and explain why the data fix is needed.
- At least one peer must review and approve before merge.

### 5. Monitor execution

Scripts execute automatically after the PR merges to the target branch. Check the DbScriptRunner logs or the deployment pipeline output to confirm successful execution.

## Quality checklist

- [ ] Script placed in the correct `{environment}/{server}/{year}/{month}/` folder
- [ ] Filename follows `dd_nn_TICKET-description.sql` convention
- [ ] `USE [DatabaseName]` or fully-qualified names used
- [ ] Expected row counts documented on every DML statement
- [ ] No PII in script content or comments
- [ ] No transaction statements
- [ ] PR raised and peer-reviewed before merge
