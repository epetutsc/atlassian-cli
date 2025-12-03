# Atlassian CLI

A .NET tool that provides a command-line interface to interact with self-hosted Confluence, Jira, and Bamboo (on-prem) instances via their REST APIs.

## Installation

### Global Tool Installation

Install the tool globally using the .NET CLI:

```bash
dotnet tool install --global AtlassianCli
```

After installation, you can use the `atlassiancli` command from anywhere:

```bash
atlassiancli --help
```

### Update

To update to the latest version:

```bash
dotnet tool update --global AtlassianCli
```

### Uninstall

To uninstall the tool:

```bash
dotnet tool uninstall --global AtlassianCli
```

## Features

### Confluence
- **Create pages** in any Confluence space
- **Read pages** by ID or by title + space key
- **Update pages** with replace or append mode
- Support for Confluence storage format (XHTML)

### Jira
- **Read issues** by key
- **Create issues** in any project
- **Add comments** to issues
- **Change status** of issues (transitions)
- **Assign users** to issues
- **Update issue descriptions**

### Bamboo
- **List projects** and view project details
- **List plans** and view plan configuration
- **List branches** for a plan
- **View build results** for plans
- **Get build details** including stages, changes, and test results
- **Get build logs** with optional text filtering (e.g., filter for 'error' or 'exception')
- **Trigger builds** (queue builds for plans and branches)

### General
- Environment variable-based configuration
- Comprehensive help system
- Hierarchical sub-commands (confluence/jira/bamboo)

## Requirements

- .NET 10.0 SDK or later
- Access to a Confluence, Jira, and/or Bamboo instance with REST API enabled

## Configuration

### Authentication Methods

The CLI supports three authentication methods (in order of preference):

