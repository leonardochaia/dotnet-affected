---
title: Output formats
description: Traversal, text and JSON output, and how to control the generated file names.
sidebar:
  order: 4
---

`--format` (`-f`) selects one or more output formats. Each format writes its own file, named after
[`--output-name`](/reference/cli/#--output-name) with the format's extension appended. The default is `traversal`.

## `traversal` → `affected.proj`

An [MSBuild Traversal SDK](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) project with one
`ProjectReference` per changed or affected project. This is the format you hand back to `dotnet build` or `dotnet test`
— see [Build and test what changed](/guides/build-and-test/).

```xml
<Project Sdk="Microsoft.Build.Traversal/4.1.82">
  <ItemGroup>
    <ProjectReference Include="/repo/src/DotnetAffected.Abstractions/DotnetAffected.Abstractions.csproj" />
    <ProjectReference Include="/repo/src/DotnetAffected.Core/DotnetAffected.Core.csproj" />
    <ProjectReference Include="/repo/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

The Traversal SDK version is fixed by the tool, so you don't need to reference it yourself.

## `text` → `affected.txt`

One absolute project path per line. Convenient for shell pipelines and for tools that take a file list.

```text
/repo/src/DotnetAffected.Abstractions/DotnetAffected.Abstractions.csproj
/repo/src/DotnetAffected.Core/DotnetAffected.Core.csproj
/repo/src/dotnet-affected/dotnet-affected.csproj
```

## `json` → `affected.json`

An indented JSON array with the project name (filename without extension) and its full path.

```json
[
  {
    "Name": "DotnetAffected.Abstractions",
    "FilePath": "/repo/src/DotnetAffected.Abstractions/DotnetAffected.Abstractions.csproj"
  },
  {
    "Name": "DotnetAffected.Core",
    "FilePath": "/repo/src/DotnetAffected.Core/DotnetAffected.Core.csproj"
  }
]
```

## `slnf` → `affected.slnf`

A [Solution Filter](https://learn.microsoft.com/visualstudio/ide/filtered-solutions) narrowing an existing solution
down to the affected projects. Unlike the other formats it references another file, so it needs
`--filter-file-path` pointing at a `.sln`, `.slnx` or `.slnf`:

```bash
dotnet affected --filter-file-path Affected.sln --format slnf
```

```json
{
  "solution": {
    "path": "Affected.sln",
    "projects": [
      "src\\DotnetAffected.Abstractions\\DotnetAffected.Abstractions.csproj",
      "src\\dotnet-affected\\dotnet-affected.csproj",
      "test\\dotnet-affected.Tests\\dotnet-affected.Tests.csproj"
    ]
  }
}
```

Following the `.slnf` convention, `solution.path` is relative to the filter file being written and the project paths
are relative to the solution, with backslash separators. That keeps the file usable from any `--output-dir`, and it
opens in Visual Studio like any other solution filter.

When the input is itself a `.slnf`, the output references the *solution* behind it, not the input filter — filters
never chain.

Asking for `slnf` without a solution to reference fails while arguments are parsed, before any project is evaluated:

```text
The slnf format needs a Solution to reference. Point --filter-file-path at a Solution (.sln, .slnx) or a Solution Filter (.slnf).
```

## Several formats at once

`--format` takes a space-separated list, and one file per format is written:

```bash
dotnet affected --format text traversal json
```

```text
WRITE: /repo/affected.txt
WRITE: /repo/affected.proj
WRITE: /repo/affected.json
```

Every formatter receives the path of the file it is writing and the filter file projects were discovered from, so
formats that emit paths — `slnf` today — stay correct wherever the output goes.

## Naming and location

```bash
dotnet affected --output-dir ./artifacts --output-name build-set --format text json
```

writes `./artifacts/build-set.txt` and `./artifacts/build-set.json`. A relative `--output-dir` is resolved against
`--repository-path`.

:::note
Every format contains both the changed and the affected projects — the two lists that `--verbose` and `describe` print
separately. There is no format that emits only one of them.
:::

## Seeing the output without writing it

`--dry-run` prints each file's path and contents to stdout and writes nothing:

```bash
dotnet affected --dry-run --format json
```
