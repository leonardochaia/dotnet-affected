---
title: Exploring with assumed changes
description: Use --assume-changes to see what would be affected, without changing anything.
sidebar:
  order: 6
---

`--assume-changes` replaces the Git diff entirely: the projects you name are treated as changed, and the graph is
walked from there. Nothing on disk is touched, and no Git comparison happens.

```bash
dotnet affected describe --assume-changes DotnetAffected.Core
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
```

That answers "if I touch this, what has to be rebuilt?" before you touch it.

## How projects are matched

Each value is resolved against the project graph and matches when **any** of these holds:

- it equals the project's full path — absolute, or relative to `--repository-path`;
- it equals the project's `ProjectName` property, case-insensitively;
- it equals the project file's name without extension, case-insensitively.

```bash
# By name
dotnet affected --assume-changes DotnetAffected.Core

# By path relative to the repository
dotnet affected --assume-changes src/DotnetAffected.Core/DotnetAffected.Core.csproj

# Several at once
dotnet affected --assume-changes DotnetAffected.Core dotnet-affected
```

:::note
A value that matches no project is an error, not an empty result — otherwise a typo would silently look like "nothing
is affected". The tool fails with a message naming the assumption that could not be resolved.
:::

## Combining with other options

`--assume-changes` overrides change detection, so `--from` and `--uncommitted` have no effect alongside it. Everything else
still applies: `--exclude-output` filters the result, `--format` and `--output-name` control what gets written, and
`--dry-run` keeps it all on stdout.

```bash
dotnet affected --dry-run --assume-changes DotnetAffected.Core --exclude-output '\.Benchmarks\.csproj$'
```

:::caution
Because the named projects are reported as changed, excluding them also removes everything reachable through them.
See [Excluding projects](/guides/excluding-projects/).
:::
