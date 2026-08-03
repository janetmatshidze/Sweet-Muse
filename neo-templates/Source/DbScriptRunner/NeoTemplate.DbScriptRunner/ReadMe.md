# DB Script Runner

A utility for executing vetted, auditable SQL data adjustment scripts against SQL Server databases. Scripts are added to this repository, reviewed via Pull Request, and
automatically executed by CI/CD pipelines once merged (and correctly setup in your DevOps processes).

## Table of Contents
- [DB Script Runner](#db-script-runner)
  - [Table of Contents](#table-of-contents)
  - [Quick Start](#quick-start)
  - [Folder Structure \& Naming](#folder-structure--naming)
  - [How It Works](#how-it-works)
  - [Script Standards](#script-standards)
    - [No PII](#no-pii)
    - [No Transaction Statements](#no-transaction-statements)
    - [Database Targeting](#database-targeting)
    - [Expected Row Counts (Recommended)](#expected-row-counts-recommended)
    - [Script Timeout](#script-timeout)
  - [Validators](#validators)
  - [Configuration](#configuration)
    - [Core Options](#core-options)
    - [CI/CD Context (for auditing)](#cicd-context-for-auditing)
    - [Example appsettings.Development.json](#example-appsettingsdevelopmentjson)
  - [Journal Tables](#journal-tables)
    - [DbUpdateScripts (Success)](#dbupdatescripts-success)
    - [DbUpdateScriptFailures (Failures)](#dbupdatescriptfailures-failures)
  - [Running Locally](#running-locally)
  - [Running in CI/CD](#running-in-cicd)
  - [PR Review Checklist](#pr-review-checklist)
  - [Setup Guides](#setup-guides)
    - [Add DbScriptRunner project using Blueprints](#add-dbscriptrunner-project-using-blueprints)
    - [GoCD Pipeline for CALM Projects](#gocd-pipeline-for-calm-projects)

## Quick Start

1. Create script in: `Scripts/<environment>/<server>/<year>/<month>/`
2. Name it: `dd_nn_TICKET-description.sql` (e.g., `05_01_DT-101_fix-null-customer-names.sql`)
3. Add expected row count comments before data-modifying queries
4. Use either `USE [DatabaseName]` or fully qualified table names in your scripts
5. Test locally (validates only, doesn't execute scripts in development)
6. Submit PR for review and approval
7. CI/CD executes the script after merge

> NOTE: If you have a blueprinted and/or CALM project, see [Setup Guides](#setup-guides) for details on getting DbScriptRunner into your project and setting up GoCD pipelines.

## Folder Structure & Naming

```
Scripts/
└── <environment>/          # e.g., prd, uat, qa
    └── <server>/           # e.g., sql01, sql02
        └── <year>/         # e.g., 2025
            └── <month>/    # e.g., 03, 12
                └── dd_nn_TICKET-description.sql
```

**Naming Pattern:** `dd_nn_TICKET-description.sql`
- `dd` = Day (01-31)
- `nn` = Sequence (01, 02, ...)
- `TICKET` = Work item reference
- `description` = Brief kebab-case description

**Example:** `2025/06/05_01_DT-101_fix-null-customer-names.sql` = First script for June 5th, ticket DT-101

## How It Works

1. **Discovery**: Scans `Scripts/<folder>/<sub-folder>/` for new scripts
2. **Journal Check**: Identifies scripts not in `DbUpdateScripts` table
3. **Validation**: Applies configured validators
4. **Transaction**: Wraps each script in a transaction
5. **Execution**: Runs script (unless `ValidateOnly = true`)
6. **Journaling**: Records result with Git/CI/CD context

## Script Standards

### No PII
Do not include names, emails, phone numbers, or sensitive personal data.

### No Transaction Statements
**Do not use:** `BEGIN TRAN`, `COMMIT`, `ROLLBACK`

The runner handles transactions automatically:
- ✅ Success → Commit
- ❌ Failure or row count mismatch → Rollback

### Database Targeting

You will need to use a `USE` statement or ensure all table names are fully qualified.

**Option A: USE Statement**
```sql
USE [DatabaseName]

UPDATE dbo.Users SET Status = 'Active'
```

**Option B: Fully Qualified Names**
```sql
UPDATE Database.dbo.Users SET Status = 'Active'
```

### Expected Row Counts (Recommended)

The DB Script Runner can check the row counts of each row affecting query in the script, and compare these to the expectation. If they do not match, the transaction will be automatically rolled back. This is a good safety mechanism to ensure we only update what we intend to.

Place **immediately before** the query:

```sql
-- 3 rows
UPDATE dbo.Users SET Status = 'Active' WHERE Region = 'North'

-- Expected: 5
DELETE FROM dbo.TempData WHERE Date < '2024-01-01'

-- 1 row
INSERT INTO dbo.AuditLog VALUES ('Action')

-- Unknown rows  (skips validation)
UPDATE dbo.LargeTable SET Flag = 1
```

Note: both formats (`-- Expected: X` and `-- X rows`) are shown here, but rather stick to 1 format.

⚠️ **Don't comment out with the query:**
```sql
/* BAD
-- 1 row
UPDATE ...
*/
```

**Change placement:** If you would prefer to put Expected Row Counts after the statements, uncomment this line in `Program.cs`:
```csharp
config.RowCountsBeforeQueries = false;
```

### Script Timeout

**Default:** 120 seconds

This default can be changed by setting the `DefaultTimeoutSeconds` property in config:

```csharp
config.DefaultTimeoutSeconds = 300; // 5 minutes
```

The above example sets timeout to 300 seconds (5 minutes) as the **default for all scripts**.

**Override:** Add at script top:
```sql
/*
Timeout: 600
Long-running operation, may take 10 minutes
*/
```

The above example sets timeout to 600 seconds (10 minutes) **for this script**.

## Validators

| Validator | Purpose | Configuration |
|-----------|---------|---------------|
| `NoTransactionStatementsValidator` | Blocks transaction statements | N/A |
| `ScriptOrValidator(ScriptHasUseStatementValidator, FullyQualifiedTableNameValidator)` | Requires USE statement OR fully qualified names | `AllowedDbNamePrefixes` |
| `AllowedAffectedDbNamePrefixesValidator` | Restricts modifiable databases | `AllowedAffectedDbNamePrefixes` |

**Customize:** Edit `GetScriptValidators` in `Program.cs` to add/remove validators as needed. You can create custom validators by implementing `IScriptValidator`.

## Configuration

**Precedence:** CLI > Environment Variable > appsettings.json

### Core Options

| Option | CLI | Env Var | Config | Default |
|--------|-----|---------|--------|---------|
| Connection String | `--connection-string`, `-c` | `CONNECTION_STRING` | `ConnectionStrings:Main` | *(required)* |
| Environment Folder | `--folder`, `-f` | `SCRIPTS_FOLDER` | `Folder` | `""` |
| Server Sub-Folder | `--sub-folder`, `-sf` | `SCRIPTS_SUBFOLDER` | `SubFolder` | `""` |
| Validate Only | `--validate-only`, `-v` | `VALIDATE_ONLY` | `ValidateOnly` | `false` |
| Default Timeout | `--default-timeout-seconds` | `DEFAULT_TIMEOUT_SECONDS` | `DefaultTimeoutSeconds` | `120` |
| Allowed DB Prefixes | `--allowed-db-name-prefixes` | `ALLOWED_DB_NAME_PREFIXES` | `AllowedDbNamePrefixes` | `""` |
| Allowed Affected DBs | `--allowed-affected-db-name-prefixes` | `ALLOWED_AFFECTED_DB_NAME_PREFIXES` | `AllowedAffectedDbNamePrefixes` | `""` |

### CI/CD Context (for auditing)

| Option | CLI | Env Var | Config |
|--------|-----|---------|--------|
| Pipeline Job Tag | `--pipeline-job-tag`, `-t` | `PIPELINE_JOB_TAG` | `PipelineJobTag` |
| Pipeline Job URL | `--pipeline-job-url` | `PIPELINE_JOB_URL` | `PipelineJobUrl` |
| Git Commit Hash | `--git-commit-hash` | `GIT_COMMIT_HASH` | `GitCommitHash` |
| Git Commit URL | `--git-commit-url` | `GIT_COMMIT_URL` | `GitCommitUrl` |
| Git PR Number | `--git-pull-request-number`, `-pr` | `GIT_PULL_REQUEST_NUMBER` | `GitPullRequestNumber` |
| Git PR URL | `--git-pull-request-url` | `GIT_PULL_REQUEST_URL` | `GitPullRequestUrl` |

### Example appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Main": "Server=(local),1433;Database=NeoTemplate.DbUpdates;Trusted_Connection=True;Encrypt=False;Column Encryption Setting=Enabled;MultipleActiveResultSets=True;Persist Security Info=True;"
  },
  //// If you have to change the schema or table name, do it here
  //"Journal": {
  //  "Table": "DbUpdateScriptsTest",
  //  "Schema": "DbUp"
  //},
  // These are just example values for development environment
  "PipelineJobTag": "DevTesting_PipelineJobTag",
  "PipelineJobUrl": "https://www.github.com/DevTesting_PipelineJobUrl",
  "GitCommitHash": "DevTesting_GitCommitHash",
  "GitCommitUrl": "https://www.github.com/DevTesting_GitCommitUrl",
  "GitPullRequestNumber": 1234,
  "GitPullRequestUrl": "https://www.github.com/DevTesting_GitPullRequestUrl",
  // Since we only really want to validate our scripts in development (not actually execute them), we set ValidateOnly to true
  "ValidateOnly": true,
  "AllowedDbNamePrefixes": [
    // can use both PP and Prod databases in all scripts
    { "Name": "NeoTemplate.PP." },
    { "Name": "NeoTemplate.Prd." }
  ],
  "AllowedAffectedDbNamePrefixes": [
    { // can only affect PP databases in pp scripts
      "AppliesToFolder": "pp",
      "Name": "NeoTemplate.PP."
    },
    { // can only affect Prod databases in prd scripts
      "AppliesToFolder": "prd",
      "Name": "NeoTemplate.Prd"
    }
  ]
}
```

## Journal Tables

In the target database, two tables are created for auditing:

### DbUpdateScripts (Success)
- Script name, contents, execution timestamp
- CI/CD context (pipeline job, git commit, PR)
- Execution results and row counts

### DbUpdateScriptFailures (Failures)
- Script name, contents, failure timestamp
- Error message and stack trace
- CI/CD context

**Queries:**
```sql
-- Recent executions
SELECT * FROM dbo.DbUpdateScripts ORDER BY Applied DESC

-- Recent failures
SELECT * FROM dbo.DbUpdateScriptFailures ORDER BY Attempted DESC

-- Check if script ran
SELECT * FROM dbo.DbUpdateScripts WHERE ScriptName LIKE '%fix-null%'

-- Scripts by PR
SELECT * FROM dbo.DbUpdateScripts WHERE Context_GitPullRequestNumber = '1234'
```

## Running Locally

**Visual Studio:** Press F5 (validates only, doesn't execute, provided `ValidateOnly` is set to `true`)

## Running in CI/CD

Once setup, the CI/CD pipeline should automatically trigger once PRs with new scripts are merged into the main branch.

Getting this running in your pipeline will be straightforward if you are using CALM. This is more difficult if you are not using CALM, but still possible provided you can work out a way to get the pipeline to securely connect to the SQL Server.

## PR Review Checklist

- [ ] Correct folder: `Scripts/<env>/<server>/<year>/<month>/`
- [ ] Name follows: `dd_nn_TICKET-description.sql`
- [ ] No PII
- [ ] No transaction statements
- [ ] Uses USE or fully qualified names
- [ ] Expected row counts for data modifications
- [ ] Expected counts NOT commented with queries
- [ ] Custom timeout if > 2 minutes for prod data
- [ ] Tested locally in validate-only mode - a lot of the above would have been caught then!

---

**Remember:** Always test with `ValidateOnly = true` locally before submitting PRs!

## Setup Guides

### Add DbScriptRunner project using Blueprints
For projects with a blueprint file, you can add the DbScriptRunner to the `projects` array and then let the blueprint generator add the project for you.

Prerequisites:
- Your dev machine is configured to run [Blueprints](https://github.com/SingularSystems/neo-iac/blob/master/docs/blueprints/using-neo-blueprints.md), you have the latest [Neo.PS](https://github.com/SingularSystems/neo-iac/blob/master/docs/powershell/pwsh-neo-ps.md) and an up to date `neo-templates` checkout.
- Your project's blueprint must be [upgraded](https://github.com/SingularSystems/neo-iac/blob/master/docs/blueprints/upgrading-neo-blueprints.md) to version `2.6.0`.

Follow these steps to bring in the `DbScriptRunner` project:
- In your `blueprints.json` file, add the following to the end of the `projects` array:
  ```json
      {
        "name": "db-script-runner",
        "folder": "Server/NeoSample.DbScriptRunner",
        "type": "DotNet",
        "dotNet": {
          "template": "neodbsr",
          "arguments": ["-n", "NeoSample"],
          "type": "DbScriptRunner"
        }
      },
  ```
- Replace `NeoSample` in the config above with your project's short name (This should be in the blueprint under `names.shortName`). You can also check your other project entries to see what they are using as the short name.
- If your project is using `Dockerfiles`, you should also ensure that the DbScriptRunner project is excluded from being copied upwards to the `/Server` folder. In the blueprint's `generators` section, ensure you have the following:
  ```json
    "generators": {
      ...
      "dotNetProjects": {
        ...
        "dockerFileMoveExcludedProjectTypes": ["DbScriptRunner"],
        ...
      },
  ```
- Open a `pwsh` terminal in your repository root and run:
  ```pwsh
  Build-NeoBlueprint
  ```

### GoCD Pipeline for CALM Projects
If your project runs on CALM, you can also add an operational pipeline for the environments you want to use DbScriptRunner against. This pipeline will both build and execute DbScriptRunner against the environment's SQL Server(s).

Prerequisites:
- First complete the [Add DbScriptRunner project using Blueprints](#add-dbscriptrunner-project-using-blueprints) section above.

Make the following changes to your `blueprint.json` file:
- In the `iac.operationalPipelines` array, add a new entry using this template:
  ```json
      {
        "name": "execute-db-scripts-{ENVIRONMENT}",
        "description": "Builds and executes the DB Script Runner against the {ENVIRONMENT} environment when changes are detected in the DbScriptRunner's scripts folder for the environment.",
        "location": "we",
        "environment": "host-{HOST_ENVIRONMENT}",
        "variableSets": ["nuget", "deploy"],
        "buildIncludes": ["Server/{SHORT_NAME}.DbScriptRunner/{SHORT_NAME}.DbScriptRunner/Scripts/{ENVIRONMENT}/**/*"],
        "trigger": "auto",
        "tasks": [
          {
            "type": "execute-db-script-runner",
            "executeDbScriptRunner": {
              "environment": "{ENVIRONMENT}",
              "servers": ["sql01"],
              "allowedDbNamePrefixes": ["{ENVIRONMENT_DATABASES_PREFIX}"],
              "allowedAffectedDbNamePrefixes": ["{ENVIRONMENT_DATABASES_PREFIX}"]
            }
          }
        ]
      }
  ```
  - Update the template as follows:
    - Replace `{ENVIRONMENT}` with the `app-space` environment you want to run against (E.g. `pp`, `prd`).
    - Replace `{HOST_ENVIRONMENT}` with the `host-space` environment that the app-space belongs to (E.g. `stg`, `prd`).
    - Replace `{ENVIRONMENT_DATABASES_PREFIX}` with the prefix used on your database names for the environment. In the latest templates, these are defaulted to an uppercase of the project prefix and app-space environment prefix (E.g. `NSMP.PP.`, `NSMP.PRD.`), but this can differ for older projects. You can check what the correct prefix to use is in one of two places:
      - In your `app-space` terraform, open the `tfvars/terraform.{ENVIRONMENT}.tfvars` file, and look at the `sql_servers.{SQL_KEY}.database_prefix` property.
      - Connect to your SQL Server via SSMS and get it from your database names.
    - Replace `{PROJECT_PREFIX}` with the project prefix found in the blueprint's `names.prefix` field.
    - Replace `{SHORT_NAME}` with the short name found in the blueprint's `names.shortName` field.
    - If your SQL Server uses a non-standard name suffix, or you have more than one SQL Server, you will need to update the `servers` array appropriately.
    - ADDITIONAL NOTES:
      - `allowedDbNamePrefixes` is configured to only allow scripts to read from DBs in the target environment. You can add additional environment prefixes if required. (E.g. You have a PreProd script which needs to read the most up to date data from a Prod DB)
      - `allowedAffectedDbNamePrefixes` is configured to only allow scripts to write to DBs in the target environment. Allowing writes to other environments is not recommended.
  - Add a pipeline entry like this for each environment you want to use DbScriptRunner on.
- In the `iac.variableSets` array, ensure you have the following two entries:
  ```json
      "nuget": {
        "NugetConfig": "{{SECRET:[singular-secrets][nuget-config]}}"
      },
      "deploy": {
        "GitHubToken": "{{SECRET:[singular-secrets][singular-github-readonly-token]}}",
        "GitHubRepository": "SingularSystems/{REPOSITORY_NAME}",
        "GitHubRepositoryDeployBranch": "main"
      },
  ```
  - Replace `{REPOSITORY_NAME}` with the name of your GitHub repository (E.g. `sharetrust`, `neo-iac-sampleproject`)
  - Update `GitHubRepositoryDeployBranch` if your primary branch is not called `main` (E.g. Older projects may still be using `master`).
- When done, build the blueprint to generate your new pipelines:
  ```pwsh
  Build-NeoBlueprint
  ```
- Commit and PR your changes into your primary branch, after which the pipelines should appear in GoCD.
  - On the Dashboard, edit your project tab, search for the new pipeline(s) (E.g. using `execute-db-scripts`), tick them, and click `Save`. You will then be able to monitor / check the results when the pipeline has run in response to scripts being PR'd into your primary branch.
  - If your pipelines run into any secret permission issues, you will need to ask one of the GoCD admins to check that your project has the following permissions:
    - `singular-secrets`: Allow, Environment, `{PROJECT_PREFIX}_*`
    - `singular-secrets`: Allow, PipelineGroup, `{PROJECT_PREFIX}_host_*`
    - In both cases, replace `{PROJECT_PREFIX}` with the project prefix found in the blueprint's `names.prefix` field.

> Please reach out to the DevOps team if you require assistance with this pipeline configuration.
