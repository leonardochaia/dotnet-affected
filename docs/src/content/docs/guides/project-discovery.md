---
title: Project discovery
description: How dotnet-affected finds the projects it considers, and how to narrow that set with a filter file.
sidebar:
  order: 3
---

Before anything can be compared, dotnet-affected has to know which projects exist. There are two discovery modes.

## Filesystem discovery (default)

`--repository-path` (or the current directory) is searched recursively for every `.csproj`, `.fsproj` and `.vbproj`.
Nothing else is considered.

```bash
dotnet affected --repository-path /repo
```

This suits repositories with no solution files at all, for example when solutions are generated on demand with
[SlnGen](https://microsoft.github.io/slngen/).

### Ignored paths

The search honours your `.gitignore`. Paths git ignores are not repository content, so build output, tooling scratch
directories, git worktrees and nested clones are skipped — along with any copies of your projects living inside them.
Ignored directories are pruned rather than walked and filtered, which is also where most of the time saving comes
from: that is where the bulk of a repository's files usually are.

Two exceptions keep the result honest:

- **Tracked files are always discovered.** A project git tracks is repository content no matter which patterns match
  it, so `git add -f build/Tool.csproj` keeps it visible. The walk stops at the ignored directory, and the index puts
  the project back.
- **The `.git` directory is never searched.** Git does not ignore its own directory, it just never looks at it. It
  holds no projects and, in a packed repository, most of the files under the root.

To search everything, as versions before v7 did:

```bash
dotnet affected --no-gitignore
```

With the MSBuild SDK, set `DotnetAffectedHonourGitIgnore` to `false`.

:::note
This applies only to filesystem discovery. A filter file lists its projects explicitly, and those are used whether git
ignores them or not.

Discovery also falls back to searching everything when the repository path is not a git repository at all — nothing to
read patterns from.
:::

## Filter files

`--filter-file-path` restricts discovery to the projects listed in a file. Three kinds are supported, chosen by
extension:

| Extension        | Discovered projects                                              |
|------------------|-------------------------------------------------------------------|
| `.sln`           | The solution's projects                                           |
| `.slnx`          | The solution's projects (XML solution format)                     |
| `.slnf`          | The projects a Solution Filter includes                           |
| `.proj`          | Every `ProjectReference` item in the MSBuild project              |

```bash
dotnet affected --filter-file-path ./MySolution.slnx
```

Relative paths inside the file are resolved against the file's own directory.

:::note
The `.proj` form accepts any MSBuild project with `ProjectReference` items — including a Traversal project, and
including an `affected.proj` produced by a previous run.

A `.slnf` input pairs naturally with [`--format slnf`](/guides/output-formats/): narrow a solution once, then keep
narrowing it to what each change affects.
:::

### What filtering means

Only the discovered projects participate. If a change lands in a project that the filter file does not include, it is
ignored, and dotnet-affected reports that nothing is affected.

### Repository path and filter files

The Git root and the filter file are separate concerns. When your solution is not at the root of the repository, pass
both:

```bash
dotnet affected \
    --repository-path /repo \
    --filter-file-path /repo/my-big-project/MyBigProject.sln
```

Without `--repository-path`, the repository path defaults to the filter file's directory, which is not a Git
repository root in this layout and will fail.

## Keeping projects out of discovery

`--exclude-discovery` drops matching projects from the discovered set before the graph is built, which is the only
point early enough to help when a project cannot be evaluated at all. See
[Excluding projects](/guides/excluding-projects/).

## `--solution-path` is obsolete

`--solution-path` predates `--filter-file-path` and accepts only `.sln`. It still works, and the CLI marks it
`[OBSOLETE]`. Use `--filter-file-path` instead.

## What counts as a project's file

A changed file is attributed to a project when the project references it — which is broader than "files in the project
directory". Compiled sources, content, embedded resources and any other item the MSBuild evaluation pulls in all count,
as do imported files like `Directory.Build.props` and `Directory.Build.targets`, which are attributed to every project
that imports them.

That last point has a practical consequence: touching a root `Directory.Build.props` marks every project under it as
changed.
