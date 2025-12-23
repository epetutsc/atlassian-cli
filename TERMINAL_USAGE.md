# Atlassian CLI - Terminal Usage

CLI tool for Confluence, Jira, Bamboo, and BitBucket. Set environment variables and run commands with `dotnet run -- <command>`.

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

# Bamboo
export BAMBOO_BASE_URL=https://bamboo.example.com
export BAMBOO_USERNAME=your.username
export BAMBOO_API_TOKEN=your-api-token

# BitBucket
export BITBUCKET_BASE_URL=https://bitbucket.example.com
export BITBUCKET_USERNAME=your.username
export BITBUCKET_API_TOKEN=your-api-token
```

## File Support

All commands that accept body, description, or text content support reading from a UTF-8 encoded file using the `--file` option (or `--description-file` for create-issue). This allows you to provide content from a file instead of the command line.

## Confluence Commands

### create-page
```bash
dotnet run -- create-page --space <SPACE_KEY> --title "<TITLE>" --body "<CONTENT>"
dotnet run -- create-page -s DOCS -t "My Page" -b "<p>Content</p>"
dotnet run -- create-page --space DOCS --title "My Page" --file content.html
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
dotnet run -- update-page --id 12345 --file content.html
dotnet run -- update-page --id 12345 --file content.html --append
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
dotnet run -- create-issue --project PROJ --summary "Task" --type Task --description-file desc.txt
```

### add-comment
```bash
dotnet run -- add-comment --key <ISSUE_KEY> --body "<COMMENT>"
dotnet run -- add-comment -k PROJ-123 -b "Working on this"
dotnet run -- add-comment --key PROJ-123 --file comment.txt
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
dotnet run -- update-issue --key PROJ-123 --file description.txt
```

## Bamboo Commands

### get-projects
```bash
dotnet run -- bamboo get-projects
```

### get-project
```bash
dotnet run -- bamboo get-project --key <PROJECT_KEY>
dotnet run -- bamboo get-project -k PROJ
```

### get-plans
```bash
dotnet run -- bamboo get-plans
dotnet run -- bamboo get-plans --project <PROJECT_KEY>
```

### get-plan
```bash
dotnet run -- bamboo get-plan --key <PLAN_KEY>
dotnet run -- bamboo get-plan -k PROJ-PLAN
```

### get-branches
```bash
dotnet run -- bamboo get-branches --key <PLAN_KEY>
```

### get-builds
```bash
dotnet run -- bamboo get-builds --key <PLAN_KEY>
dotnet run -- bamboo get-builds --key PROJ-PLAN --max-results 10
```

### get-build
```bash
dotnet run -- bamboo get-build --key <BUILD_KEY>
dotnet run -- bamboo get-build -k PROJ-PLAN-123
```

### get-latest-build
```bash
dotnet run -- bamboo get-latest-build --key <PLAN_KEY>
```

### queue-build
```bash
dotnet run -- bamboo queue-build --key <PLAN_KEY>
dotnet run -- bamboo queue-build --key PROJ-PLAN --branch feature/my-branch
```

### get-build-logs
```bash
dotnet run -- bamboo get-build-logs --key <BUILD_KEY>
dotnet run -- bamboo get-build-logs --key PROJ-PLAN-123 -f error
dotnet run -- bamboo get-build-logs --key PROJ-PLAN-123 -f error -f exception
```

## BitBucket Commands

### get-pr
```bash
dotnet run -- bitbucket get-pr --project <PROJECT> --repo <REPO> --id <PR_ID>
dotnet run -- bitbucket get-pr -p PROJ -r my-repo -i 123
```

### get-pr-diff
```bash
dotnet run -- bitbucket get-pr-diff --project <PROJECT> --repo <REPO> --id <PR_ID>
dotnet run -- bitbucket get-pr-diff -p PROJ -r my-repo -i 123
```

### get-pr-commits
```bash
dotnet run -- bitbucket get-pr-commits --project <PROJECT> --repo <REPO> --id <PR_ID>
dotnet run -- bitbucket get-pr-commits -p PROJ -r my-repo -i 123
```

### get-pr-comments
```bash
dotnet run -- bitbucket get-pr-comments --project <PROJECT> --repo <REPO> --id <PR_ID>
dotnet run -- bitbucket get-pr-comments -p PROJ -r my-repo -i 123
```

### add-pr-comment
```bash
dotnet run -- bitbucket add-pr-comment --project <PROJECT> --repo <REPO> --id <PR_ID> --text "<COMMENT>"
dotnet run -- bitbucket add-pr-comment -p PROJ -r my-repo -i 123 -t "Looks good!"
dotnet run -- bitbucket add-pr-comment --project PROJ --repo my-repo --id 123 --file comment.txt
```
