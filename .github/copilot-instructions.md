# GitHub Copilot Instructions for Atlassian CLI

This document provides comprehensive instructions for GitHub Copilot to understand and assist with the Atlassian CLI tool, a .NET console application for interacting with self-hosted Confluence and Jira instances via their REST APIs.

## Tool Overview

The Atlassian CLI is a command-line interface that enables programmatic interaction with:
- **Confluence**: Create, read, and update wiki pages
- **Jira**: Manage issues, comments, status transitions, and user assignments

The CLI uses environment variables for authentication and supports both API token and username/password authentication methods.

## Project Structure

```
AtlassianCli/
├── Program.cs                    # Entry point with CLI parsing using CommandLineParser
├── Client/
│   ├── ConfluenceClient.cs       # HTTP client for Confluence REST API
│   └── JiraClient.cs             # HTTP client for Jira REST API
├── Models/
│   ├── ConfluenceModels.cs       # DTOs for Confluence API
│   └── JiraModels.cs             # DTOs for Jira API
├── Commands/
│   ├── CommandOptions.cs         # CLI option definitions with attributes
│   └── CommandHandlers.cs        # Command execution logic
└── AtlassianCli.csproj           # .NET project file (targets net10.0)
```

## Prerequisites

- .NET 8.0 SDK or later
- Access to a Confluence and/or Jira instance with REST API enabled

## Building the Project

```bash
dotnet build
```

## Running the CLI

```bash
# Show help
dotnet run -- --help

# Run a specific command
dotnet run -- <command> [options]
```

---

## Environment Variables

### Confluence Configuration

Set these environment variables before using Confluence commands:

| Variable | Required | Description |
|----------|----------|-------------|
| `CONFLUENCE_BASE_URL` | Yes | Base URL of your Confluence instance (e.g., `https://confluence.example.com`) |
| `CONFLUENCE_USERNAME` | Yes | Username for authentication |
| `CONFLUENCE_API_TOKEN` | Conditional | API token (recommended - use this OR password) |
| `CONFLUENCE_PASSWORD` | Conditional | Password (use this OR API token) |

### Jira Configuration

Set these environment variables before using Jira commands:

| Variable | Required | Description |
|----------|----------|-------------|
| `JIRA_BASE_URL` | Yes | Base URL of your Jira instance (e.g., `https://jira.example.com`) |
| `JIRA_USERNAME` | Yes | Username for authentication |
| `JIRA_API_TOKEN` | Conditional | API token (recommended - use this OR password) |
| `JIRA_PASSWORD` | Conditional | Password (use this OR API token) |

### Example Configuration (Bash)

```bash
# Confluence
export CONFLUENCE_BASE_URL=https://confluence.example.com
export CONFLUENCE_USERNAME=your.username
export CONFLUENCE_API_TOKEN=your-api-token

# Jira
export JIRA_BASE_URL=https://jira.example.com
export JIRA_USERNAME=your.username
export JIRA_API_TOKEN=your-api-token
```

### Example Configuration (PowerShell)

```powershell
# Confluence
$env:CONFLUENCE_BASE_URL = "https://confluence.example.com"
$env:CONFLUENCE_USERNAME = "your.username"
$env:CONFLUENCE_API_TOKEN = "your-api-token"

# Jira
$env:JIRA_BASE_URL = "https://jira.example.com"
$env:JIRA_USERNAME = "your.username"
$env:JIRA_API_TOKEN = "your-api-token"
```

---

## Confluence Commands

### create-page

Creates a new page in a Confluence space.

**Syntax:**
```bash
dotnet run -- create-page --space <SPACE_KEY> --title "<TITLE>" --body "<CONTENT>"
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--space` | `-s` | Yes | Space key where the page will be created (e.g., 'MYSPACE') |
| `--title` | `-t` | Yes | Title of the new page |
| `--body` | `-b` | Yes | Body content in Confluence storage format (XHTML) or plain text |

**Examples:**
```bash
# Create a simple page
dotnet run -- create-page --space DOCS --title "API Documentation" --body "<p>Welcome to our API docs!</p>"

# Create a page with formatted content
dotnet run -- create-page -s TEAM -t "Meeting Notes" -b "<h2>Agenda</h2><ul><li>Item 1</li><li>Item 2</li></ul>"

# Create a page with a table
dotnet run -- create-page --space PROJ --title "Status Report" --body "<table><tr><th>Task</th><th>Status</th></tr><tr><td>Development</td><td>Complete</td></tr></table>"
```

