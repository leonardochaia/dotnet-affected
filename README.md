# dotnet-affected

A .NET Tool for determining which projects are affected by a set of changes. This tool is particularly useful for large
projects or mono-repositories.

## Features

dotnet-affected works by comparing two versions of your code, usually by a commit range in CI, or HEAD against your
current working directory.

1. Detects which MSBuild Projects have changed based on the files that changed.
2. When using Central Package Management, detects which NuGet Packages have changed.
3. Detects which projects are affected by projects or packages that have changed.
4. Outputs a [MSBuild Traversal SDK](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) Project that can
   be used to `dotnet build` and `test` which projects where changed/affected.
5. Outputs a text file which can be used to deploy only what's needed.

## How it works

dotnet-affected discovers all `*.csproj` and uses MSBuild to builds a `ProjectGraph` of all projects and which projects
they depend on.

Then `git diff` is ran, to determine which files have changed. These files are then mapped to which project they belong
to, and we get a list of which projects have any changes.

dotnet-affected will also detect changes to `Directory.Packages.props` and determine which NuGet packages have been
added/deleted/updated.

With the changed projects, and changed NuGet Packages, it uses the `ProjectGraph` to find which projects are affected by
those changes.

For example, given this project structure:

1. Inventory.Shared
2. Inventory (depends on .1)
3. Inventory.Tests (depends on .2)
4. Inventory.Shared.Tests (depends on .1)

When .2 changes, .3 will be affected so we will build and test .2 and .3. There's no need to build and test .4 since .1
has not changed.

When 1. changes, everything needs to be built/test, since, transitively, they all depend on .1.

## Caveats

