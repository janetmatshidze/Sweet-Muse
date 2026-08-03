---
applyTo: "**/*"
---

## Special Area Risk Rating

When performing a code review, always include a risk rating for the following areas below.

If any specifically mentioned check in an area is found, the risk rating for that area should be at least the severity level of the most severe check found.

### AI

Changes to AI-related code, such as agents, chatmodes, instructions, prompts, skills, or any code that directly interacts with AI models. Issues here could lead to incorrect AI behavior, hallucinations, or security vulnerabilities.

| Severity | Check |
| :---     | :---  |
| High     | GitHub Copilot instruction changes in `.github/copilot-instructions.md`. |
| High     | GitHub Copilot agent changes in `.github/agents`. |
| High     | GitHub Copilot chat mode changes in `.github/chatmodes`. |
| High     | GitHub Copilot instruction changes in `.github/instructions`. |
| High     | GitHub Copilot prompt changes in `.github/prompts`. |
| High     | GitHub Copilot skills changes in `.github/skills`. |
| High     | Claude file changes in `.claude`. |
| High     | Core AI changes in `.ai-core`. |

### Code Analysis

Changes to code analysis tools, such as linters, formatters, or any code that directly affects code quality checks. Issues here could lead to undetected code quality problems or false positives/negatives in analysis results.

| Severity | Check |
| :---     | :---  |
| High     | Editor configuration changes in a `.editorconfig` file. |
| High     | Diagnostic suppression changes using a `System.Diagnostics.CodeAnalysis.SuppressMessage` attribute. |
| High     | Diagnostic suppression changes using a `#pragma warning` directive. |
| High     | Global diagnostic suppression changes in a `GlobalSuppressions.cs` file. |
| Medium   | CODEOWNERS configuration changes in `.github/CODEOWNERS`. |
| Medium   | Dependabot configuration changes in `.github/dependabot.yml`. |
| Medium   | Git attributes configuration changes in a `.gitattributes` file. |
| Medium   | Project changes in Sdk-style project (`.csproj`, `.vbproj`) files (excluding `<Version>` and `<TargetFramework>` changes). |
| Medium   | Central package management configuration changes in a `Directory.Packages.props` file. |
| Low      | Spelling exclusion dictionary changes in a `.dic` file. |

### Data

Changes to data-related code, such as database models, migrations, or any code that directly interacts with data storage. Issues here could lead to data loss, corruption, or security vulnerabilities.

| Severity | Check |
| :---     | :---  |
| Medium   | SQL changes in a `.sql` file (excluding migrations). |
| Medium   | SQL query changes in a `.cs` file (excluding migrations). |
| Medium   | SQL execution changes in a `.cs` file (excluding migrations). |
| Medium   | Entity tracking changes (excluding migrations). |
| Medium   | LINQ query (method syntax) changes (excluding migrations). |
| Medium   | LINQ query (query syntax) changes (excluding migrations). |

### DevOps

Changes to DevOps-related code, such as CI/CD pipeline configurations, infrastructure as code, or any code that directly affects deployment processes. Issues here could lead to deployment failures, security vulnerabilities, or downtime.

| Severity | Check |
| :---     | :---  |
| High     | Blueprint changes in `blueprint.json`. |
| High     | Dockerfile changes in a `Dockerfile*` file. |
| High     | PowerShell changes in a `.ps1` file. |
| High     | Terraform changes in a `.tf` file. |
| High     | App setting changes in an `appsettings[.*].json` file. |
| High     | Changes to any file in the `IaC/**prd` path. |
| Medium   | Any change to a file in the `IaC/**` path (excluding `IaC/pipelines/**`). |

### Front End

Changes to front-end code, such as UI components, styles, or any code that directly affects the user interface. Issues here could lead to broken UI, poor user experience, or security vulnerabilities.

| Severity | Check |
| :---     | :---  |
| Medium   | Package changes in a `package.json` file (excluding `version` changes). |

### Front End Style

Changes to front-end styles, such as CSS or any code that directly affects the visual appearance of the application. Issues here could lead to broken layouts, inconsistent styling, or poor user experience.

| Severity | Check |
| :---     | :---  |
| Medium   | CSS style changes in a `.css`, `.less` or `.scss` file. |

### General

Changes to general code, such as business logic, utility functions, or any code that does not fall into the above categories. Issues here could lead to bugs, performance issues, or security vulnerabilities.

| Severity | Check |
| :---     | :---  |
| Medium   | Target framework changes. |
| Medium   | Significant PR: More than 250 changes in the PR (excluding migrations, snapshots and changes to `.sln`, `.csproj` and `.vbproj` files). |

### Security

Changes to security-related code, such as authentication, authorization, or any code that directly affects the security of the application. Issues here could lead to security vulnerabilities or breaches.

| Severity | Check |
| :---     | :---  |
| High     | Changes to a controller's accessibility using the `[AllowAnonymous]` attribute. |
| High     | Change containing potentially sensitive credentials, passwords or secrets. |

### Format

The special area risk ratings should be output as a single line per special area in the review summary with corresponding label added to the pull request.

| Area            | Format                                        | Label |
| :---            | :---                                          | :---  |
| AI              | `**AI Risk:** [Low/Medium/High]`              | `ai-[low/medium/high]-risk` |
| Code Analysis   | `**Code Analysis Risk:** [Low/Medium/High]`   | `code-analysis-[low/medium/high]-risk` |
| Data            | `**Data Risk:** [Low/Medium/High]`            | `data-[low/medium/high]-risk` |
| DevOps          | `**DevOps Risk:** [Low/Medium/High]`          | `devops-[low/medium/high]-risk` |
| Front End       | `**Front End Risk:** [Low/Medium/High]`       | `front-end-[low/medium/high]-risk` |
| Front End Style | `**Front End Style Risk:** [Low/Medium/High]` | `front-end-style-[low/medium/high]-risk` |
| General         | `**General Risk:** [Low/Medium/High]`         | `general-[low/medium/high]-risk` |
| Security        | `**Security Risk:** [Low/Medium/High]`        | `security-[low/medium/high]-risk` |

## Overall Risk Rating

When performing a code review, always include an overall risk rating of the changes (Low, Medium, High) based on the potential impact of merging the changes.

The overall risk rating should take into account the special area risk ratings, as well as any other factors that may affect the risk of merging the changes, such as the size of the PR, the complexity of the changes, and the potential impact on users.

### Format

The risk rating should be output as a single line in the review summary with corresponding label added to the pull request.

| Format                                | Label |
| :---                                  | :---  |
| `**Overall Risk:** [Low/Medium/High]` | `overall-[low/medium/high]-risk` |