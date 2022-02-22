# dotnet-affected

A .NET Tool for determining which projects are affected by a set of changes.

The tool can output the affected project list for informative purposes, or
it can generate an [MSBuild Traversal SDK](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal)
file which you can then use to build and test what is affected only.

This tool is particularly useful for large projects or monorepos.
Ideally one would use Bazel to do this. However, integrating Bazel with an existing
solution may be an extensive task.

dotnet-affected integrates seamlessly with your existing build system.
It uses `git diff` to determine which projects have changed.
Then, it uses MSBuild to build a graph of all your projects and its dependencies.
We can then determine which projects are affected by the changes that `git diff` gave us.

For example, given this project structure:

1. Project.Shared
2. Project (depends on .1)
3. Project.Tests (depends on .2)
4. Shared.Tests (depends on .1)

When .2 changes, .3 will be affected so we will build and test .2 and .3.
There's no need to build and test .4 since .1 has not changed.

## Caveats

1. It currently works for .csproj only, however, changing to support \*proj should be trivial
1. SDK projects only. Previous csproj format is not supported.

## Installation

The tool can be installed using `dotnet install`:

```bash
dotnet tool install --global dotnet-affected --version 2.1.0
```

You can then run the tool using `dotnet affected` in the root of your repository.

```text
$ dotnet affected --help
affected
  Determines which projects are affected by a set of changes.

Usage:
  affected [options] [command]

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

## Examples

### Project Discovery

By default, projects are discovered from from the filesystem, by recursively searching
the current working directory.

When using `--repository-path`, projects will be discovered from that path.

One can narrow down which projects should be considered by the tool by providing
a Solution file using the `--solution-file` arg. When doing so, only the projects
included in the Solution will be considered for changes.

For example, when using a Solution file, if changes are made to projects not referenced
by the Solution file, those changes will be ignored by the dotnet affected
and it will assume nothing has changed.

Note that, if your Solution file is not at the root of your Git Repository (where the `.git` directory is),
you still need to specify `--repository-path`.

For example:

```bash
dotnet affected --repository-path /home/lchaia/monorepo --solution-path /home/lchaia/monorepo/directory/MySolution.sln
```

### Build only affected projects

In order to build only what is affected, the tool outputs an
MSBuild Traversal project that can can then be used with `dotnet build`.

For example, the below command outputs `affected.proj` at the current directory.

```text
$ dotnet affected --verbose
Building Dependency Graph
Built Graph with 6 Projects in 0.36s
Found 2 changed files inside 1 projects.
WRITE: /home/lchaia/dev/dotnet-affected/affected.proj
```

The contents of `affected.proj` are:

```xml
<Project Sdk="Microsoft.Build.Traversal/3.0.3">
  <ItemGroup>
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

You can then use `dotnet test` (or any other dotnet commands) against the resulting `affected.proj` file:

```bash
dotnet test affected.proj
```

### Output Formatting

dotnet-affected currently supports outputting Traversal SDK project files
and plain text list of changed and affected projects.

```text
dotnet affected --dry-run --format text
DRY-RUN: WRITE /home/lchaia/dev/dotnet-affected/affected.txt
DRY-RUN: CONTENTS:
/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj
/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj
```

Multiple formats can be outputted at the same time by space separating them:

```text
dotnet affected --dry-run --format text traversal
DRY-RUN: WRITE /home/lchaia/dev/dotnet-affected/affected.txt
DRY-RUN: CONTENTS:
/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj
/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj

DRY-RUN: WRITE /home/lchaia/dev/dotnet-affected/affected.proj
DRY-RUN: CONTENTS:
<Project Sdk="Microsoft.Build.Traversal/3.0.3">
  <ItemGroup>
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

### Continuos Integration

For usage in CI, it's recommended to use the `--from` and `--to` options, which allows
you to provide commit hashes or branch names to `git diff`. This is used in combination
with the environment variables provided by your build tool.

For example, for building a main branch one may use a setup similar to this:

```bash
# Replace env vars with what your CI system gives you
dotnet affected \
    --from $LAST_SUCCESSFUL_BUILD_COMMIT \
    --to $CURRENT_COMMIT_HASH
dotnet test affected.proj
```

For building PRs, something like this perhaps:

```bash
dotnet affected generate --from $GIT_BRANCH --to origin/main
dotnet test affected.proj
```

It's important to note that CI system usually triggers a build per push, not per commit.
Which means a set of commits may be built, instead of just one.
So keep that in mind, you may need something like "the commit of the latest successful build for this branch".

Or just use `--to` and `--from` with branch names, which will compare the two branches.

### Detecting when nothing has changed

If nothing has changed, or the changes are not related to any of the discovered projects,
there is no need to run `dotnet test`.

In order to detect this, dotnet affected will exit with an exit status code `166`.
You can use this to prevent spending time on unnecessary tasks when nothing has changed.

Note that dotnet affected returns `166` when nothing has changed, not to be confused when nothing is affected.
If projects have changed, but nothing is affected by those changes, we still need to build those that changed.

```bash
dotnet affected # [..] other args
if [ "$?" -eq 0 ]; then
    dotnet build affected.proj
fi
```

### Which projects do I need to re deploy

In order to determine what projects need to be deployed since our previous release,
we can use dotnet-affected to determine which projects were affected from the previous
release to the current one.

```bash
dotnet affected --from releases/v1.0.0 --to releases/v2.0.0
```

Of course this assumes that your .NET dependencies also represent system's dependencies.
For example, if your systems communicate through HTTP and you don't share any assemblies between them,
this won't work.
But, if your systems share a common assembly with data transfer objects, or auto-generated HttpClients
for example, this works wonderful.

### Describe Command

dotnet-affected includes a `describe` command that outputs to stdout in a readable fashion
which projects have changed and which projects are affected by those changes.

```dotnet affected describe
Files inside these projects have changed:
    dotnet-affected
These projects are affected by those changes:
    dotnet-affected.Tests
```

### Troubleshooting

Some useful commands and flags are included for troubleshooting or just
observing what would be affected by a small change to a system.

#### Dry Running

Sometimes it is useful to see what the tool would do under certain situation.

When adding the `--dry-run` flag, dotnet-affected will write to stdout
instead of generating output files.

```text
dotnet affected --dry-run
DRY-RUN: WRITE /home/lchaia/dev/dotnet-affected/affected.proj
DRY-RUN: CONTENTS:
<Project Sdk="Microsoft.Build.Traversal/3.0.3">
  <ItemGroup>
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
    <ProjectReference Include="/home/lchaia/dev/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

#### Assume Changes

You can also use `--assume-changes some-project-name` in order to fake changes
being made to a certain project. This let's you see what would be affected
if that project changed.

```text
dotnet-affected --dry-run --assume-changes dotnet-affected.Tests
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

Note: our build infra supports UNIX only, that being said,
if you install the proper SDK and just build the solution it should work

First run this script to locally install the proper version of the .NET SDK we are using.
This won't affect other .NET projects that you have.

```bash
./eng/install-sdk.sh
```

It will install the SDK at `eng/.dotnet`.

Before running any `dotnet` commands, you need to activate the SDK by running:

```bash
source ./eng/activate.sh
```

You can then build using.

```bash
dotnet build
```

Or open your favorite ide through the activated command line and it will use the locally installed .NET

```bash
source ./eng/activate.sh
rider Affected.sln
```
