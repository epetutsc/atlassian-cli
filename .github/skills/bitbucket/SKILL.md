---
name: bitbucket
description: Interact with Bitbucket Server / Data Center via its REST API. Use this skill when asked to get pull request details, diffs, commits, or comments, or to add a comment to a pull request.
---

Use the Bitbucket REST API directly to perform Bitbucket operations. All configuration is provided through environment variables.

## Authentication

Read the following environment variables to build the `Authorization` header:

- `BITBUCKET_BASE_URL` – base URL of the Bitbucket instance, e.g. `https://bitbucket.example.com` (required)
- `BITBUCKET_API_TOKEN` – API token or Personal Access Token
- `BITBUCKET_USERNAME` – username (required when using an API token or basic auth)
- `BITBUCKET_PASSWORD` – password (only for basic username/password auth)

Choose the authentication method based on which variables are set:

| Variables set | Auth header |
|---|---|
| `BITBUCKET_API_TOKEN` only | `Authorization: Bearer <BITBUCKET_API_TOKEN>` |
| `BITBUCKET_USERNAME` + `BITBUCKET_API_TOKEN` | `Authorization: Basic base64(<BITBUCKET_USERNAME>:<BITBUCKET_API_TOKEN>)` |
| `BITBUCKET_USERNAME` + `BITBUCKET_PASSWORD` | `Authorization: Basic base64(<BITBUCKET_USERNAME>:<BITBUCKET_PASSWORD>)` |

Always add `Accept: application/json` and, for write requests, `Content-Type: application/json`.

## Operations

All pull request endpoints share the base path:
`$BITBUCKET_BASE_URL/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}`

### Get a pull request

```
GET $BITBUCKET_BASE_URL/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}
```

Returns full pull request details including `id`, `title`, `state`, `open`, `author`, `fromRef` (source branch), `toRef` (target branch), `reviewers`, `description`, and `links.self`.

### Get the diff of a pull request

```
GET $BITBUCKET_BASE_URL/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/diff
```

Returns a structured diff with `fromHash`, `toHash`, and an array of file diffs (`diffs`). Each file diff contains `source`, `destination`, and an array of `hunks`, where each hunk lists `segments` of type `ADDED`, `REMOVED`, or `CONTEXT`.

### Get commits in a pull request

Use pagination with `start` and `limit` query parameters (recommended limit: 100). Continue fetching pages until `isLastPage` is `true`.

```
GET $BITBUCKET_BASE_URL/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/commits?start={start}&limit=100
```

Returns a paged list under `values`. Each commit has `displayId`, `author` (name, emailAddress), `authorTimestamp`, and `message`.

### Get comments on a pull request

Retrieve all activities and filter those where `comment` is not null. Use pagination until `isLastPage` is `true`.

```
GET $BITBUCKET_BASE_URL/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/activities?start={start}&limit=100
```

Each activity with a `comment` contains the comment `text`, `author`, `createdDate`, and optionally a `commentAnchor` with `path` and `line` for inline comments. Nested replies are found in `comment.comments`.

### Add a comment to a pull request

```
POST $BITBUCKET_BASE_URL/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/comments
Content-Type: application/json

{
  "text": "<comment text>"
}
```

Returns the created comment including its `id` and `createdDate` (Unix milliseconds).

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
