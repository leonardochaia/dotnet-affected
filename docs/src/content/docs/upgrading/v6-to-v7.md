---
title: Upgrading from v6 to v7
description: What changed between dotnet-affected v6 and v7, and what you need to update.
sidebar:
  order: 1
---

Most v6 setups keep working unchanged on v7: every removed option still has a working alias, and the output formats
you already use are unchanged. What does change is what gets **discovered** — read the first section even if you
change nothing else.

## Why upgrade

v7 is faster on the same repositories, and the difference grows with size — 6.5× faster at 2,000
projects, where v6 takes over twelve minutes and v7 takes under two.

| Projects | v6.2.0 | v7 | Speedup |
|---|---|---|---|
| 250 | 7.80s | 5.90s | 1.3× |
| 500 | 17.1s | 10.9s | 1.6× |
| 1,000 | 79.9s | 28.1s | 2.8× |
| 2,000 | 727s | 111s | 6.5× |

Desktop numbers, comparable to each other and nothing else. Both versions reported the same affected
projects at every size. [Full method and charts](/performance/).

## Behaviour changes

### Every comparison ends at the working tree

This is the change most likely to need an edit to your scripts.

In v6, `--from` and `--to` named two revisions. But the project graph was *always* built from the working tree, so a
`--to` naming anything else compared the files that changed up to one revision against the project structure of
another. A project added between the two ends was counted among the changed files and then reported under no project
at all — silently, with exit code `0`.

v7 makes the working tree the fixed end of every comparison:

- `--from` names the baseline, and defaults to `HEAD`.
- `--to` is accepted only when it names the commit already checked out, which makes it a no-op. Anything else is
  refused with an error. It warns on every use and will be **removed in v8**.
- `--uncommitted <all|staged|none>` chooses what the working tree contributes on top of the commits since `--from`.

```bash
# v6
dotnet affected --from origin/main --to "$CURRENT_COMMIT_HASH"

# v7 — CI already has that commit checked out
dotnet affected --from origin/main --uncommitted none
```

To compare two arbitrary revisions, check the later one out first:

```bash
git checkout releases/v2.0.0
dotnet affected --from releases/v1.0.0 --uncommitted none
```

**`--from` now includes uncommitted changes.** In v6, `--from X --to Y` compared two commits and ignored the working
tree entirely; in v7 the same `--from X` also picks up whatever is staged, unstaged or untracked. Pass
`--uncommitted none` for the old behaviour — and prefer it in CI, so a step that writes to a tracked file before
dotnet-affected runs cannot change the answer.

With the MSBuild SDK the same applies: `DotnetAffectedFromRef` names the baseline, `DotnetAffectedUncommitted` takes
`All`, `Staged` or `None`, and `DotnetAffectedToRef` warns and is going away.

See [Choosing what to compare](/guides/commit-ranges/).

### Changed project files nothing owns are reported

A changed `.csproj` that is not in the graph now produces a warning naming why it is missing — `--exclude-discovery`
matched it, the filter file does not reference it, or git ignores the path it is under. v6 counted such a file among
the changes and reported nothing for it, which was indistinguishable from a correct empty result.

Expect warnings on runs that were previously quiet. They point at a real gap between what changed and what was
analysed.

### NuGet versions are read from the right revisions

Package comparison used `--from` as the *new* side and `HEAD` as the old, inverting the reported versions and missing
uncommitted version bumps entirely. v7 reads `Directory.Packages.props` from the same two revisions the file diff
uses.

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

## If you use the GitHub Action

The action major now tracks the dotnet-affected major it drives, so upgrading the tool means upgrading the action:

```yaml
# v6
- uses: leonardochaia/dotnet-affected-action@v1

# v7
- uses: leonardochaia/dotnet-affected-action@v7
```

`@v1` targets 6.x, is deprecated, and logs a warning on every run — `v1.5` is its last release. It now installs the
latest 6.x rather than whatever is newest on NuGet, which is what kept it from silently picking up this major.

