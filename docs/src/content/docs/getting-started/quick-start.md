---
title: Quick start
description: Run dotnet-affected for the first time and understand its output.
sidebar:
  order: 2
---

Run the tool from the root of your repository. With no arguments, it compares your working directory against the
current `HEAD`.

```bash
dotnet affected --verbose
```

```text
1 files have changed referenced by 1 projects
0 NuGet Packages have changed
5 projects are affected by these changes
0 projects were excluded
Changed Projects
Name                 Path
DotnetAffected.Core  /repo/src/DotnetAffected.Core/DotnetAffected.Core.csproj

Affected Projects
Name                        Path
dotnet-affected             /repo/src/dotnet-affected/dotnet-affected.csproj
dotnet-affected.Benchmarks  /repo/benchmarks/dotnet-affected.Benchmarks/dotnet-affected.Benchmarks.csproj
dotnet-affected.Tests       /repo/test/dotnet-affected.Tests/dotnet-affected.Tests.csproj
DotnetAffected.Core.Tests   /repo/test/DotnetAffected.Core.Tests/DotnetAffected.Core.Tests.csproj
DotnetAffected.Tasks        /repo/src/DotnetAffected.Tasks/DotnetAffected.Tasks.csproj
WRITE: /repo/affected.proj
```

## Reading the output

**Changed projects** are the projects that own at least one changed file. **Affected projects** are the projects that
depend — directly or transitively — on something that changed. The two lists are disjoint: a project appears under
*changed* or under *affected*, never both.

The generated `affected.proj` contains both lists, because everything that changed and everything affected by those
changes needs to be built:

```xml
<Project Sdk="Microsoft.Build.Traversal/4.1.82">
  <ItemGroup>
    <ProjectReference Include="/repo/benchmarks/dotnet-affected.Benchmarks/dotnet-affected.Benchmarks.csproj" />
    <ProjectReference Include="/repo/src/dotnet-affected/dotnet-affected.csproj" />
    <ProjectReference Include="/repo/src/DotnetAffected.Core/DotnetAffected.Core.csproj" />
    <ProjectReference Include="/repo/src/DotnetAffected.Tasks/DotnetAffected.Tasks.csproj" />
    <ProjectReference Include="/repo/test/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
    <ProjectReference Include="/repo/test/DotnetAffected.Core.Tests/DotnetAffected.Core.Tests.csproj" />
  </ItemGroup>
</Project>
```

You can then treat it like any other project file:

```bash
dotnet test affected.proj
```

See [Build and test what changed](/guides/build-and-test/) for the full workflow.

## Look before you write

`--dry-run` prints what would be written instead of creating files, and `describe` prints the changed and affected
projects without producing any output file at all:

```bash
dotnet affected --dry-run
dotnet affected describe
```

`--assume-changes` pretends a project changed, which is a quick way to explore the blast radius of a change before
making it:

```bash
dotnet affected describe --assume-changes DotnetAffected.Core
```

## When nothing changed

When no project changed and nothing is affected, the tool writes no output and exits with code `166` instead of `0`.
Use that to skip build and test steps entirely — see [Exit codes](/reference/exit-codes/).