1. **Bearer Token (Personal Access Token)** - Recommended for on-premises instances
   - Set only `*_API_TOKEN` environment variable
   - Uses HTTP Bearer authentication
   - See [Atlassian PAT documentation](https://confluence.atlassian.com/enterprise/using-personal-access-tokens-1026032365.html)

2. **API Token with Username** - For Atlassian Cloud
   - Set both `*_USERNAME` and `*_API_TOKEN`
   - Uses HTTP Basic authentication with token as password

3. **Username and Password** - Legacy method
   - Set both `*_USERNAME` and `*_PASSWORD`
   - Uses HTTP Basic authentication

### Confluence Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `CONFLUENCE_BASE_URL` | Yes | Base URL of your Confluence instance (e.g., `https://confluence.example.com`) |
| `CONFLUENCE_API_TOKEN` | Conditional | Personal Access Token for Bearer auth (use alone), or API token (use with username) |
| `CONFLUENCE_USERNAME` | Conditional | Username for Basic auth (required with API token for Cloud, or with password) |
| `CONFLUENCE_PASSWORD` | Conditional | Password for Basic auth (use with username) |

### Jira Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `JIRA_BASE_URL` | Yes | Base URL of your Jira instance (e.g., `https://jira.example.com`) |
| `JIRA_API_TOKEN` | Conditional | Personal Access Token for Bearer auth (use alone), or API token (use with username) |
| `JIRA_USERNAME` | Conditional | Username for Basic auth (required with API token for Cloud, or with password) |
| `JIRA_PASSWORD` | Conditional | Password for Basic auth (use with username) |

### Bamboo Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `BAMBOO_BASE_URL` | Yes | Base URL of your Bamboo instance (e.g., `https://bamboo.example.com`) |
| `BAMBOO_API_TOKEN` | Conditional | Personal Access Token for Bearer auth (use alone), or API token (use with username) |
| `BAMBOO_USERNAME` | Conditional | Username for Basic auth (required with API token for Cloud, or with password) |
| `BAMBOO_PASSWORD` | Conditional | Password for Basic auth (use with username) |

### Example Configurations

#### Bearer Token (Personal Access Token) - On-Premises

```bash
# Confluence with PAT
export CONFLUENCE_BASE_URL=https://confluence.example.com
export CONFLUENCE_API_TOKEN=your-personal-access-token

# Jira with PAT
export JIRA_BASE_URL=https://jira.example.com
export JIRA_API_TOKEN=your-personal-access-token
```

#### API Token with Username - Atlassian Cloud

```bash
# Confluence Cloud
export CONFLUENCE_BASE_URL=https://your-domain.atlassian.net/wiki
export CONFLUENCE_USERNAME=your.email@example.com
export CONFLUENCE_API_TOKEN=your-api-token

# Jira Cloud
export JIRA_BASE_URL=https://your-domain.atlassian.net
export JIRA_USERNAME=your.email@example.com
export JIRA_API_TOKEN=your-api-token

# Bamboo with PAT
export BAMBOO_BASE_URL=https://bamboo.example.com
export BAMBOO_API_TOKEN=your-personal-access-token
```

## Building

```bash
dotnet build
```

## Usage

The CLI uses a hierarchical command structure where you first specify the service (`confluence`, `jira`, or `bamboo`) followed by the specific command.

### Show Help

```bash
dotnet run -- --help
dotnet run -- confluence --help
dotnet run -- jira --help
dotnet run -- bamboo --help
```

---

## File Support

All commands that accept body or description content support reading from a UTF-8 encoded file using the `--file` option (or `--description-file` for create-issue). This allows you to provide content from a file instead of the command line.

Example with file:
```bash
dotnet run -- confluence create-page --space MYSPACE --title "My Page" --file content.html
```

---

## Confluence Commands

### Create a Page

```bash
dotnet run -- confluence create-page --space MYSPACE --title "My New Page" --body "<p>Hello Confluence!</p>"
```

With file:
```bash
dotnet run -- confluence create-page --space MYSPACE --title "My New Page" --file content.html
```

Options:
- `-s, --space` (required): Space key
- `-t, --title` (required): Page title
- `-b, --body`: Page content in storage format (either --body or --file is required)
- `--file`: Path to a UTF-8 encoded file containing the body content

### Get a Page

By ID:
```bash
dotnet run -- confluence get-page --id 12345
```

By title and space:
```bash
dotnet run -- confluence get-page --space MYSPACE --title "My Page"
```

Options:
- `-i, --id`: Page ID
- `-s, --space`: Space key (required with --title)
- `-t, --title`: Page title (required with --space)
- `-f, --format`: Output format - `storage` (default) or `view`

### Update a Page

Replace content:
```bash
dotnet run -- confluence update-page --id 12345 --body "<p>New content</p>"
```

Append content:
```bash
dotnet run -- confluence update-page --id 12345 --body "<p>Additional content</p>" --append
```

Update by title:
```bash
dotnet run -- confluence update-page --space MYSPACE --title "My Page" --body "<p>Updated</p>"
```

With file:
```bash
dotnet run -- confluence update-page --id 12345 --file content.html
dotnet run -- confluence update-page --id 12345 --file content.html --append
```

Options:
- `-i, --id`: Page ID
- `-s, --space`: Space key (required with --title)
- `-t, --title`: Page title (required with --space)
- `-b, --body`: New page content (either --body or --file is required)
- `--file`: Path to a UTF-8 encoded file containing the body content
- `-a, --append`: Append to existing content instead of replacing

---

## Jira Commands

### Get an Issue

```bash
dotnet run -- jira get-issue --key PROJ-123
```

Options:
- `-k, --key` (required): Issue key (e.g., PROJ-123)

### Create an Issue

```bash
dotnet run -- jira create-issue --project PROJ --summary "My new task" --type Task
```

With description:
```bash
dotnet run -- jira create-issue --project PROJ --summary "Bug fix needed" --type Bug --description "Detailed description here"
```

With description from file:
```bash
dotnet run -- jira create-issue --project PROJ --summary "Task" --type Task --description-file desc.txt
```

Options:
- `-p, --project` (required): Project key
- `-s, --summary` (required): Issue summary/title
- `-t, --type` (required): Issue type (e.g., Task, Bug, Story)
- `-d, --description`: Issue description
- `--description-file`: Path to a UTF-8 encoded file containing the description

### Add a Comment

```bash
dotnet run -- jira add-comment --key PROJ-123 --body "This is my comment"
```

With file:
```bash
dotnet run -- jira add-comment --key PROJ-123 --file comment.txt
```

Options:
- `-k, --key` (required): Issue key
- `-b, --body`: Comment body text (either --body or --file is required)
- `--file`: Path to a UTF-8 encoded file containing the comment body

### Change Status

```bash
dotnet run -- jira change-status --key PROJ-123 --status "In Progress"
```

Options:
- `-k, --key` (required): Issue key
- `-s, --status` (required): Target status name (e.g., "In Progress", "Done")

Note: The status must be a valid transition from the current issue status.

### Assign User

```bash
dotnet run -- jira assign-user --key PROJ-123 --user john.doe
```

Options:
- `-k, --key` (required): Issue key
- `-u, --user` (required): Username or display name to assign

### Update Issue Description

```bash
dotnet run -- jira update-issue --key PROJ-123 --description "Updated description for the issue"
```

With file:
```bash
dotnet run -- jira update-issue --key PROJ-123 --file description.txt
```

Options:
- `-k, --key` (required): Issue key
- `-d, --description`: New description (either --description or --file is required)
- `--file`: Path to a UTF-8 encoded file containing the description

---

## Bamboo Commands

### List Projects

```bash
dotnet run -- bamboo get-projects
```

### Get a Project

```bash
dotnet run -- bamboo get-project --key PROJ
```

Options:
- `-k, --key` (required): Project key (e.g., PROJ)

### List Plans

```bash
dotnet run -- bamboo get-plans
```

Filter by project:
```bash
dotnet run -- bamboo get-plans --project PROJ
```

Options:
- `-p, --project`: Filter plans by project key (optional)

### Get a Plan (with configuration)

```bash
dotnet run -- bamboo get-plan --key PROJ-PLAN
```

Options:
- `-k, --key` (required): Plan key (e.g., PROJ-PLAN)

This shows plan details including stages, variables, and branches.

### List Plan Branches

```bash
dotnet run -- bamboo get-branches --key PROJ-PLAN
```

Options:
- `-k, --key` (required): Plan key

### List Build Results

```bash
dotnet run -- bamboo get-builds --key PROJ-PLAN
```

With limited results:
```bash
dotnet run -- bamboo get-builds --key PROJ-PLAN --max-results 10
```

Options:
- `-k, --key` (required): Plan key
- `-n, --max-results`: Maximum number of results (default: 25)

### Get a Specific Build Result

```bash
dotnet run -- bamboo get-build --key PROJ-PLAN-123
```

Options:
- `-k, --key` (required): Build result key (e.g., PROJ-PLAN-123)

This shows detailed build information including stages, changes, and test results.

### Get Latest Build Result

```bash
dotnet run -- bamboo get-latest-build --key PROJ-PLAN
```

Options:
- `-k, --key` (required): Plan key

### Queue a Build (Trigger Build)

Queue a build for the default branch:
```bash
dotnet run -- bamboo queue-build --key PROJ-PLAN
```

Queue a build for a specific branch:
```bash
dotnet run -- bamboo queue-build --key PROJ-PLAN --branch feature/my-branch
```

Options:
- `-k, --key` (required): Plan key
- `-b, --branch`: Branch name (optional, builds default branch if not specified)

### Get Build Logs

Get the logs from a build:
```bash
dotnet run -- bamboo get-build-logs --key PROJ-PLAN-123
```

Filter logs to find errors:
```bash
dotnet run -- bamboo get-build-logs --key PROJ-PLAN-123 --filter error
```

Filter logs to find exceptions:
```bash
dotnet run -- bamboo get-build-logs --key PROJ-PLAN-123 --filter exception
```

Get logs for a specific job within the build:
```bash
dotnet run -- bamboo get-build-logs --key PROJ-PLAN-123 --job PROJ-PLAN-JOB1
```

Options:
- `-k, --key` (required): Build result key (e.g., PROJ-PLAN-123)
- `-f, --filter`: Filter log lines containing this text (case-insensitive)
- `-j, --job`: Get logs for a specific job within the build

---

## Project Structure

```
AtlassianCli/
├── Program.cs                    # Entry point, CLI parsing
├── Client/
│   ├── ConfluenceClient.cs       # Confluence REST API client
│   ├── JiraClient.cs             # Jira REST API client
│   └── BambooClient.cs           # Bamboo REST API client
├── Models/
│   ├── ConfluenceModels.cs       # Confluence data transfer objects
│   ├── JiraModels.cs             # Jira data transfer objects
│   └── BambooModels.cs           # Bamboo data transfer objects
├── Commands/
│   ├── CommandOptions.cs         # CLI option definitions
│   └── CommandHandlers.cs        # Command execution logic
└── Progress.md                   # Development progress tracking
```

## License

MIT License