:::caution
Pinned to `@v1.4` or earlier? Those tags install an unpinned dotnet-affected and will pull in 7.x as soon as it is
published — an action written against v6 driving a v7 CLI. Move to `@v7`, or hold the tool back explicitly:

```yaml
- uses: leonardochaia/dotnet-affected-action@v1.4
  with:
    toolVersion: '6.*'
```
:::

There is no `@v6`: the action targeted 6.x throughout `@v1`'s life, and jumps to `@v7` so the two numbers line up from
here on.

`@v7` keeps every v6-era input working, so the version bump is the only required edit. Three of them are deprecated
and warn:

| Input           | On `@v7`                                                        |
|-----------------|------------------------------------------------------------------|
| `to`            | Ignored entirely — the comparison already ends at the checked-out commit |
| `solution-path` | Sends `--filter-file-path`; use that input instead                |
| `exclude`       | Sends `--exclude-output`; use that input instead                  |

`@v7` also gains inputs for the options v6 could not reach: `uncommitted`, `filter-file-path`, `exclude-discovery`,
`no-gitignore` and `repository-path`, and `slnf` becomes a usable `output-format`.

:::note
With `from` set, the action defaults `uncommitted` to `none`, which is what preserves your old `from` + `to`
semantics: the comparison stays between commits, and steps running before it cannot change the answer.
:::

See [GitHub Action](/github-action/).

## Library API changes

Only relevant if you reference `DotnetAffected.Core` or `DotnetAffected.Abstractions` directly.

| Change                                                                 | Impact                                                            |
|------------------------------------------------------------------------|--------------------------------------------------------------------|
| `IChangesProvider.GetChangedFiles` takes an `UncommittedChanges` instead of a `to` ref | **Breaking for implementers**                     |
| `AssumptionChangesProvider` removed                                    | **Breaking.** Pass `AffectedOptions.AssumeChanges` instead — assumptions are resolved against the graph now |
| `IChangesProvider` gained `ReadFilesAt`                                | **Breaking for implementers.** Needed to restore deleted files      |
| `AffectedSummary` gained `Diagnostics`                                 | Additive. Carries warnings so the CLI and the MSBuild task can report them their own way |
| `ToRefNotAtHeadException` added                                        | Thrown when `--to` names anything but the checked-out commit        |
| `IDiscoveryOptions` gained `ExcludeDiscoveryRegex` and `HonourGitIgnore` | **Breaking for implementers**                                       |
| `IOutputFormatter.Format` takes an `OutputFormatterContext`            | **Breaking for implementers.** Carries the output path and filter file |
| `AffectedOptions` constructor gained optional parameters               | Source-compatible; recompile required                              |
| `AffectedSummary` gained `ProjectsExcludedFromDiscovery`               | Additive; the constructor parameter is optional                    |
| `AssumedProjectNotFoundException` added                                | Thrown when `--assume-changes` matches nothing                     |
| `SolutionFilter` and `TraversalProject` added                          | Public helpers for reading/writing `.slnf` and authoring Traversal projects |

## Upgrade checklist

1. Bump the tool: `dotnet tool update dotnet-affected`, or the SDK version in `global.json` / your `.props` file. On
   GitHub Actions, move the action from `@v1` to `@v7`.
2. **Drop `--to`.** Make sure the revision it named is the one checked out, and add `--uncommitted none` to keep the
   comparison commit-only. Same for `DotnetAffectedToRef` in `.props` files.
3. Run `dotnet affected describe --verbose` and compare it with `--no-gitignore`. Investigate any project that
   disappears.
4. Replace `-e`/`--exclude` with `--exclude-output` in scripts and pipelines.
5. Check that every `--assume-changes` value still resolves — they now fail rather than pass silently.
6. Read any new warnings about changed project files belonging to no project: they were always happening, v6 just did
   not say so.
7. Expect more changed projects on ranges containing deletions.
