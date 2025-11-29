# Atlassian CLI - Terminal Usage

CLI tool for Confluence and Jira. Set environment variables and run commands with `dotnet run -- <command>`.

## Environment Variables

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

## Confluence Commands

### create-page
```bash
dotnet run -- create-page --space <SPACE_KEY> --title "<TITLE>" --body "<CONTENT>"
dotnet run -- create-page -s DOCS -t "My Page" -b "<p>Content</p>"
```

### get-page
```bash
dotnet run -- get-page --id <PAGE_ID>
dotnet run -- get-page --space <SPACE_KEY> --title "<TITLE>"
dotnet run -- get-page -i 12345 -f view
```

### update-page
```bash
dotnet run -- update-page --id <PAGE_ID> --body "<NEW_CONTENT>"
dotnet run -- update-page --id 12345 --body "<p>More</p>" --append
dotnet run -- update-page --space DOCS --title "My Page" --body "<p>Updated</p>"
```

## Jira Commands

### get-issue
```bash
dotnet run -- get-issue --key <ISSUE_KEY>
dotnet run -- get-issue -k PROJ-123
```

### create-issue
```bash
dotnet run -- create-issue --project <PROJECT> --summary "<SUMMARY>" --type <TYPE>
dotnet run -- create-issue -p PROJ -s "New task" -t Task -d "Description"
```

### add-comment
```bash
dotnet run -- add-comment --key <ISSUE_KEY> --body "<COMMENT>"
dotnet run -- add-comment -k PROJ-123 -b "Working on this"
```

### change-status
```bash
dotnet run -- change-status --key <ISSUE_KEY> --status "<STATUS>"
dotnet run -- change-status -k PROJ-123 -s "In Progress"
```

### assign-user
```bash
dotnet run -- assign-user --key <ISSUE_KEY> --user <USERNAME>
dotnet run -- assign-user -k PROJ-123 -u john.doe
```

### update-issue
```bash
dotnet run -- update-issue --key <ISSUE_KEY> --description "<DESCRIPTION>"
dotnet run -- update-issue -k PROJ-123 -d "Updated description"
```