**Output:**
```
Creating page 'API Documentation' in space 'DOCS'...

Page created successfully!
  ID:    12345
  Title: API Documentation
  Space: DOCS
  URL:   /display/DOCS/API+Documentation
```

---

### get-page

Retrieves a page from Confluence by ID or by title and space.

**Syntax:**
```bash
# By ID
dotnet run -- get-page --id <PAGE_ID>

# By title and space
dotnet run -- get-page --space <SPACE_KEY> --title "<TITLE>"
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--id` | `-i` | Conditional | Page ID (use this OR space+title) |
| `--space` | `-s` | Conditional | Space key (required with --title) |
| `--title` | `-t` | Conditional | Page title (required with --space) |
| `--format` | `-f` | No | Output format: 'storage' (raw XHTML, default) or 'view' (rendered HTML) |

**Examples:**
```bash
# Get page by ID
dotnet run -- get-page --id 12345

# Get page by title and space
dotnet run -- get-page --space DOCS --title "API Documentation"

# Get page content in view format
dotnet run -- get-page -i 12345 -f view

# Get page using short options
dotnet run -- get-page -s TEAM -t "Meeting Notes"
```

**Output:**
```
Fetching page with ID '12345'...

=== Page Information ===
ID:      12345
Title:   API Documentation
Space:   DOCS
Status:  current
Version: 3
URL:     /display/DOCS/API+Documentation

=== Body Content ===
<p>Welcome to our API docs!</p>
```

---

### update-page

Updates an existing page's content in Confluence.

**Syntax:**
```bash
# By ID (replace content)
dotnet run -- update-page --id <PAGE_ID> --body "<NEW_CONTENT>"

# By ID (append content)
dotnet run -- update-page --id <PAGE_ID> --body "<ADDITIONAL_CONTENT>" --append

# By title and space
dotnet run -- update-page --space <SPACE_KEY> --title "<TITLE>" --body "<NEW_CONTENT>"
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--id` | `-i` | Conditional | Page ID (use this OR space+title) |
| `--space` | `-s` | Conditional | Space key (required with --title) |
| `--title` | `-t` | Conditional | Page title (required with --space) |
| `--body` | `-b` | Yes | New body content in Confluence storage format (XHTML) |
| `--append` | `-a` | No | Append to existing content instead of replacing (default: false) |

**Examples:**
```bash
# Replace page content by ID
dotnet run -- update-page --id 12345 --body "<p>Completely new content</p>"

# Append content to page by ID
dotnet run -- update-page --id 12345 --body "<p>Additional paragraph</p>" --append

# Update page by title and space
dotnet run -- update-page --space DOCS --title "API Documentation" --body "<p>Updated docs</p>"

# Using short options
dotnet run -- update-page -i 12345 -b "<p>Quick update</p>" -a
```

**Output:**
```
Updating page with ID '12345'...

Page updated successfully!
  ID:      12345
  Title:   API Documentation
  Version: 4
  URL:     /display/DOCS/API+Documentation
```

---

## Jira Commands

### get-issue

Retrieves a Jira issue by its key.

**Syntax:**
```bash
dotnet run -- get-issue --key <ISSUE_KEY>
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--key` | `-k` | Yes | Issue key (e.g., PROJ-123) |

**Examples:**
```bash
# Get issue details
dotnet run -- get-issue --key PROJ-123

# Using short option
dotnet run -- get-issue -k BUG-456
```

**Output:**
```
Fetching issue 'PROJ-123'...

=== Issue Information ===
Key:         PROJ-123
ID:          10001
Summary:     Fix login button not working
Type:        Bug
Status:      In Progress
Project:     PROJ (Project Name)
Assignee:    John Doe
Reporter:    Jane Smith
Priority:    High
Created:     2024-01-15T10:30:00.000+0000
Updated:     2024-01-16T14:45:00.000+0000
URL:         https://jira.example.com/rest/api/2/issue/10001

=== Description ===
The login button on the homepage is not responding to clicks.

=== Comments (3) ===
[2024-01-15T11:00:00.000+0000] Jane Smith:
  Can you reproduce this on all browsers?

[2024-01-15T12:30:00.000+0000] John Doe:
  Confirmed on Chrome and Firefox.
```

---

### create-issue

Creates a new issue in a Jira project.

**Syntax:**
```bash
dotnet run -- create-issue --project <PROJECT_KEY> --summary "<SUMMARY>" --type <ISSUE_TYPE> [--description "<DESCRIPTION>"]
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--project` | `-p` | Yes | Project key (e.g., PROJ) |
| `--summary` | `-s` | Yes | Issue summary/title |
| `--type` | `-t` | Yes | Issue type (e.g., Task, Bug, Story, Epic) |
| `--description` | `-d` | No | Issue description |

