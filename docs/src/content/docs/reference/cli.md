---
title: CLI reference
description: Every command and option supported by the dotnet-affected CLI.
sidebar:
  order: 1
---

```text
$ dotnet affected --help
Description:
  Determines which projects are affected by a set of changes.

Usage:
  dotnet-affected [command] [options]

Options:
  -p, --repository-path <repository-path>  Path to the root of the repository, where the .git directory is.
                                           [Defaults to current directory, or solution's directory when using --solution-path]
  --solution-path <solution-path>          [OBSOLETE: use --filter-file-path] Path to a Solution file (.sln) used to discover projects that may be affected.
                                           When omitted, will search for project files inside --repository-path.
  --filter-file-path <filter-file-path>    Path to a filter file (.sln, .slnx, .slnf) used to discover projects that may be affected.
                                           When omitted, will search for project files inside --repository-path.
  --no-gitignore                           Discover projects inside paths that git ignores, such as build output or nested clones.
                                           [Only applies when searching --repository-path, not when using --filter-file-path] [default: False]
  -v, --verbose                            Write useful messages or just the desired output. [default: False]
  --assume-changes <assume-changes>        Hypothetically assume that given projects have changed instead of using Git diff to determine them.
  --from <from>                            A branch or commit to compare against --to.
  --to <to>                                A branch or commit to compare against --from.
  --exclude-output <exclude-output>        A dotnet Regular Expression matched against each project's full path.
                                           Matching projects are still evaluated, and still carry changes through to
                                           the projects depending on them, but are kept out of the output.
  --exclude-discovery <exclude-discovery>  A dotnet Regular Expression matched against each project's full path.
                                           Matching projects are never loaded, so one that MSBuild cannot evaluate
                                           stops failing the run. Nothing can depend on them either.
  -e, --exclude <exclude>                  [OBSOLETE: use --exclude-output] A dotnet Regular Expression used to
                                           exclude projects from the output.
  -f, --format <format>                    Space-seperated output file formats. Possible values: <traversal, text, json, slnf>. [default: traversal]
  --dry-run                                Only output to stdout. No output files will be created. [default: False]
  --output-dir <output-dir>                The directory where the output file(s) will be generated.
                                           Relative paths will be based on --repository-path.
  --output-name <output-name>              The filename to create.
                                           Format file extensions will be appended. [default: affected]
  --version                                Show version information
  -?, -h, --help                           Show help and usage information

Commands:
  describe  Prints the current changed and affected projects.
```

## Commands

### Root command

Determines the changed and affected projects and writes them in the requested [formats](#-f---format).

### `describe`

Prints the changed and affected projects to stdout in a readable table, without writing any file.

```bash
dotnet affected describe
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
dotnet-affected.Tests       /repo/test/dotnet-affected.Tests/dotnet-affected.Tests.csproj
```

All the options below except the output ones (`--format`, `--dry-run`, `--output-dir`, `--output-name`) apply to
`describe` as well.

## Options

### `-p`, `--repository-path`

Path to the root of the repository — the directory containing `.git`. Git operations and relative output paths are
resolved from here.

Defaults to the current directory, or to the filter file's directory when `--filter-file-path` (or the obsolete
`--solution-path`) is used.

```bash
dotnet affected --repository-path /repo
```

:::note
If your solution lives in a subdirectory of the repository, you still need `--repository-path` to point at the Git
root:

```bash
dotnet affected --repository-path /repo --filter-file-path /repo/my-big-project/MyBigProject.sln
```
:::

### `--filter-file-path`

Path to a filter file used to discover the projects that may be affected. When omitted, projects are discovered by
recursively searching `--repository-path`.

| Extension | Discovered projects                                   |
|-----------|--------------------------------------------------------|
| `.sln`    | The solution's projects                                |
| `.slnx`   | The solution's projects (XML solution format)          |
| `.slnf`   | The projects a solution filter includes                |
| `.proj`   | Every `ProjectReference` item in the MSBuild project   |

Only projects included in the filter file are considered. Changes to projects outside it are ignored, and the tool
reports that nothing is affected.

```bash
dotnet affected --filter-file-path ./MySolution.slnx
```

See [Project discovery](/guides/project-discovery/) for the details.

