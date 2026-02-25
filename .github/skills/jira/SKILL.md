---
name: jira
description: Interact with Jira via its REST API. Use this skill when asked to get, create, update, or delete Jira issues, search with JQL, manage comments, worklogs, attachments, watchers, votes, issue links, projects, users, or work with Jira Agile boards and sprints.
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

## Issue Operations

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
    "description": "<description>",          // optional
    "priority": { "name": "<priority>" },    // optional
    "labels": ["<label1>", "<label2>"],      // optional
    "fixVersions": [{ "name": "<version>" }] // optional
  }
}
```

Returns `key`, `id`, and `self` (URL) of the newly created issue.

### Update an issue

Use `PUT` to update any combination of fields on an existing issue:

```
PUT $JIRA_BASE_URL/rest/api/2/issue/{issueKey}
Content-Type: application/json

{
  "fields": {
    "summary": "<new summary>",
    "description": "<new description>",
    "priority": { "name": "<priority>" },
    "labels": ["<label1>"],
    "fixVersions": [{ "name": "<version>" }]
  }
}
```

Returns HTTP 204 on success (no body).

### Delete an issue

```
DELETE $JIRA_BASE_URL/rest/api/2/issue/{issueKey}
```

Returns HTTP 204 on success. Add `?deleteSubtasks=true` to also delete subtasks.

### Search issues with JQL

```
GET $JIRA_BASE_URL/rest/api/2/search?jql={encodedJQL}&startAt={startAt}&maxResults={maxResults}&fields={fieldList}
```

Example: `jql=project=PROJ AND status="In Progress"&maxResults=50`

Returns matching issues under `issues`, with `total`, `startAt`, and `maxResults` for pagination. Use `startAt` to page through results.

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

## Comments

### Add a comment

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/comment
Content-Type: application/json

{ "body": "<comment text>" }
```

Returns the created comment including its `id` and `created` timestamp.

### List comments

```
GET $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/comment
```

### Update a comment

```
PUT $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/comment/{commentId}
Content-Type: application/json

{ "body": "<updated text>" }
```

### Delete a comment

```
DELETE $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/comment/{commentId}
```

## Worklogs (Time Tracking)

### List worklogs

```
GET $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/worklog
```

### Add a worklog

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/worklog
Content-Type: application/json

{
  "timeSpent": "2h 30m",
  "started": "2024-01-15T10:00:00.000+0000",
  "comment": "<optional comment>"
}
```

### Update a worklog

```
PUT $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/worklog/{worklogId}
Content-Type: application/json

{ "timeSpent": "3h", "comment": "<updated comment>" }
```

### Delete a worklog

```
DELETE $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/worklog/{worklogId}
```

## Attachments

### Add an attachment

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/attachments
X-Atlassian-Token: no-check
Content-Type: multipart/form-data

<file as multipart field named "file">
```

### Delete an attachment

```
DELETE $JIRA_BASE_URL/rest/api/2/attachment/{attachmentId}
```

## Watchers

### List watchers

```
GET $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/watchers
```

### Add a watcher

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/watchers
Content-Type: application/json

"<username>"
```

### Remove a watcher

```
DELETE $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/watchers?username={username}
```

## Votes

### Get vote information

```
GET $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/votes
```

### Add your vote

```
POST $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/votes
```

### Remove your vote

```
DELETE $JIRA_BASE_URL/rest/api/2/issue/{issueKey}/votes
```

## Issue Links

### Create an issue link

```
POST $JIRA_BASE_URL/rest/api/2/issueLink
Content-Type: application/json

{
  "type": { "name": "<linkType>" },
  "inwardIssue": { "key": "<issueKey1>" },
  "outwardIssue": { "key": "<issueKey2>" }
}
```

Common link types: `Blocks`, `Cloners`, `Duplicate`, `Relates`.

### Delete an issue link

First retrieve the link ID from the issue's `fields.issuelinks`, then:

```
DELETE $JIRA_BASE_URL/rest/api/2/issueLink/{linkId}
```

## Project Operations

### List all projects

```
GET $JIRA_BASE_URL/rest/api/2/project
```

### Get a project

```
GET $JIRA_BASE_URL/rest/api/2/project/{projectIdOrKey}
```

### Get project versions

```
GET $JIRA_BASE_URL/rest/api/2/project/{projectIdOrKey}/versions
```

### Get project components

```
GET $JIRA_BASE_URL/rest/api/2/project/{projectIdOrKey}/components
```

## Fields and Metadata

### List all fields

```
GET $JIRA_BASE_URL/rest/api/2/field
```

Returns all built-in and custom fields with their `id`, `name`, and `schema`.

### Get issue create metadata

```
GET $JIRA_BASE_URL/rest/api/2/issue/createmeta?projectKeys={projectKey}&issuetypeNames={issueType}&expand=projects.issuetypes.fields
```

Returns the fields required and available when creating an issue for a given project/issue type.

## Agile (Jira Software) API

These endpoints use the `/rest/agile/1.0/` base path.

### List boards

```
GET $JIRA_BASE_URL/rest/agile/1.0/board
```

Optional filters: `?projectKeyOrId={key}&type=scrum` (type can be `scrum` or `kanban`).

### Get a board

```
GET $JIRA_BASE_URL/rest/agile/1.0/board/{boardId}
```

### List sprints on a board

```
GET $JIRA_BASE_URL/rest/agile/1.0/board/{boardId}/sprint?state=active
```

State values: `future`, `active`, `closed`. Omit to get all sprints.

### Get a sprint

```
GET $JIRA_BASE_URL/rest/agile/1.0/sprint/{sprintId}
```

### Get issues in a sprint

```
GET $JIRA_BASE_URL/rest/agile/1.0/sprint/{sprintId}/issue
```

### Get issues on a board

```
GET $JIRA_BASE_URL/rest/agile/1.0/board/{boardId}/issue
```

### Get epics on a board

```
GET $JIRA_BASE_URL/rest/agile/1.0/board/{boardId}/epic
```

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