**Examples:**
```bash
# Create a basic task
dotnet run -- create-issue --project PROJ --summary "Implement new feature" --type Task

# Create a bug with description
dotnet run -- create-issue --project PROJ --summary "Login button broken" --type Bug --description "The login button does not respond to clicks on the homepage"

# Create a story using short options
dotnet run -- create-issue -p TEAM -s "User authentication" -t Story -d "As a user, I want to log in securely"

# Create an epic
dotnet run -- create-issue --project PROJ --summary "Q1 Release" --type Epic
```

**Output:**
```
Creating Task in project 'PROJ'...

Issue created successfully!
  Key:  PROJ-124
  ID:   10002
  URL:  https://jira.example.com/rest/api/2/issue/10002
```

---

### add-comment

Adds a comment to an existing Jira issue.

**Syntax:**
```bash
dotnet run -- add-comment --key <ISSUE_KEY> --body "<COMMENT_TEXT>"
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--key` | `-k` | Yes | Issue key (e.g., PROJ-123) |
| `--body` | `-b` | Yes | Comment body text |

**Examples:**
```bash
# Add a simple comment
dotnet run -- add-comment --key PROJ-123 --body "Working on this now"

# Add a detailed comment
dotnet run -- add-comment -k BUG-456 -b "Fixed in commit abc123. Please review and test."

# Add a comment with mentions (if supported)
dotnet run -- add-comment --key PROJ-123 --body "[~john.doe] Can you review this?"
```

**Output:**
```
Adding comment to issue 'PROJ-123'...

Comment added successfully!
  Comment ID: 10050
  Created:    2024-01-16T15:00:00.000+0000
```

---

### change-status

Changes the status of a Jira issue (performs a workflow transition).

**Syntax:**
```bash
dotnet run -- change-status --key <ISSUE_KEY> --status "<TARGET_STATUS>"
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--key` | `-k` | Yes | Issue key (e.g., PROJ-123) |
| `--status` | `-s` | Yes | Target status name (e.g., "In Progress", "Done", "Resolved") |

**Important:** The target status must be a valid transition from the current issue status. The CLI will automatically find the appropriate transition.

**Examples:**
```bash
# Move to In Progress
dotnet run -- change-status --key PROJ-123 --status "In Progress"

# Mark as Done
dotnet run -- change-status -k PROJ-123 -s "Done"

# Resolve an issue
dotnet run -- change-status --key BUG-456 --status "Resolved"

# Reopen an issue
dotnet run -- change-status --key PROJ-123 --status "Open"
```

**Output:**
```
Changing status of issue 'PROJ-123' to 'In Progress'...

Issue 'PROJ-123' transitioned to 'In Progress' successfully!
```

**Error (invalid transition):**
```
Error: Cannot transition to 'Closed'. Available transitions: In Progress, Done, On Hold
```

---

### assign-user

Assigns a user to a Jira issue.

**Syntax:**
```bash
dotnet run -- assign-user --key <ISSUE_KEY> --user <USERNAME>
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--key` | `-k` | Yes | Issue key (e.g., PROJ-123) |
| `--user` | `-u` | Yes | Username or display name to assign |

**Examples:**
```bash
# Assign by username
dotnet run -- assign-user --key PROJ-123 --user john.doe

# Assign by display name
dotnet run -- assign-user -k PROJ-123 -u "John Doe"

# Reassign to different user
dotnet run -- assign-user --key BUG-456 --user jane.smith
```

**Output:**
```
Assigning user 'john.doe' to issue 'PROJ-123'...

User 'john.doe' assigned to issue 'PROJ-123' successfully!
```

---

### update-issue

Updates a Jira issue's description.

**Syntax:**
```bash
dotnet run -- update-issue --key <ISSUE_KEY> --description "<NEW_DESCRIPTION>"
```

**Options:**
| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--key` | `-k` | Yes | Issue key (e.g., PROJ-123) |
| `--description` | `-d` | Yes | New description for the issue |

**Examples:**
```bash
# Update description
dotnet run -- update-issue --key PROJ-123 --description "Updated description with more details"

# Clear and replace description
dotnet run -- update-issue -k BUG-456 -d "Steps to reproduce:\n1. Go to login page\n2. Click login button\n3. Nothing happens"
```

**Output:**
```
Updating description of issue 'PROJ-123'...

