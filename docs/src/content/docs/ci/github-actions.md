---
title: GitHub Actions
description: Use the dotnet-affected action to build and test only what changed.
sidebar:
  order: 2
---

[`leonardochaia/dotnet-affected-action`](https://github.com/leonardochaia/dotnet-affected-action) installs and runs the
tool for you, and exposes the result as a step output so later steps can be skipped.

## Inputs and outputs

| Input           | Description                                                       |
|-----------------|--------------------------------------------------------------------|
| `toolVersion`   | Version of dotnet-affected to install                              |
| `from`          | Commit, branch or tag to compare from                              |
| `to`            | Commit, branch or tag to compare to                                |
| `exclude`       | .NET regex of projects to ignore                                   |
| `solution-path` | Solution file used to discover projects                            |
| `output-format` | Space-separated formats, e.g. `text traversal`                     |

| Output     | Description                                        |
|------------|-----------------------------------------------------|
| `affected` | The changed and affected projects, empty if none    |

## Pull requests

Compare the base branch against the PR head:

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0

- uses: leonardochaia/dotnet-affected@v1
  id: affected
  with:
    from: origin/${{ github.base_ref }}
    to: ${{ github.sha }}

- name: Restore
  if: success() && steps.affected.outputs.affected != ''
  run: dotnet restore affected.proj

- name: Test
  if: success() && steps.affected.outputs.affected != ''
  run: dotnet test affected.proj
```

`fetch-depth: 0` is not optional — with the default shallow clone the base ref is missing and the comparison fails.

## Branch builds

For branches there is no obvious "other side" of the comparison, so use the last commit that built successfully:

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0

- uses: nrwl/last-successful-commit-action@v1
  id: last_successful_commit
  with:
    github_token: ${{ secrets.GITHUB_TOKEN }}
    branch: ${{ github.ref_name }}
    workflow_id: 'build.yml'

- uses: leonardochaia/dotnet-affected@v1
  id: affected
  with:
    from: ${{ steps.last_successful_commit.outputs.commit_hash }}
    to: ${{ github.sha }}
```

:::note
Guarding steps on `steps.affected.outputs.affected != ''` is the Actions equivalent of checking for exit code
[`166`](/reference/exit-codes/): it keeps the job green while skipping the work.
:::

Complete, maintained examples for both cases live in the
[action's README](https://github.com/leonardochaia/dotnet-affected-action#readme).
