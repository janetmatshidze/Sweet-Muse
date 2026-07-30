---
name: ef-migrations
description: >
  Generates, removes, or rolls back Entity Framework Core database migrations.
  Use when model changes require a new database migration to be created, or when
  the user asks to add, create, generate, remove, or roll back an EF migration.
---

# EF Migrations Skill

This skill runs EF Core migration commands using the project's `z_EFCommands.txt`
reference file as the authoritative source for `--project`, `--startup-project`,
`--context`, and working-directory settings.

## Step 1 — Discover Command Files

Run the helper script to find all `z_EFCommands.txt` files and their associated
solution directories:

```powershell
$commandFiles = & "$skillDir\find-ef-commands.ps1"
```

The script returns an array of objects with:

| Property | Description |
|---|---|
| `Label` | Display name — the migrations project path relative to the repository root |
| `FilePath` | Absolute path to `z_EFCommands.txt` |
| `SolutionDir` | Directory to `cd` into before running `dotnet ef` commands |

## Step 2 — Select the Target Migrations Project

**If zero files are found:** Inform the developer that no `z_EFCommands.txt` was
found in the repository. Ask them to confirm they are in the right repo, and stop.

**If exactly one file is found:** Proceed with it directly.

**If more than one file is found:** Read each file and identify the DbContext(s) it
references (look for `--context <ContextName>` in the command lines). Then:

- If the target DbContext is **not known** from the request, collect all unique
  DbContext names across every file. Normalize fully-qualified names to their simple
  (unqualified) name — e.g. `NeoTemplate.IdentityServer.IdentityDbContext` →
  `IdentityDbContext` — before de-duplicating. Use `ask_user` to present the
  de-duplicated simple names and ask the developer which context they want to
  target. Once a context is chosen, match files using both the simple name and any
  fully-qualified variant that ends with that simple name. Then apply the rules below.
- If **exactly one** file references the target DbContext, proceed with that file.
- If **more than one** file references the target DbContext, use `ask_user` to
  present only the matching `Label` values and ask the developer which migrations
  project they want to work with.
- If **none** of the files reference the target DbContext, use `ask_user` to
  present the full list of `Label` values and ask the developer which migrations
  project they want to work with.

## Step 3 — Read the Command File

Read the selected `z_EFCommands.txt` using the `view` tool. The file contains
`dotnet ef` command templates. Key conventions:

- Lines beginning with `//` are comments and section markers; do not treat them as commands
- Only lines that begin with `dotnet ef ` are eligible command candidates
- Ignore non-`dotnet ef` setup or tooling lines such as `dotnet tool install --global dotnet-ef --version {version}`
- Extract only the command that matches the selected action from the relevant DbContext section
- For **add** actions, if both an `InitialMigration` command and a `{Name}` command are present, prefer the `{Name}` template unless the user explicitly requested or confirmed an initial migration
- `{Name}` is a placeholder for the new migration name
- `{PreviousMigrationName}` is a placeholder for a rollback target
- Multiple DbContext sections may exist, each preceded by a `// <ContextName>` comment

If the file contains **multiple DbContext sections**, and the exact target DbContext is not
known, use `ask_user` to ask which context they want to target.

## Step 4 — Determine the Action

Map the task or user's request to one of these actions:

| User intent | Action |
|---|---|
| "add migration", "create migration", "generate migration", "new migration" | **add** |
| "remove migration", "undo last migration", "delete last migration" | **remove** |
| "rollback", "revert database", "update database to", "downgrade" | **rollback** |

If the intent is still unclear, use `ask_user` with choices:
`["Add a new migration", "Remove the last migration", "Roll back the database to a specific migration"]`

## Step 5 — Collect Missing Parameters

### For **add**:

If the user has not already provided a migration name, ask:

> "What should the migration be named? Use PascalCase describing the change (e.g. `AddUserEmailIndex`)."

Note: `InitialMigration` is the conventional name for the very first migration on a
project. Only suggest it if the project clearly has no existing migrations yet.

### For **rollback**:

If the user has not specified the target migration name, ask:

> "Which migration should the database be rolled back to? Provide the exact migration name."

### For **remove**:

No additional parameters required.

## Step 6 — Build the Command

Before substituting, validate the provided value using the rules appropriate to each placeholder:

**`{Name}` (migration name)** — must match `^[A-Za-z][A-Za-z0-9_]*$` (letters, digits, and
underscores only, starting with a letter — a valid C# identifier / PascalCase name).
If it does not match, tell the developer the name is invalid, explain the constraint, and
ask them to provide a corrected name before proceeding.

**`{PreviousMigrationName}` (rollback target)** — must be one of:
- `0` (rolls back all migrations), or
- A plain migration name matching `^[A-Za-z][A-Za-z0-9_]*$`, or
- A timestamp-prefixed migration ID matching `^[0-9]{14}_[A-Za-z][A-Za-z0-9_]*$`
  (e.g. `20221011131922_InitialMigration`).

If the rollback target does not match any of these forms, tell the developer it is invalid,
explain the accepted formats, and ask them to provide a corrected value before proceeding.

Once validated, substitute the placeholders:

- Replace `{Name}` with the migration name provided by the developer
- Replace `{PreviousMigrationName}` with the rollback target

Do **not** alter any flags (`--project`, `--context`, `--startup-project`, `-o`,
etc.) already present in the template — they are correct for this project.

If needed, you can append additional arguments to obtain output information.

## Step 7 — Run the Command

1. `cd` to `SolutionDir` (from Step 1).
2. Run the constructed `dotnet ef` command.
3. Report the full output to the developer.
4. On **success**: summarise what was done (e.g. "Migration `AddUserEmailIndex` added to `NeoTemplate.Models.Migrations`").
5. On **failure**: show the error output and suggest common fixes:
   - "dotnet ef not found" → `dotnet tool install --global dotnet-ef`
   - Build errors → check that the project compiles before running migrations
   - Multiple pending model changes → ensure all model edits are saved

## Notes

- Always run from `SolutionDir`, not from the migrations project folder itself.
- Never modify `z_EFCommands.txt`; it is a developer reference file, not generated output.
- If the user wants to run an `InitialMigration` on a project that already has
  migrations, warn them and confirm before proceeding.
