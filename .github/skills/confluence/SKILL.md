---
name: confluence
description: Interact with Confluence via its REST API. Use this skill when asked to create, retrieve, or update Confluence pages.
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

## Operations

### Create a page

```
POST $CONFLUENCE_BASE_URL/rest/api/content
Content-Type: application/json

{
  "type": "page",
  "title": "<title>",
  "space": { "key": "<spaceKey>" },
  "body": {
    "storage": {
      "value": "<body content in XHTML storage format>",
      "representation": "storage"
    }
  }
}
```

Returns the created page including `id`, `title`, `space.key`, and `_links.webui`.

### Get a page by ID

```
GET $CONFLUENCE_BASE_URL/rest/api/content/{pageId}?expand=body.storage,body.view,version,space
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

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
