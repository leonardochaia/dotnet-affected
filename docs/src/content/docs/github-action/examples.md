---
title: Action examples
description: Complete pull-request and branch-build workflows using the dotnet-affected action.
sidebar:
  order: 3
---

## Pull requests

Compare from the **merge base** — the commit the branch was actually cut from:

```yaml
name: PR
on:
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Merge base
        id: base
        run: echo "sha=$(git merge-base origin/${{ github.base_ref }} HEAD)" >> "$GITHUB_OUTPUT"

      - uses: leonardochaia/dotnet-affected-action@v7
        id: affected
        with:
          from: ${{ steps.base.outputs.sha }}

      - name: Restore
        if: success() && steps.affected.outputs.affected != ''
        run: dotnet restore affected.proj

      - name: Build
        if: success() && steps.affected.outputs.affected != ''
        run: dotnet build --configuration Release --no-restore affected.proj

      - name: Test
        if: success() && steps.affected.outputs.affected != ''
        run: dotnet test --no-restore --verbosity normal affected.proj
```

Not the base branch's tip. `github.event.pull_request.base.sha` is the tip at the time the PR was opened; if the base
branch has moved since, comparing from it sweeps in every unrelated commit that landed in the meantime.

## Branch builds

For a branch there is no obvious baseline, so use the last commit that built successfully. A build covers a push
rather than a commit, and previous builds may have failed, so the baseline has to reach further back than "the
previous commit".

```yaml
name: CI
on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - uses: nrwl/last-successful-commit-action@v1
        id: last_successful_commit
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          branch: ${{ github.ref_name }}
          workflow_id: 'ci.yml'

      - uses: leonardochaia/dotnet-affected-action@v7
        id: affected
        with:
          from: ${{ steps.last_successful_commit.outputs.commit_hash }}

      - name: Test
        if: success() && steps.affected.outputs.affected != ''
        run: dotnet test affected.proj
```

[nrwl/last-successful-commit-action](https://github.com/nrwl/last-successful-commit-action) explains
[the problem](https://github.com/nrwl/last-successful-commit-action#problem) it solves.
[`nrwl/nx-set-shas`](https://github.com/nrwl/nx-set-shas) covers both cases in one step, exposing `base` and `head`
outputs — use its `base` as `from`, and ignore `head`, since the workspace is already checked out at it.

## Picking a specific tool version

```yaml
- uses: leonardochaia/dotnet-affected-action@v7
  with:
    toolVersion: '7.0.1'
```

The input takes NuGet range syntax, so `'7.*'` pins the major while following patches. Pinning an exact version makes
CI reproducible; pinning a *different major* than the action targets will be refused.

## Narrowing what is analysed

```yaml
- uses: leonardochaia/dotnet-affected-action@v7
  id: affected
  with:
    from: ${{ steps.base.outputs.sha }}
    solution-path: MySolution.sln
    exclude: '\.Benchmarks\.csproj$'
```

`solution-path` also passes the workspace as `--repository-path`, so a solution living in a subdirectory still finds
the git root.

## Using a matrix

Run the analysis once and let every matrix leg read the same output:

```yaml
jobs:
  affected:
    runs-on: ubuntu-latest
    outputs:
      affected: ${{ steps.affected.outputs.affected }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - uses: leonardochaia/dotnet-affected-action@v7
        id: affected
        with:
          from: origin/main
      - uses: actions/upload-artifact@v4
        if: steps.affected.outputs.affected != ''
        with:
          name: affected
          path: affected.proj

  test:
    needs: affected
    if: needs.affected.outputs.affected != ''
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/download-artifact@v4
        with:
          name: affected
      - run: dotnet test affected.proj
```

:::caution
`affected.proj` contains **absolute** paths, so passing it between jobs only works when the workspace path is the same
on both runners. Across operating systems it is not — regenerate per leg, or use the `affected` output as a list and
build the paths yourself.
:::