1. Detects `.csproj` only. Supporting other SDK projects will be
   implemented. [#16](https://github.com/leonardochaia/dotnet-affected/issues/16)
2. SDK projects only. Supporting non-SDK projects will be
   implemented. [#15](https://github.com/leonardochaia/dotnet-affected/issues/15)
3. `.props` files and other "global" files that may affect projects are not being detected.

## Installation

The tool can be installed using `dotnet install`:

```shell
dotnet tool install dotnet-affected
```

It can also be installed globally with the `--global` flag but installing using local tools is recommended so all devs
share the same version, and so you share the same version in CI as well.

You can then run the tool using `dotnet affected` in the root of your repository.

```text
$ dotnet affected --help
Description:
  Determines which projects are affected by a set of changes.

Usage:
  affected [command] [options]

Options:
  -p, --repository-path <repository-path>  Path to the root of the repository, where the .git directory is.
                                           [Defaults to current directory, or solution's directory when using --solution-path]
  --solution-path <solution-path>          Path to a Solution file (.sln) used to discover projects that may be affected.
                                           When omitted, will search for project files inside --repository-path.
  -v, --verbose                            Write useful messages or just the desired output. [default: False]
  --assume-changes <assume-changes>        Hypothetically assume that given projects have changed instead of using Git diff to determine them.
  --from <from>                            A branch or commit to compare against --to.
  --to <to>                                A branch or commit to compare against --from
  -f, --format <format>                    Space separated list of formatters to write the output. [default: traversal]
  --dry-run                                Doesn't create files, outputs to stdout instead. [default: False]
  --output-dir <output-dir>                The directory where the output file(s) will be generated
                                           If relative, it's relative to the --repository-path
  --output-name <output-name>              The name for the file to create for each format.
                                           Format extension is appended to this name. [default: affected]
  --version                                Show version information
  -?, -h, --help                           Show help and usage information

Commands:
  describe  Prints the current changed and affected projects.
```

## Locating Git Repository

dotnet-affected needs the path to your Git repository (where the `.git` folder is) so it can run `git diff`. By default,
dotnet-affected will attempt to interpret the current working directory as a git repository.

This can be customized using `--repository-path`, shorthand `-p`.

Locally, it's recommended to run the tool at the repository root to simplify things. In CI, you usually provide the
working directory that your CI provider gives you in an environment variable.

## Project Discovery

By default, projects are discovered by recursively searching the `--repository-path`, or current working directory if
not specified.

This is quite useful for projects that do not have Solution Files and are using something
like [SlnGen](https://microsoft.github.io/slngen/)
to generate solutions.

However, when you do have a Solution File, the `--solution-file` can be used to discover projects from the Solution
instead. This can also be used to filter down which projects the tool discovers, if you don't want to discover all
present in file system.

When using `--solution-file`, only the projects included in the Solution will be considered for changes.

For example, if changes are made to projects that are not referenced by the Solution file, those changes will be ignored
and dotnet-affected will output that nothing.

Note that, if your Solution file is not at the root of your Git Repository (where the `.git` directory is), you still
need to specify `--repository-path`. For example:

```shell
dotnet affected --repository-path /home/lchaia/monorepo --solution-path /home/lchaia/monorepo/my-big-project/MyBigProjectSolution.sln
```

## Build/test affected projects

In order to build only what is affected, the tool outputs an MSBuild Traversal project that can can then be feed
to `dotnet build`.

For example, the below command outputs `affected.proj` at the current directory, by comparing your changes against the
current HEAD.

```text
$ dotnet affected --verbose
Discovering projects from /home/lchaia/dev/dotnet-affected
Building Dependency Graph
Built Graph with 8 Projects in 0.31s
1 files have changed inside 1 projects
0 NuGet Packages have changed
1 projects are affected by these changes
Changed Projects
Name  Path
      /home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj

Affected Projects
Name                   Path
dotnet-affected.Tests  /home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj
WRITE: /home/lchaia/dev/dotnet-affected/affected.proj
```

The contents of `affected.proj` are:

```xml

<Project Sdk="Microsoft.Build.Traversal/3.0.3">
    <ItemGroup>
        <ProjectReference
            Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj"/>
        <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj"/>
    </ItemGroup>
</Project>
```

You can then use `dotnet test` (or any other `dotnet` commands) against the resulting `affected.proj` file:

```shell
dotnet test affected.proj
```

## Affected projects between commit ranges

By default, dotnet-affected will compare changes between your working directory against the current HEAD. This can be
changed by providing the `--from` and `--to` parameters. Commit sha or branch names can be used.

Examples:

```shell
# Compares HEAD against working directory
dotnet affected

# Compares HEAD against branch chore/target-net7
dotnet affected --from chore/target-net7

# Compares main against branch chore/target-net7
dotnet affected --from chore/target-net7 --to main
```

## Output Formatting

dotnet-affected currently supports outputting Traversal SDK project files and plain text list of changed and affected
projects. The `--format` option can be used to choose which format to output.

```text
$ dotnet affected -v --format text
Discovering projects from /home/lchaia/dev/dotnet-affected
Building Dependency Graph
Built Graph with 8 Projects in 0.31s
1 files have changed inside 1 projects
0 NuGet Packages have changed
1 projects are affected by these changes
Changed Projects
Name  Path
      /home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj

Affected Projects
Name                   Path
dotnet-affected.Tests  /home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj
WRITE: /home/lchaia/dev/dotnet-affected/affected.txt
```

```text
$ cat affected.txt                                                                                                                                         ✔
/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj
/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj
```

## Multiple Output Formats

Multiple formats can be generated at the same time by separating them with spaces. This is quite useful in CI to
build/test and also get the list of projects to deploy.

```text
$ dotnet affected -v --format text traversal
Discovering projects from /home/lchaia/dev/dotnet-affected
Building Dependency Graph
Built Graph with 8 Projects in 0.39s
1 files have changed inside 1 projects
0 NuGet Packages have changed
1 projects are affected by these changes
Changed Projects
Name  Path
      /home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj

Affected Projects
Name                   Path
dotnet-affected.Tests  /home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj
WRITE: /home/lchaia/dev/dotnet-affected/affected.txt
WRITE: /home/lchaia/dev/dotnet-affected/affected.proj
```

## Continuous Integration

For usage in CI, it's recommended to use the `--from` and `--to` options with the environment variables provided by your
build tool.

dotnet-affected can be used in any CI system where you `dotnet` is present. You can install the tool and run
`dotnet affected` commands as if locally.

However, an action is provided for [Github actions](https://github.com/leonardochaia/dotnet-affected-action)
and having a way to simplify this for other CI systems would be welcome.

### Building branches/tags

For example, for building a branch a setup like this could be used:

```shell
# Replace env vars with what your CI system gives you
dotnet affected \
    --from $LAST_SUCCESSFUL_BUILD_COMMIT \
    --to $CURRENT_COMMIT_HASH
dotnet test affected.proj
```

It's important to note that CI system triggers a build per push, not per commit. Which means a set of commits may be
built, instead of just one. There is also the case where the previous build/s have failed, so we need to build from the
latest commit that has a successful build.

There's an in-depth explanation of the problem in [here](https://github.com/nrwl/last-successful-commit-action#problem)

When using GitHub Actions, `leonardochaia/dotnet-affected@v1` can be used to execute dotnet-affected. This can be
combined with [nrwl/last-successful-commit-action](https://github.com/nrwl/last-successful-commit-action) to
build/test only what's affected since last succesful commit.

You can see a complete example for [building branches with GitHub actions here](https://github.com/leonardochaia/dotnet-affected-action#for-building-branches).

### Building Pull Requests

For building PRs, we need to provide the target branch/commit and the PR branch/commit.

```shell
dotnet affected generate --from origin/main --to $CURRENT_COMMIT_HASH
dotnet test affected.proj
```

You can see a complete example for [building PRs with GitHub actions here](https://github.com/leonardochaia/dotnet-affected-action#for-building-prs).

## Don't build/test/deploy when no projects have changed

If nothing has changed, or the changes are not related to any of the discovered projects, there is no need to
run `dotnet test`.

In order to detect this, dotnet affected will exit with an exit status code `166`. You can use this to prevent spending
time on unnecessary tasks when nothing has changed.

Note that dotnet affected returns `166` when nothing has changed, not to be confused when nothing is affected. If
projects have changed, but nothing is affected by those changes, we still need to build those that changed.

```shell
dotnet affected # [..] other args
if [ "$?" -eq 0 ]; then
    dotnet build affected.proj
fi
```

When using GitHub Actions, conditions can be added to skip steps when nothing has changed or is affected:

```yaml
- name: Install dependencies
  if: success() && steps.affected.outputs.affected != ''
  run: dotnet restore affected.proj
```

[Complete example](https://github.com/leonardochaia/dotnet-affected-action#for-building-prs)

## Which projects do I need to re deploy

In order to determine what projects need to be deployed since our previous release, we can use dotnet-affected to
determine which projects were affected from the previous release to the current one.

```shell
dotnet affected --from releases/v1.0.0 --to releases/v2.0.0
```

Of course this assumes that your .NET dependencies also represent system's dependencies. For example, if your systems
communicate through HTTP and you don't share any assemblies between them, this won't work. But, if your systems share a
common assembly with data transfer objects, or auto-generated HttpClients for example, this works wonderful.

## Describe Command

dotnet-affected includes a `describe` command that outputs to stdout in a readable fashion which projects have changed
and which projects are affected by those changes.

```text
$ dotnet affected describe
1 files have changed inside 1 projects
0 NuGet Packages have changed
1 projects are affected by these changes
Changed Projects
Name  Path
      /home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj

Affected Projects
Name                   Path
dotnet-affected.Tests  /home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj
```

## Troubleshooting

Some useful commands and flags are included for troubleshooting or just observing what would be affected by a small
change to a system.

### Dry Running

Sometimes it is useful to see what the tool would do under certain situation.

When adding the `--dry-run` flag, dotnet-affected will write to stdout instead of generating output files.

```text
$ dotnet affected --dry-run
DRY-RUN: WRITE /home/lchaia/dev/dotnet-affected/affected.proj
DRY-RUN: CONTENTS:
<Project Sdk="Microsoft.Build.Traversal/3.0.3">
  <ItemGroup>
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

### Assume Changes

You can also use `--assume-changes some-project-name` in order to fake changes being made to a certain project. This
let's you see what would be affected if that project changed.

```text
$ dotnet-affected --dry-run --assume-changes dotnet-affected.Tests
DRY-RUN: WRITE /home/lchaia/dev/dotnet-affected/affected.proj
DRY-RUN: CONTENTS:
<Project Sdk="Microsoft.Build.Traversal/3.0.3">
  <ItemGroup>
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
  </ItemGroup>
</Project>
```

## Contributing

We accept PRs! Feel free to file issues if you encounter any problem.

If you wanna build the solution, these are the steps:

Note: Windows users, you can either use WSL or GitBash, or use PowerShell replacing `.sh` scripts with `.ps1`

### Installing the SDK

First run this script to locally install the proper versions of the .NET SDKs we are using. This won't affect other .NET
projects that you have.

```shell
./eng/install-sdk.sh
```

It will install the SDKs at `eng/.dotnet`.

### Activating your console

Before running any `dotnet` commands, you need to activate the SDK by running:

```shell
. ./eng/activate.sh
```

If you run `dotnet --info` you should see all SDK installed.

You can then build using.

```shell
dotnet build
```

Or open your favorite ide through the activated command line and it will use the locally installed .NET

```shell
source ./eng/activate.sh
rider Affected.sln
```
