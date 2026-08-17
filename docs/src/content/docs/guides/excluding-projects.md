---
title: Excluding projects
description: The two ways to keep projects out — from the output, or from discovery entirely.
sidebar:
  order: 5
---

There are two exclusion options, and they differ in kind rather than degree.

| Option                | The project is…                                            | Its dependents…                             |
|-----------------------|-------------------------------------------------------------|---------------------------------------------|
| `--exclude-output`    | discovered and evaluated, but left out of the results       | are still found through it                  |
| `--exclude-discovery` | dropped before the graph is built, so it is not a starting point | are unaffected — unless they reference it |

Both take a .NET [regular expression](https://learn.microsoft.com/dotnet/standard/base-types/regular-expressions)
matched against each project's full path.

## `--exclude-output`

Use this to keep something out of the build set while leaving the analysis intact: benchmarks, samples, a project your
pipeline handles separately.

```bash
dotnet affected --dry-run --verbose --exclude-output '\.Benchmarks\.csproj$'
```

```text
1 files have changed referenced by 1 projects
0 NuGet Packages have changed
7 projects are affected by these changes
1 projects were excluded
Changed Projects
Name                         Path
DotnetAffected.Abstractions  /repo/src/DotnetAffected.Abstractions/DotnetAffected.Abstractions.csproj

Affected Projects
Name                       Path
dotnet-affected            /repo/src/dotnet-affected/dotnet-affected.csproj
dotnet-affected.Tests      /repo/test/dotnet-affected.Tests/dotnet-affected.Tests.csproj
...

Excluded Projects
Name                        Path
dotnet-affected.Benchmarks  /repo/benchmarks/dotnet-affected.Benchmarks/dotnet-affected.Benchmarks.csproj
```

Because the project is still evaluated, changes flow *through* it: excluding a mid-chain library keeps everything
depending on that library in the results.

:::caution
One edge case does not follow that rule. Exclusion is applied to the changed projects before the graph is walked, so
when the pattern matches a project that **itself changed**, its dependents are never discovered. Excluding a changed
library in a repository where nothing else changed reports `No affected projects where found for the current changes`.
:::

## `--exclude-discovery`

Use this when a project cannot be loaded at all — a project MSBuild fails to evaluate would otherwise take the whole
run down while the graph is being built.

```bash
dotnet affected --verbose --exclude-discovery '/legacy/'
```

```text
1 files have changed referenced by 1 projects
0 NuGet Packages have changed
7 projects are affected by these changes
0 projects were excluded
1 projects were excluded from discovery
...

Projects Excluded from Discovery
Path
/repo/benchmarks/dotnet-affected.Benchmarks/dotnet-affected.Benchmarks.csproj
```

The two lists are reported separately, and the discovery line only appears when something was actually excluded that
way. Excluded-from-discovery projects are listed by path alone: nothing evaluated them at discovery time, so there is
no project name to show.

:::caution[It removes starting points, not projects]
Exclusion happens to the *discovered* set — the projects the graph is built from. MSBuild then follows
`ProjectReference` items from those entry points, so a project excluded from discovery that another discovered project
references is pulled back into the graph, and can still show up as affected.

It reliably keeps out projects nothing references — the usual case for a broken or legacy project sitting on its own.
To keep a referenced project out of the results, use `--exclude-output` as well.
:::

## Writing the pattern

- The pattern is matched with `Regex.IsMatch` against the project's absolute path, so it matches **anywhere** in the
  path — it is not anchored, and it is not a glob.
- `.` is a regex wildcard. `.Tests.` matches any path containing `Tests` with any character on either side; to match a
  literal dot, escape it: `'\.Tests\.'`.
- Matching is case-sensitive unless you say otherwise: `'(?i)tests'`.

```bash
# Everything under a directory
dotnet affected --exclude-output '/samples/'

# Test projects by file name
dotnet affected --exclude-output '\.Tests\.csproj$'

# Two things at once
dotnet affected --exclude-output '(/samples/|\.Benchmarks\.csproj$)'
```

## `--exclude` is obsolete

`-e`/`--exclude` still works and behaves exactly like `--exclude-output`, which it is now an alias for. When both are
given, `--exclude-output` wins.

## Exclusion and the "nothing changed" exit code

Exclusion happens before the result is judged empty, so excluding everything that changed produces exit code `166` —
the same as if nothing had changed at all. See [Exit codes](/reference/exit-codes/).
