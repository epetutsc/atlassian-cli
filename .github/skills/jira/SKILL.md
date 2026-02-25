---
name: jira
description: Interact with Jira via its REST API. Use this skill when asked to get, create, or update Jira issues, add comments, change issue status, or assign users.
---

Use the Jira REST API directly to perform Jira operations. All configuration is provided through environment variables.

## Authentication

Read the following environment variables to build the `Authorization` header:

- `JIRA_BASE_URL` – base URL of the Jira instance, e.g. `https://jira.example.com` (required)
- `JIRA_API_TOKEN` – API token or Personal Access Token
- `JIRA_USERNAME` – username (required when using an API token for Atlassian Cloud or basic auth)
- `JIRA_PASSWORD` – password (only for basic username/password auth)

Choose the authentication method based on which variables are set:

| Variables set | Auth header |
|---|---|
| `JIRA_API_TOKEN` only | `Authorization: Bearer <JIRA_API_TOKEN>` |
| `JIRA_USERNAME` + `JIRA_API_TOKEN` | `Authorization: Basic base64(<JIRA_USERNAME>:<JIRA_API_TOKEN>)` |
| `JIRA_USERNAME` + `JIRA_PASSWORD` | `Authorization: Basic base64(<JIRA_USERNAME>:<JIRA_PASSWORD>)` |

Always add `Accept: application/json` and, for write requests, `Content-Type: application/json`.

## Operations

### Get an issue

```
GET $JIRA_BASE_URL/rest/api/2/issue/{issueKey}?expand=renderedFields
```

Returns full issue details including summary, status, assignee, reporter, priority, description, and comments.

### Create an issue

```
POST $JIRA_BASE_URL/rest/api/2/issue
Content-Type: application/json

{
  "fields": {
    "project": { "key": "<projectKey>" },
    "summary": "<summary>",
    "issuetype": { "name": "<issueType>" },
    "description": "<description>"   // optional
  }
}
```

Returns `key`, `id`, and `self` (URL) of the newly created issue.

### Add a comment to an issue

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/comment
Content-Type: application/json

{
  "body": "<comment text>"
}
```

Returns the created comment including its `id` and `created` timestamp.

### Change issue status (transition)

First, retrieve available transitions:

```
GET $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/transitions
```

Find the transition whose `name` (or `to.name`) matches the desired status (case-insensitive). Then perform the transition:

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/transitions
Content-Type: application/json

{
  "transition": { "id": "<transitionId>" }
}
```

### Assign a user to an issue

First, search for the user:

```
GET $JIRA_BASE_URL/rest/api/2/user/search?query={username}
```

Use the returned `accountId` (Cloud) or `name` (Server) in the assignment request:

```
PUT $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/assignee
Content-Type: application/json

{
  "accountId": "<accountId>",   // Atlassian Cloud
  "name": "<name>"              // Jira Server / Data Center
}
```

If no user is found by search, pass the provided username directly as `name`.

### Update an issue description

```
PUT $JIRA_BASE_URL/rest/api/2/issue/{issueKey}
Content-Type: application/json

{
  "fields": {
    "description": "<new description>"
  }
}
```

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