### `--solution-path`

**Obsolete — use [`--filter-file-path`](#--filter-file-path).** Behaves the same but accepts only `.sln` files.

### `--no-gitignore`

Search every directory under `--repository-path`, including paths git ignores. Defaults to `False`: ignored paths are
skipped, because they are not repository content.

```bash
dotnet affected --no-gitignore
```

Only applies to filesystem discovery. A filter file lists its projects explicitly, ignored or not. See
[Project discovery](/guides/project-discovery/#ignored-paths).

### `-v`, `--verbose`

Write progress messages — project discovery, graph construction, change counts, and the tables of changed and affected
projects — instead of only the desired output. Defaults to `False`.

### `--assume-changes`

Pretend the given projects changed, instead of using `git diff` to determine them. Accepts one or more values, each
matched against the project's full path, its `ProjectName`, or its file name without extension. A value that matches
no project is an error.

```bash
dotnet affected --dry-run --assume-changes DotnetAffected.Core
```

Useful for answering "what would break if I touched this?" without touching it — see
[Exploring with assumed changes](/guides/assume-changes/).

### `--from`

A branch, tag or commit to compare against `--to`. When `--to` is omitted, the comparison runs against `HEAD`.

### `--to`

A branch, tag or commit to compare against `--from`. Requires `--from`; using it alone fails with
`--from is required when using --to`.

See [Comparing commit ranges](/guides/commit-ranges/).

### `--exclude-output`

A .NET [regular expression](https://learn.microsoft.com/dotnet/standard/base-types/regular-expressions) matched against
each project's full path. Matching projects are still discovered and evaluated — so changes flow through them to their
dependents — but they are kept out of the results and listed under *Excluded Projects* when running with `--verbose`.

```bash
dotnet affected --dry-run --verbose --exclude-output '\.Tests\.csproj$'
```

### `--exclude-discovery`

Same kind of pattern, applied earlier: matching projects are never loaded at all. Use it for projects MSBuild cannot
evaluate, which would otherwise take the whole run down. Nothing can depend on them, so they never appear as affected
either. They are listed under *Projects Excluded from Discovery*.

```bash
dotnet affected --verbose --exclude-discovery '/legacy/'
```

The two are different in kind, not degree — see [Excluding projects](/guides/excluding-projects/) for which to reach
for.

### `-e`, `--exclude`

**Obsolete — use [`--exclude-output`](#--exclude-output)**, which it is an alias for. If both are given,
`--exclude-output` wins.

### `-f`, `--format`

Space-separated list of output formats. Defaults to `traversal`.

| Value       | Output file      | Contents                                                                   |
|-------------|------------------|----------------------------------------------------------------------------|
| `traversal` | `affected.proj`  | An MSBuild Traversal SDK project referencing every changed/affected project |
| `text`      | `affected.txt`   | One project path per line                                                   |
| `json`      | `affected.json`  | Project names and paths as JSON                                             |
| `slnf`      | `affected.slnf`  | A Solution Filter narrowing a solution to those projects                    |

`slnf` is the only format that references another file, so it requires `--filter-file-path` (or the obsolete
`--solution-path`) to point at a `.sln`, `.slnx` or `.slnf`. This is checked while parsing arguments — before any
project is evaluated:

```text
The slnf format needs a Solution to reference. Point --filter-file-path at a Solution (.sln, .slnx) or a Solution Filter (.slnf).
```

```bash
# One format
dotnet affected --format text

# Several at once — one file per format
dotnet affected --format text traversal json
```

See [Output formats](/guides/output-formats/) for what each file contains.

### `--dry-run`

Write what would be generated to stdout instead of creating files. Defaults to `False`.

```text
$ dotnet affected --dry-run
DRY-RUN: WRITE /repo/affected.proj
DRY-RUN: CONTENTS:
<Project Sdk="Microsoft.Build.Traversal/4.1.82">
  <ItemGroup>
    <ProjectReference Include="/repo/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

### `--output-dir`

The directory where output files are generated. Relative paths are resolved against `--repository-path`.

### `--output-name`

The filename to create, without extension — each format appends its own. Defaults to `affected`, producing
`affected.proj`, `affected.txt` and `affected.json`.

### `--version`

Print the version of the tool.
