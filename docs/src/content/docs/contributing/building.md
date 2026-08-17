---
title: Building the project
description: Set up the pinned .NET SDK and build dotnet-affected from source.
sidebar:
  order: 1
---

Issues and pull requests are welcome.

:::note
On Windows, use WSL or Git Bash, or run the PowerShell equivalents — every `.sh` script below has a `.ps1` sibling in
`eng/`.
:::

## Install the SDK

The repository pins its .NET SDK in `global.json` and installs it locally, so it does not interfere with anything else
on your machine:

```bash
./eng/install-sdk.sh
```

The SDK lands in `./eng/.dotnet`, along with the older runtimes the test suite needs.

## Activate your shell

```bash
. ./eng/activate.sh
```

This puts the local SDK first on `PATH` and sets `DOTNET_ROOT`. Confirm with `dotnet --info`; run `deactivate` to undo
it.

```bash
dotnet build
```

Launching your IDE from the activated shell makes it use the same SDK:

```bash
. ./eng/activate.sh
rider Affected.sln
```

## Test

```bash
dotnet test
```

The suite creates temporary Git repositories and runs real MSBuild evaluations, so it is slower than a unit-test-only
run and needs `git` available.

## Layout

| Path                          | What it is                                                    |
|-------------------------------|----------------------------------------------------------------|
| `src/dotnet-affected`         | The CLI: commands, options, views and output formatters        |
| `src/DotnetAffected.Core`     | Discovery, Git change detection, NuGet diffing, graph walking  |
| `src/DotnetAffected.Abstractions` | Interfaces and DTOs shared by the CLI, SDK and tests       |
| `src/DotnetAffected.Tasks`    | The MSBuild SDK — task, `Sdk.props`/`Sdk.targets`, examples    |
| `test/`                       | Test projects plus the shared temporary-repository helpers     |
| `benchmarks/`                 | BenchmarkDotNet projects                                       |
| `docs/`                       | This documentation site                                        |
| `eng/`                        | SDK install/activate scripts and packaging helpers             |

## Working on the docs

The site is an Astro Starlight project under `docs/`. See
[`docs/README.md`](https://github.com/leonardochaia/dotnet-affected/blob/main/docs/README.md) — it includes a
container-based workflow, so you don't need Node installed to run it.
