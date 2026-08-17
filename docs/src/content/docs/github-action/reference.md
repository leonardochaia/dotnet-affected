---
title: Action reference
description: Every input and output of the dotnet-affected GitHub Action, and the CLI flags they map to.
sidebar:
  order: 2
---

## Inputs

Every input is optional.

| Input              | Sends                           | Description                                                     |
|--------------------|---------------------------------|------------------------------------------------------------------|
| `toolVersion`      | `dotnet tool install --version` | Version of dotnet-affected to install. Defaults to `7.*`, the major `@v7` targets. Takes NuGet range syntax |
| `repository-path`  | `--repository-path`             | The checkout to analyse. Defaults to the workspace. Output files are written here |
| `from`             | `--from`                        | The baseline to compare the working tree against. Defaults to the checked-out commit |
| `uncommitted`      | `--uncommitted`                 | `all`, `staged` or `none` — see [the default](#the-uncommitted-default) |
| `filter-file-path` | `--filter-file-path`            | Filter file used to discover projects: `.sln`, `.slnx` or `.slnf` |
| `exclude-output`   | `--exclude-output`              | .NET regex; matching projects are evaluated and still affect their dependents, but stay out of the output |
| `exclude-discovery`| `--exclude-discovery`           | .NET regex; matching projects are never loaded                   |
| `no-gitignore`     | `--no-gitignore`                | Discover projects inside paths git ignores. Defaults to `false`  |
| `output-format`    | `--format`                      | Space-separated: `text`, `traversal`, `json`, `slnf`. Defaults to `text traversal` |

### Deprecated inputs

Kept so no workflow breaks on the upgrade. Each one logs a warning, and each loses to its replacement when both are
set.

| Input           | Sends                | Behaviour                                                          |
|-----------------|----------------------|---------------------------------------------------------------------|
| `to`            | *nothing*            | Warns and is ignored — see [below](#the-to-input)                   |
| `solution-path` | `--filter-file-path` | Warns; `filter-file-path` wins if both are set                      |
| `exclude`       | `--exclude-output`   | Warns; `exclude-output` wins if both are set                        |

### The `uncommitted` default

The tool defaults to `all`, which suits a developer running it on a checkout they are editing. A CI job is not that:
restore, code generation and version stamping all run before this step, and every one of those writes would otherwise
count as a change. So the action picks:

| `from` | Default    | Why                                                                             |
|--------|------------|----------------------------------------------------------------------------------|
| set    | `none`     | Compare the commits alone, so earlier steps cannot change which projects are reported |
| unset  | `all`      | `--from` then defaults to the checked-out commit; ignoring the working tree too would compare that commit with itself and report nothing on every run |

Set the input explicitly to override either.

### The `to` input

Deprecated, warns, and **is not passed to the tool at all**. dotnet-affected 7 ends every comparison at the working
tree, which Actions has already checked out at the commit being built, so the input has nothing left to name.

It is accepted rather than removed so that a workflow which has not migrated yet gets one clear message instead of a
parse error. Drop it and use `uncommitted` — see
[Choosing what to compare](/guides/commit-ranges/#the---to-option).

### `output-format` and the `affected` output

The `affected` output is read from `affected.txt`, so it is only set when the format list contains `text`. The default
(`text traversal`) covers both: a file list for the output, and `affected.proj` for `dotnet build`.

Asking for `traversal` alone leaves `affected` empty on every run, which reads exactly like "nothing was affected" —
and step conditions built on it silently skip everything.

`slnf` needs a solution to reference, so pair it with `filter-file-path`. The tool refuses the combination otherwise,
while parsing arguments.

## Outputs

| Output     | Description                                                              |
|------------|---------------------------------------------------------------------------|
| `affected` | Contents of `affected.txt`: one project path per line. Empty when nothing changed |

## Files written

Into `repository-path`, which defaults to the workspace:

| File            | When                                     |
|-----------------|-------------------------------------------|
| `affected.proj` | `traversal` is in the format list          |
| `affected.txt`  | `text` is in the format list               |
| `affected.json` | `json` is in the format list               |
| `affected.slnf` | `slnf` is in the format list               |

Nothing is written when nothing changed — the run exits `166` first.

## Version guard

After installing, the action reads back `dotnet affected --version` and stops if the major is newer than the one it
targets, naming the action version to move to. A version it cannot parse is let through: this check must never be what
breaks a run that would otherwise have worked.

Older majors are allowed, so pinning `toolVersion` to an earlier release still works.

## Exit-code handling

| Tool exit     | Action behaviour                                                  |
|---------------|--------------------------------------------------------------------|
| `0`           | Sets `affected` from `affected.txt`                                |
| `166`         | Succeeds with an empty `affected` output — nothing changed         |
| anything else | Logs the tool's stderr and fails the step                          |

## Requirements

- **`fetch-depth: 0` on `actions/checkout`.** A shallow clone lacks the baseline and any merge base.
- **A .NET SDK on the runner.** GitHub's hosted images ship one; with `actions/setup-dotnet`, run it before this action
  so `dotnet tool install` has something to install into.
- **`$GITHUB_WORKSPACE`** — the action fails without it, which in practice means it only runs inside Actions.
