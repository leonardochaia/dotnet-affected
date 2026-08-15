---
title: Upgrading from v6 to v7
description: What changed between dotnet-affected v6 and v7, and what you need to update.
sidebar:
  order: 1
---

Most v6 setups keep working unchanged on v7: every removed option still has a working alias, and the output formats
you already use are unchanged. What does change is what gets **discovered** — read the first section even if you
change nothing else.

## Behaviour changes

### Discovery honours `.gitignore`

Filesystem discovery now skips paths git ignores, where v6 searched every directory under `--repository-path`.
Projects inside build output, tooling scratch directories, git worktrees and nested clones are no longer discovered.

For most repositories this only removes copies you never wanted — and makes runs faster, since ignored directories are
pruned instead of walked. It changes results when you *rely* on a project inside an ignored path, for example a
generated project written into an ignored directory.

Two ways to keep such a project:

```bash
# Search everything, as v6 did
dotnet affected --no-gitignore
```

```bash
# Or make the project repository content — it is then always discovered
git add -f build/Generated.csproj
```

With the MSBuild SDK, set `DotnetAffectedHonourGitIgnore` to `false`:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <DotnetAffectedHonourGitIgnore>false</DotnetAffectedHonourGitIgnore>
    </PropertyGroup>
</Project>
```

:::tip
Compare before and after on a real repository before rolling it out:
`dotnet affected describe --verbose` against `dotnet affected describe --verbose --no-gitignore`. If the two agree,
nothing about this change affects you.
:::

### `--assume-changes` fails on unknown projects

v6 silently ignored a value matching no project, which looked exactly like "nothing is affected". v7 fails:

```text
Couldn't find a project matching 'IgnoredProbe'. Assumed changes accept a project name, a project file name, or a path to a project file.
```

It also matches more than v6 did: a project's full path or its file name, not just its `ProjectName`.

Check CI scripts that pass project names for typos — a job that used to run and find nothing will now fail loudly.
Note this interacts with the `.gitignore` change above: a project that is no longer discovered can no longer be
assumed changed either.

### Deleted files are attributed to their projects

A file deleted in the compared range no longer disappears from the analysis. v6 evaluated projects against the current
file system, where the file no longer existed, so the project owning it was often not reported as changed. v7 restores
the deleted files from the `from` commit before evaluating.

Expect *more* projects to be reported as changed for ranges containing deletions. That is the correction, not a
regression.

## Renamed options

| v6            | v7                  | Notes                                                       |
|---------------|---------------------|--------------------------------------------------------------|
| `-e`, `--exclude` | `--exclude-output` | The old spelling still works and is an alias. If both are given, `--exclude-output` wins |

`--exclude-output` behaves as `--exclude` did: matching projects are still evaluated and still carry changes to their
dependents, they are just kept out of the results.

```bash
# v6
dotnet affected -e '\.Tests\.csproj$'

# v7
dotnet affected --exclude-output '\.Tests\.csproj$'
```

`--solution-path` remains obsolete in favour of `--filter-file-path`, as it already was in v6.

## New in v7

Nothing here is required, but these are what the release is for.

### `--exclude-discovery`

Keeps projects out of the graph entirely, rather than out of the results. This is the option to reach for when a
project cannot be evaluated at all and takes the whole run down with it — something no v6 option could express.

```bash
dotnet affected --exclude-discovery '/legacy/'
```

See [Excluding projects](/guides/excluding-projects/) for how the two differ.

### Solution filter output, and `.slnf` input

`--format slnf` writes a [Solution Filter](/guides/output-formats/) narrowing a solution down to the affected
projects, which opens in Visual Studio like any other filter. It needs a solution to reference:

```bash
dotnet affected --filter-file-path MySolution.sln --format slnf
```

`--filter-file-path` also accepts `.slnf` now, so a filter can be both input and output.

### `--no-gitignore`

Covered above: opts back into v6's discovery.

### Faster runs, nothing to configure

Change detection and affected detection were reworked, and pruning ignored directories removes the largest part of
discovery's work on a big repository. No option controls any of this — it applies to the same commands you already
run.

## Library API changes

Only relevant if you reference `DotnetAffected.Core` or `DotnetAffected.Abstractions` directly.

| Change                                                                 | Impact                                                            |
|------------------------------------------------------------------------|--------------------------------------------------------------------|
| `AssumptionChangesProvider` removed                                    | **Breaking.** Pass `AffectedOptions.AssumeChanges` instead — assumptions are resolved against the graph now |
| `IChangesProvider` gained `ReadFilesAt`                                | **Breaking for implementers.** Needed to restore deleted files      |
| `IDiscoveryOptions` gained `ExcludeDiscoveryRegex` and `HonourGitIgnore` | **Breaking for implementers**                                       |
| `IOutputFormatter.Format` takes an `OutputFormatterContext`            | **Breaking for implementers.** Carries the output path and filter file |
| `AffectedOptions` constructor gained optional parameters               | Source-compatible; recompile required                              |
| `AffectedSummary` gained `ProjectsExcludedFromDiscovery`               | Additive; the constructor parameter is optional                    |
| `AssumedProjectNotFoundException` added                                | Thrown when `--assume-changes` matches nothing                     |
| `SolutionFilter` and `TraversalProject` added                          | Public helpers for reading/writing `.slnf` and authoring Traversal projects |

## Upgrade checklist

1. Bump the tool: `dotnet tool update dotnet-affected`, or the SDK version in `global.json` / your `.props` file.
2. Run `dotnet affected describe --verbose` and compare it with `--no-gitignore`. Investigate any project that
   disappears.
3. Replace `-e`/`--exclude` with `--exclude-output` in scripts and pipelines.
4. Check that every `--assume-changes` value still resolves — they now fail rather than pass silently.
5. Expect more changed projects on ranges containing deletions.
