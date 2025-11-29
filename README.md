# Confluence CLI

A .NET console application that provides a command-line interface to interact with a self-hosted Confluence (on-prem) instance via Confluence's REST API.

## Features

- **Create pages** in any Confluence space
- **Read pages** by ID or by title + space key
- **Update pages** with replace or append mode
- Support for Confluence storage format (XHTML)
- Environment variable-based configuration
- Comprehensive help system

## Requirements

- .NET 8.0 SDK or later
- Access to a Confluence instance with REST API enabled

## Configuration

Set the following environment variables before using the CLI:

| Variable | Required | Description |
|----------|----------|-------------|
| `CONFLUENCE_BASE_URL` | Yes | Base URL of your Confluence instance (e.g., `https://confluence.example.com`) |
| `CONFLUENCE_USERNAME` | Yes | Username for authentication |
| `CONFLUENCE_API_TOKEN` | Conditional | API token (recommended - use this OR password) |
| `CONFLUENCE_PASSWORD` | Conditional | Password (use this OR API token) |

### Example Configuration

```bash
export CONFLUENCE_BASE_URL=https://confluence.example.com
export CONFLUENCE_USERNAME=your.username
export CONFLUENCE_API_TOKEN=your-api-token
```

## Building

```bash
dotnet build
```

## Usage

### Show Help

```bash
dotnet run -- --help
```

### Create a Page

```bash
dotnet run -- create-page --space MYSPACE --title "My New Page" --body "<p>Hello Confluence!</p>"
```

Options:
- `-s, --space` (required): Space key
- `-t, --title` (required): Page title
- `-b, --body` (required): Page content in storage format

### Get a Page

By ID:
```bash
dotnet run -- get-page --id 12345
```

By title and space:
```bash
dotnet run -- get-page --space MYSPACE --title "My Page"
```

Options:
- `-i, --id`: Page ID
- `-s, --space`: Space key (required with --title)
- `-t, --title`: Page title (required with --space)
- `-f, --format`: Output format - `storage` (default) or `view`

### Update a Page

Replace content:
```bash
dotnet run -- update-page --id 12345 --body "<p>New content</p>"
```

Append content:
```bash
dotnet run -- update-page --id 12345 --body "<p>Additional content</p>" --append
```

Update by title:
```bash
dotnet run -- update-page --space MYSPACE --title "My Page" --body "<p>Updated</p>"
```

Options:
- `-i, --id`: Page ID
- `-s, --space`: Space key (required with --title)
- `-t, --title`: Page title (required with --space)
- `-b, --body` (required): New page content
- `-a, --append`: Append to existing content instead of replacing

## Project Structure

```
ConfluenceCli/
├── Program.cs                    # Entry point, CLI parsing
├── Client/
│   └── ConfluenceClient.cs       # REST API client
├── Models/
│   └── ConfluenceModels.cs       # Data transfer objects
├── Commands/
│   ├── CommandOptions.cs         # CLI option definitions
│   └── CommandHandlers.cs        # Command execution logic
└── Progress.md                   # Development progress tracking
```

## License

MIT License