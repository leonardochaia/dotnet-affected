---
title: GitHub Action
description: Run dotnet-affected in GitHub Actions and skip the steps that have nothing to build.
sidebar:
  order: 1
---

[`leonardochaia/dotnet-affected-action`](https://github.com/leonardochaia/dotnet-affected-action) installs
dotnet-affected, runs it, and exposes the result as a step output so later steps can be skipped.

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0

- uses: leonardochaia/dotnet-affected-action@v7
  id: affected
  with:
    from: ${{ steps.base.outputs.sha }}

- name: Test
  if: success() && steps.affected.outputs.affected != ''
  run: dotnet test affected.proj
```

`fetch-depth: 0` is not optional. The default shallow clone has neither the baseline nor a merge base, and the
comparison fails.

## Versioning

**The action major tracks the dotnet-affected major it drives.** Use `@v7` with dotnet-affected 7.x — it installs the
latest 7.x by default and refuses to run against a newer major.

`@v1` targets 6.x and is **deprecated**: `v1.5` is its last release, and every run logs a warning naming the version to
move to. It is a warning, not an error, so it cannot fail a build.

The reason for the rule: `@v1` used to install whatever dotnet-affected was newest on NuGet, so it would pick up a
major it was never written against the moment one was published — arguments and exit codes included. Tying the action
major to the tool major is what stops that.

:::caution[If you pin to `@v1.4` or earlier]
Those tags are immutable and install an unpinned dotnet-affected, so they will pull in 7.x as soon as it is published
— a v6-era action driving a v7 tool. Either move to `@v7`, or hold the tool back explicitly:

```yaml
- uses: leonardochaia/dotnet-affected-action@v1.4
  with:
    toolVersion: '6.*'
```

`toolVersion` has existed since v1.0.0, so it works on every published version.
:::

## What it does

1. Installs the tool globally with `dotnet tool install -g dotnet-affected --version <toolVersion>`, and puts
   `~/.dotnet/tools` on the path. A tool that is already installed is tolerated rather than treated as a failure.
2. Reads back `dotnet affected --version` and stops if the major is newer than the one the action targets. A version
   it cannot parse is let through — this check must never be the thing that breaks a run that would otherwise work.
3. Runs `dotnet affected` with the flags built from the inputs, writing `affected.proj` and `affected.txt` into the
   workspace.
4. Sets the `affected` output to the contents of `affected.txt`.

The [reference](/github-action/reference/) has the exact mapping from inputs to CLI flags.

## Skipping the rest of the job

When nothing changed and nothing is affected, the tool exits with [`166`](/reference/exit-codes/), which the action
treats as success with an **empty `affected` output** — the job stays green and nothing is built.

Guard the steps that would otherwise fail on a missing `affected.proj`:

```yaml
- name: Restore
  if: success() && steps.affected.outputs.affected != ''
  run: dotnet restore affected.proj
```

Any other non-zero exit fails the step, with the tool's stderr in the log.

See [Examples](/github-action/examples/) for complete pull-request and branch-build workflows.
