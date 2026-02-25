---
name: bamboo
description: Interact with Bamboo via its REST API. Use this skill when asked to list or inspect Bamboo projects, plans, branches, or build results, to trigger (queue) a build, or to retrieve build logs.
---

Use the Bamboo REST API directly to perform Bamboo operations. All configuration is provided through environment variables.

## Authentication

Read the following environment variables to build the `Authorization` header:

- `BAMBOO_BASE_URL` – base URL of the Bamboo instance, e.g. `https://bamboo.example.com` (required)
- `BAMBOO_API_TOKEN` – API token or Personal Access Token
- `BAMBOO_USERNAME` – username (required when using an API token for Atlassian Cloud or basic auth)
- `BAMBOO_PASSWORD` – password (only for basic username/password auth)

Choose the authentication method based on which variables are set:

| Variables set | Auth header |
|---|---|
| `BAMBOO_API_TOKEN` only | `Authorization: Bearer <BAMBOO_API_TOKEN>` |
| `BAMBOO_USERNAME` + `BAMBOO_API_TOKEN` | `Authorization: Basic base64(<BAMBOO_USERNAME>:<BAMBOO_API_TOKEN>)` |
| `BAMBOO_USERNAME` + `BAMBOO_PASSWORD` | `Authorization: Basic base64(<BAMBOO_USERNAME>:<BAMBOO_PASSWORD>)` |

Always add `Accept: application/json`.

## Operations

### List all projects

```
GET $BAMBOO_BASE_URL/rest/api/latest/project?expand=projects.project.plans&max-result=1000
```

Returns a list of projects under `projects.project`, each with `key`, `name`, `description`, and `plans`.

### Get a specific project

```
GET $BAMBOO_BASE_URL/rest/api/latest/project/{projectKey}?expand=plans.plan
```

Returns project details including its plans.

### List all plans

```
GET $BAMBOO_BASE_URL/rest/api/latest/plan?max-result=1000
```

Returns a list of plans under `plans.plan`, each with `key`, `name`, `projectKey`, `projectName`, `enabled`, `isBuilding`, and `isActive`.

### Get a specific plan

```
GET $BAMBOO_BASE_URL/rest/api/latest/plan/{planKey}?expand=stages,branches,variableContext
```

Returns full plan details including stages, branches, and build variables.

### List branches for a plan

```
GET $BAMBOO_BASE_URL/rest/api/latest/plan/{planKey}/branch?max-result=1000
```

Returns a list of branches under `branches.branch`, each with `key`, `name`, and `enabled`.

### List build results for a plan

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{planKey}?expand=results.result&max-result={maxResults}
```

Returns build results under `results.result`, each with `buildNumber`, `key`, `successful`, `finished`, `lifeCycleState`, and `buildRelativeTime`.

### Get a specific build result

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{buildResultKey}?expand=stages.stage,changes.change
```

`buildResultKey` format: `{PROJECT}-{PLAN}-{buildNumber}`, e.g. `MYPROJ-MYPLAN-42`.

Returns full build result including stages, test counts (`successfulTestCount`, `failedTestCount`, `quarantinedTestCount`, `skippedTestCount`), changes, and timing fields.

### Get the latest build result for a plan

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{planKey}/latest?expand=stages.stage,changes.change
```

### Queue (trigger) a build

**Default branch:**

```
POST $BAMBOO_BASE_URL/rest/api/latest/queue/{planKey}
```

**Specific branch:**

```
POST $BAMBOO_BASE_URL/rest/api/latest/queue/{planKey}/branch/{encodedBranchName}
```

URL-encode the branch name. Returns queue response with `buildNumber`, `buildResultKey`, `triggerReason`, and `link.href`.

### Get build logs

**Via JSON (log entries in result):**

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{buildResultKey}?expand=logEntries&max-result=10000
```

If `logEntries.logEntry` is present and non-empty, join the `log` field of each entry with newlines.

**Fallback – download raw log file:**

```
GET $BAMBOO_BASE_URL/download/{buildResultKey}/build_logs/{buildResultKey}.log
```

If that fails, try:

```
GET $BAMBOO_BASE_URL/browse/{buildResultKey}/log
```

**Job-specific logs:**

```
GET $BAMBOO_BASE_URL/download/{buildResultKey}/build_logs/{jobKey}.log
```

`jobKey` is the key of the individual job within the build (e.g. `MYPROJ-MYPLAN-JOB1`).

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