Issue 'PROJ-123' description updated successfully!
```

---

## Error Handling

The CLI provides detailed error messages for common issues:

### Authentication Errors

```
Error: CONFLUENCE_BASE_URL environment variable is not set.
Please set it to your Confluence instance URL (e.g., https://confluence.example.com)
```

```
Error: Authentication not configured. Please set either:
  - CONFLUENCE_USERNAME and CONFLUENCE_API_TOKEN, or
  - CONFLUENCE_USERNAME and CONFLUENCE_PASSWORD
```

### Validation Errors

```
Error: You must provide either --id or both --space and --title.
```

### API Errors

```
Error: HTTP 401 (Unauthorized)
Details: {"message":"User is not authenticated."}
```

```
Error: HTTP 404 (Not Found)
Details: Page with title 'NonExistent' not found in space 'DOCS'
```

---

## Confluence Storage Format

Confluence uses XHTML storage format for page content. Here are common formatting examples:

### Basic Text

```html
<p>This is a paragraph.</p>
<p>This is <strong>bold</strong> and <em>italic</em> text.</p>
```

### Headings

```html
<h1>Heading 1</h1>
<h2>Heading 2</h2>
<h3>Heading 3</h3>
```

### Lists

```html
<!-- Unordered list -->
<ul>
  <li>Item 1</li>
  <li>Item 2</li>
</ul>

<!-- Ordered list -->
<ol>
  <li>First</li>
  <li>Second</li>
</ol>
```

### Tables

```html
<table>
  <tr>
    <th>Header 1</th>
    <th>Header 2</th>
  </tr>
  <tr>
    <td>Cell 1</td>
    <td>Cell 2</td>
  </tr>
</table>
```

### Links

```html
<a href="https://example.com">External Link</a>
```

### Code Blocks

```html
<ac:structured-macro ac:name="code">
  <ac:parameter ac:name="language">python</ac:parameter>
  <ac:plain-text-body><![CDATA[print("Hello World")]]></ac:plain-text-body>
</ac:structured-macro>
```

---

## Common Usage Patterns

### Workflow: Create and Update Documentation

```bash
# Set up environment
export CONFLUENCE_BASE_URL=https://confluence.example.com
export CONFLUENCE_USERNAME=your.username
export CONFLUENCE_API_TOKEN=your-api-token

# Create a new documentation page
dotnet run -- create-page --space DOCS --title "API Reference" --body "<p>Initial content</p>"

# Get the page ID from output, then update it
dotnet run -- update-page --id 12345 --body "<h2>Endpoints</h2><p>GET /api/users</p>"

# Append more content
dotnet run -- update-page --id 12345 --body "<p>POST /api/users - Create a new user</p>" --append
```

### Workflow: Jira Issue Lifecycle

```bash
# Set up environment
export JIRA_BASE_URL=https://jira.example.com
export JIRA_USERNAME=your.username
export JIRA_API_TOKEN=your-api-token

# Create a new task
dotnet run -- create-issue --project PROJ --summary "Implement login feature" --type Task --description "Add OAuth2 login support"

# Assign to developer
dotnet run -- assign-user --key PROJ-125 --user john.doe

# Start work on the issue
dotnet run -- change-status --key PROJ-125 --status "In Progress"

# Add a progress comment
dotnet run -- add-comment --key PROJ-125 --body "Started implementation. ETA: 2 days"

# Mark as done
dotnet run -- change-status --key PROJ-125 --status "Done"
```

### Workflow: Bug Triage

```bash
# Get issue details
dotnet run -- get-issue --key BUG-100

# Update description with investigation findings
dotnet run -- update-issue --key BUG-100 --description "Root cause: Missing null check in AuthService.cs line 45"

# Add comment about fix
dotnet run -- add-comment --key BUG-100 --body "Fixed in PR #234. Awaiting review."

# Change status to resolved
dotnet run -- change-status --key BUG-100 --status "Resolved"
```

---

## Tips for Copilot

When assisting users with this CLI:

1. **Always check environment variables first** - Most errors occur due to missing or incorrect configuration.

2. **Use the correct command syntax** - Each command has required and optional parameters. Use `--help` to show available options.

3. **Confluence content uses XHTML** - When creating or updating pages, ensure the body content is valid XHTML.

4. **Jira status transitions must be valid** - The target status must be reachable from the current status based on the workflow configuration.

5. **API tokens are preferred** - For security, recommend using API tokens over passwords.

6. **Page IDs vs Title+Space** - Pages can be identified either by ID (faster, more reliable) or by title and space key combination.

7. **Error messages are descriptive** - When errors occur, the CLI provides detailed information including HTTP status codes and API error responses.
