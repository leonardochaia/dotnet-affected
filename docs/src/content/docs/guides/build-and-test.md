---
title: Build and test what changed
description: Use the generated Traversal project to build and test only the affected projects.
sidebar:
  order: 1
---

The default output is an [MSBuild Traversal SDK](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal)
project. It behaves like a solution for the `dotnet` CLI, so you can point any command at it.

## Generate the project

```bash
dotnet affected --verbose
```

```text
1 files have changed referenced by 1 projects
0 NuGet Packages have changed
5 projects are affected by these changes
0 projects were excluded
...
WRITE: /repo/affected.proj
```

The file is written to the repository path as `affected.proj`. Both the name and the directory are configurable with
[`--output-name`](/reference/cli/#--output-name) and [`--output-dir`](/reference/cli/#--output-dir).

## Build and test it

```bash
dotnet build affected.proj
dotnet test affected.proj
```

Restore works the same way:

```bash
dotnet restore affected.proj
```

## Skip the work when nothing changed

`dotnet affected` exits with `166` when nothing changed and nothing is affected, and no output file is written. Guard
the build so it does not fail on a missing `affected.proj`:

```bash
dotnet affected
if [ "$?" -eq 0 ]; then
    dotnet build affected.proj
fi
```

See [Exit codes](/reference/exit-codes/) for the full contract.

## Preview without writing files

`--dry-run` prints the file that would be written, contents included:

```bash
dotnet affected --dry-run
```

```text
DRY-RUN: WRITE /repo/affected.proj
DRY-RUN: CONTENTS:
<Project Sdk="Microsoft.Build.Traversal/4.1.82">
  <ItemGroup>
    <ProjectReference Include="/repo/src/dotnet-affected/dotnet-affected.csproj" />
    <ProjectReference Include="/repo/test/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
  </ItemGroup>
</Project>
```

:::note
Paths in the generated project are absolute, so the file is not portable between machines. Generate it on the machine
that will build it.
:::
