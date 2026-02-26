---
name: bamboo
description: Interact with Bamboo via its REST API. Use this skill when asked to list or inspect Bamboo projects, plans, branches, build results, deployment projects, environments, or releases, to trigger a build or deployment, retrieve build logs, or manage build artifacts.
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

## Projects

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

## Plans

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

### Get a specific branch

```
GET $BAMBOO_BASE_URL/rest/api/latest/plan/{planKey}/branch/{branchName}
```

### Create a branch plan

```
POST $BAMBOO_BASE_URL/rest/api/latest/plan/{planKey}/branch/{branchName}?vcsBranch={vcsBranchName}
```

Creates a new branch plan for the given VCS branch.

## Build Results

### List build results for a plan

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{planKey}?expand=results.result&max-result={maxResults}
```

Optional filter: `?buildstate=successful` or `?buildstate=failed`.

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

### Add a label to a build result

```
POST $BAMBOO_BASE_URL/rest/api/latest/result/{buildResultKey}/label
Content-Type: application/json

{ "name": "<labelName>" }
```

### Add a comment to a build result

```
POST $BAMBOO_BASE_URL/rest/api/latest/result/{buildResultKey}/comment
Content-Type: application/json

{ "content": "<comment text>" }
```

## Build Artifacts

### List artifacts for a build result

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{buildResultKey}/artifact
```

Returns a list of artifacts including their names and download links.

## Queue (Trigger Builds)

### Queue a build for the default branch

```
POST $BAMBOO_BASE_URL/rest/api/latest/queue/{planKey}
```

### Queue a build for a specific branch

```
POST $BAMBOO_BASE_URL/rest/api/latest/queue/{planKey}/branch/{encodedBranchName}
```

URL-encode the branch name. Both endpoints return a queue response with `buildNumber`, `buildResultKey`, `triggerReason`, and `link.href`.

### Queue a build with custom variables

```
POST $BAMBOO_BASE_URL/rest/api/latest/queue/{planKey}?bamboo.variable.myVar=myValue
```

Pass build variables as query parameters with the `bamboo.variable.` prefix.

## Build Logs

### Via JSON (log entries in result)

```
GET $BAMBOO_BASE_URL/rest/api/latest/result/{buildResultKey}?expand=logEntries&max-result=10000
```

If `logEntries.logEntry` is present and non-empty, join the `log` field of each entry with newlines.

### Download raw log file

```
GET $BAMBOO_BASE_URL/download/{buildResultKey}/build_logs/{buildResultKey}.log
```

If that fails, try:

```
GET $BAMBOO_BASE_URL/browse/{buildResultKey}/log
```

### Job-specific logs

```
GET $BAMBOO_BASE_URL/download/{buildResultKey}/build_logs/{jobKey}.log
```

`jobKey` is the key of the individual job within the build (e.g. `MYPROJ-MYPLAN-JOB1`).

## Deployment Projects

### List all deployment projects

```
GET $BAMBOO_BASE_URL/rest/api/latest/deploy/project/all
```

Returns a list of deployment projects, each with `id`, `name`, and `planKey`.

### Get a specific deployment project

```
GET $BAMBOO_BASE_URL/rest/api/latest/deploy/project/{deploymentProjectId}
```

Returns deployment project details including its environments.

## Environments

### List environments for a deployment project

```
GET $BAMBOO_BASE_URL/rest/api/latest/deploy/project/{deploymentProjectId}
```

Environments are returned under the `environments` array, each with `id`, `name`, and `deploymentProjectId`.

## Release Versions

### List release versions for a deployment project

```
GET $BAMBOO_BASE_URL/rest/api/latest/deploy/project/{deploymentProjectId}/versions
```

Returns release versions with their `id`, `name`, `creationDate`, and associated `planResultKey`.

### Create a release version

```
POST $BAMBOO_BASE_URL/rest/api/latest/deploy/project/{deploymentProjectId}/version
Content-Type: application/json

{
  "planResultKey": "<buildResultKey>",
  "name": "release-1.0"
}
```

The `name` field supports Bamboo plan variable substitution. Use `${bamboo.variable.name}` to dynamically insert plan variable values into the release name. For example: `"name": "release-${bamboo.buildNumber}"` creates a name like `release-42`.

## Deployments

### Trigger a deployment

To deploy a release to an environment:

1. Get the deployment project ID (from list or get deployment project)
2. Get the environment ID from the deployment project's `environments`
3. Get the version ID (from list release versions or create release)

```
POST $BAMBOO_BASE_URL/rest/api/latest/queue/deployment?environmentId={environmentId}&versionId={versionId}
```

Returns `deploymentResultId` and `link.href` to the result resource.

### Check deployment status

```
GET $BAMBOO_BASE_URL/rest/api/latest/deploy/result/{deploymentResultId}
```

Key response fields: `deploymentState` (`SUCCESS`, `FAILED`, `UNKNOWN`), `lifeCycleState` (`QUEUED`, `IN_PROGRESS`, `FINISHED`), `startedDate`, `finishedDate`.

### List deployments to an environment

```
GET $BAMBOO_BASE_URL/rest/api/latest/deploy/environment/{environmentId}/results
```

Returns recent deployment results for the environment.

## Deployment Queue

### View the deployment queue

```
GET $BAMBOO_BASE_URL/rest/api/latest/queue/deployment
```

### View queued deployment details

```
GET $BAMBOO_BASE_URL/rest/api/latest/queue/deployment?expand=queuedDeployments
```

Returns details of all queued deployments including their `deploymentResultId`.

### Remove a deployment from the queue

```
DELETE $BAMBOO_BASE_URL/rest/api/latest/queue/deployment/{deploymentResultId}
```

Returns HTTP 204 on success.

## Build Queue

### View the build queue

```
GET $BAMBOO_BASE_URL/rest/api/latest/queue
```

Returns builds currently in the queue. Add `?expand=queuedBuilds` to see details of each queued build.

## Error handling

On non-2xx responses, include the HTTP status code, reason phrase, and response body in the error message.
