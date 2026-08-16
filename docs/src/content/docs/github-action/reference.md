---
title: Action reference
description: Every input and output of the dotnet-affected GitHub Action, and the CLI flags they map to.
sidebar:
  order: 2
---

## Inputs

| Input           | Maps to                       | Description                                                        |
|-----------------|-------------------------------|---------------------------------------------------------------------|
| `toolVersion`   | `dotnet tool install --version` | Version of dotnet-affected to install. Defaults to the major the action targets — `7.*` for `@v7`. Accepts NuGet range syntax |
| `from`          | `--from`                      | The baseline to compare the working tree against                    |
| `to`            | `--to`                        | **Deprecated.** See [below](#the-to-input)                          |
| `solution-path` | `--solution-path` (+ `--repository-path`) | Solution used to discover projects. Also passes the workspace as the repository path |
| `exclude`       | `--exclude`                   | .NET regex of projects to keep out of the output                    |
| `output-format` | `--format`                    | Space-separated formats. Defaults to `text traversal`               |

Everything is optional. With no inputs, the action compares `HEAD` against the working tree and writes both output
files.

:::note[Inputs lag the CLI]
The action exposes a subset of the CLI's options. `--uncommitted`, `--filter-file-path`, `--exclude-discovery` and
`--no-gitignore` have no input yet; `solution-path` and `exclude` map to flags the CLI now marks obsolete
(`--filter-file-path` and `--exclude-output` are the current spellings, and the old ones still work as aliases).

If you need an option the action does not expose, install the tool yourself and call it directly — see
[Continuous integration](/ci/).
:::

### The `to` input

`--to` is deprecated in dotnet-affected 7 and accepted only when it names the commit the working tree is already
checked out at. Actions checks out the commit being built, so **leaving `to` unset is both correct and
forward-compatible**. Passing it logs a deprecation warning; passing anything else fails the run.

See [Choosing what to compare](/guides/commit-ranges/#the---to-option).

### `output-format` and the `affected` output

The `affected` output is read from `affected.txt`, so it is only set when the format list contains `text`. The default
(`text traversal`) covers both: a file list for the output, and `affected.proj` for `dotnet build`.

Asking for `--format traversal` alone leaves `affected` empty on every run, which reads exactly like "nothing was
affected" — and step conditions built on it silently skip everything.

## Outputs

| Output     | Description                                                              |
|------------|---------------------------------------------------------------------------|
| `affected` | Contents of `affected.txt`: one project path per line. Empty when nothing changed |

## Files written

Both land in `$GITHUB_WORKSPACE`, next to your checkout:

| File            | When                                     |
|-----------------|-------------------------------------------|
| `affected.proj` | `traversal` is in the format list          |
| `affected.txt`  | `text` is in the format list               |

Neither is written when nothing changed — the run exits `166` before any output is produced.

## Exit-code handling

| Tool exit | Action behaviour                                                     |
|-----------|-----------------------------------------------------------------------|
| `0`       | Sets `affected` from `affected.txt`                                   |
| `166`     | Succeeds with an empty `affected` output — nothing changed            |
| anything else | Logs the tool's stderr and fails the step                         |

## Requirements

- **`fetch-depth: 0` on `actions/checkout`.** A shallow clone lacks the baseline and any merge base.
- **A .NET SDK on the runner.** GitHub's hosted images ship one; with `actions/setup-dotnet`, run it before this action
  so `dotnet tool install` has something to install into.
- **`$GITHUB_WORKSPACE`** — the action fails without it, which in practice means it only runs inside Actions.
