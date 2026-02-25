---
name: confluence
description: Interact with Confluence via its REST API. Use this skill when asked to create, retrieve, update, or delete Confluence pages or blog posts, search content with CQL, manage spaces, labels, attachments, comments, or child pages.
---

Use the Confluence REST API directly to perform Confluence operations. All configuration is provided through environment variables.

## Authentication

Read the following environment variables to build the `Authorization` header:

- `CONFLUENCE_BASE_URL` – base URL of the Confluence instance, e.g. `https://confluence.example.com` (required)
- `CONFLUENCE_API_TOKEN` – API token or Personal Access Token
- `CONFLUENCE_USERNAME` – username (required when using an API token for Atlassian Cloud or basic auth)
- `CONFLUENCE_PASSWORD` – password (only for basic username/password auth)

Choose the authentication method based on which variables are set:

| Variables set | Auth header |
|---|---|
| `CONFLUENCE_API_TOKEN` only | `Authorization: Bearer <CONFLUENCE_API_TOKEN>` |
| `CONFLUENCE_USERNAME` + `CONFLUENCE_API_TOKEN` | `Authorization: Basic base64(<CONFLUENCE_USERNAME>:<CONFLUENCE_API_TOKEN>)` |
| `CONFLUENCE_USERNAME` + `CONFLUENCE_PASSWORD` | `Authorization: Basic base64(<CONFLUENCE_USERNAME>:<CONFLUENCE_PASSWORD>)` |

Always add `Accept: application/json` and, for write requests, `Content-Type: application/json`.

## Page Operations

### Create a page

```
POST $CONFLUENCE_BASE_URL/rest/api/content
Content-Type: application/json

{
  "type": "page",
  "title": "<title>",
  "space": { "key": "<spaceKey>" },
  "ancestors": [{ "id": "<parentPageId>" }],   // optional; only one parent supported
  "body": {
    "storage": {
      "value": "<body content in XHTML storage format>",
      "representation": "storage"
    }
  }
}
```

Omit `ancestors` to create a top-level page. Returns the created page including `id`, `title`, `space.key`, and `_links.webui`.

### Get a page by ID

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}?expand=body.storage,body.view,version,space,ancestors
```

Returns page details including content in storage (XHTML) and view (rendered HTML) representations.

### Get a page by title and space key

```
GET $CONFLUENCE_BASE_URL/rest/api/content?spaceKey={spaceKey}&title={encodedTitle}&expand=body.storage,body.view,version,space
```

Returns a list of matching pages under `results`. Use the first result if the page exists.

### Update a page by ID

First, retrieve the current page to get the current version number and title:

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}?expand=body.storage,version
```

Then update the page, incrementing `version.number` by 1:

```
PUT $CONFLUENCE_BASE_URL/rest/api/content/{pageId}
Content-Type: application/json

{
  "id": "<pageId>",
  "type": "page",
  "title": "<existing title>",
  "version": { "number": <currentVersion + 1> },
  "body": {
    "storage": {
      "value": "<new body content>",
      "representation": "storage"
    }
  }
}
```

To **append** content instead of replacing it, concatenate the existing `body.storage.value` with the new content before sending.

### Update a page by title and space key

1. Get the page by title and space key (see above) to retrieve its `id`.
2. Perform the update by ID (see above).

### Delete a page

```
DELETE $CONFLUENCE_BASE_URL/rest/api/content/{pageId}
```

Returns HTTP 204 on success.

### Get page history / versions

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/history
```

Returns the history including created/updated dates and users.

## Child Pages

### List child pages

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/child/page
```

Returns child pages under `results`, each with `id`, `title`, and `_links`.

### Get all content children (pages, attachments, comments)

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/child
```

Returns all child content types grouped by type.

## Comments

### List page comments

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/child/comment?expand=body.view,version
```

### Add a comment to a page

```
POST $CONFLUENCE_BASE_URL/rest/api/content
Content-Type: application/json

{
  "type": "comment",
  "container": { "id": "<pageId>", "type": "page" },
  "body": {
    "storage": {
      "value": "<comment content>",
      "representation": "storage"
    }
  }
}
```

### Delete a comment

```
DELETE $CONFLUENCE_BASE_URL/rest/api/content/{commentId}
```

## Labels

### Get labels on a page

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/label
```

### Add labels to a page

```
POST $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/label
Content-Type: application/json

[
  { "prefix": "global", "name": "<label1>" },
  { "prefix": "global", "name": "<label2>" }
]
```

### Remove a label from a page

```
DELETE $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/label/{labelName}
```

## Attachments

### List attachments on a page

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/child/attachment
```

### Add an attachment to a page

```
POST $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/child/attachment
X-Atlassian-Token: no-check
Content-Type: multipart/form-data

<file as multipart field named "file">
```

### Update attachment data (new version)

```
POST $CONFLUENCE_BASE_URL/rest/api/content/{pageId}/child/attachment/{attachmentId}/data
X-Atlassian-Token: no-check
Content-Type: multipart/form-data

<file as multipart field named "file">
```

## Spaces

### List all spaces

```
GET $CONFLUENCE_BASE_URL/rest/api/space
```

Optional filters: `?type=global` (type: `global` or `personal`), `?status=current`.

### Get a space by key

```
GET $CONFLUENCE_BASE_URL/rest/api/space/{spaceKey}?expand=description.plain,homepage
```

### Get all content in a space

```
GET $CONFLUENCE_BASE_URL/rest/api/space/{spaceKey}/content
```

## Search

### Search content with CQL

```
GET $CONFLUENCE_BASE_URL/rest/api/content/search?cql={encodedCQL}&limit={limit}&start={start}
```

CQL examples:
- Pages in a space: `space="KEY" AND type=page`
- Pages with a label: `label="meeting-notes" AND type=page`
- Pages by ancestor: `ancestor=<pageId>`
- Recently updated pages: `type=page AND space="KEY" ORDER BY lastmodified DESC`

Returns matching content under `results` with `start`, `limit`, and `size` for pagination.

### Full-text search across spaces

```
GET $CONFLUENCE_BASE_URL/rest/api/search?cql={encodedCQL}&limit={limit}&start={start}
```

Returns all content types (pages, blog posts, comments, attachments) matching the CQL query.

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
