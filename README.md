# dotnet-affected

A .NET Tool for determining which projects are affected by a set of changes.

The tool can output the affected project list for informative purposes, or
it can generate an MSBuild Traversal SDK file in order to build / test what is
affected only

This tool is useful for large projects or monorepos.
Ideally one would use Bazel to do this. However, integrating Bazel with an existing
solution may be an extensive task. This tool integrates seamlessly with your existing
build system and can take advantage of MSBuild project files in order to build a project
graph.

## Caveats

1. It currently works for .csproj only, however, changing to support \*proj should be trivial
1. SDK projects only. Previous csproj format is not supported.

## Installation

The tool can be installed using `dotnet install`:

```bash
dotnet tool install --global dotnet-affected --version 1.0.0-preview-4
```

You can then run the tool using `dotnet affected` in the root of your repository.

```bash
$ dotnet affected --help
affected:
  Determines which projects are affected by a set of changes.

Usage:
  affected [options] [command]

Options:
  -p, --repository-path <repository-path>    Path to the root of the repository, where the .git directory is.
                                             [default: current directory]
  -v, --verbose                              Write useful messages or just the desired output. [default: False]
  --assume-changes <assume-changes>          Hypothetically assume that given projects have changed instead of using Git diff to
                                             determine them.
  --from <from>                              A branch or commit to compare against --to.
  --to <to>                                  A branch or commit to compare against --from
  --version                                  Show version information
  -?, -h, --help                             Show help and usage information

Commands:
  generate    Generates a Microsoft.Sdk.Traversal project which includes all affected projects as build targets.
  changes     Finds projects that have any changes in any of its files using Git
```

## Examples

### Generate a project to build all affected projects

The tool can generate an MSBuild Traversal project that can be used to build or test
only the projects that are affected by a change.

```bash
$ dotnet affected --verbose generate
Finding all csproj at /home/lchaia/development/dotnet-affected
Building Dependency Graph
Found 2 projects
Finding changes from working directory against 545ee40eb9b99c624f75eb54d2b5a63947ad30b7
Found 2 changed files inside 1 projects.

Files inside these projects have changed:
        dotnet-affected

These projects are affected by those changes:
        dotnet-affected.Tests

Generating Traversal SDK Project

<Project Sdk="Microsoft.Build.Traversal/3.0.3">
  <ItemGroup>
    <ProjectReference Include="/home/lchaia/development/dotnet-affected/src/dotnet-affected.Tests/dotnet-affected.Tests.csproj" />
    <ProjectReference Include="/home/lchaia/development/dotnet-affected/src/dotnet-affected/dotnet-affected.csproj" />
  </ItemGroup>
</Project>
```

The `dotnet affected generate` command is most useful when using the `--output`
flag to generate a separate file.

```bash
$ dotnet affected generate --verbose --output build.proj
Finding all csproj at /home/lchaia/development/dotnet-affected
Building Dependency Graph
Found 2 projects
Finding changes from working directory against 545ee40eb9b99c624f75eb54d2b5a63947ad30b7
Found 2 changed files inside 1 projects.

Files inside these projects have changed:
        dotnet-affected

These projects are affected by those changes:
        dotnet-affected.Tests

Generating Traversal SDK Project

Creating file at build.proj
```

You can then build / test the affected projects by running the resulting file against
the `dotnet` CLI.

```bash
dotnet test build.proj
```

### Continuos Integration

For usage in CI, it's recommended to use the `--from` and `--to` options, which allows
you to provide commit hashes or branch names to git diff. This is used in combination
with the environment variables provided by your build tool.

```bash
dotnet affected generate --from $PREVIOUS_COMMIT_HASH --to $CURRENT_COMMIT_HASH
```

### Which projects do I need to re deploy

In order to determine what projects need to be deployed since our previous release,
we can use the tool to determine which projects are affected from the previous
release to the current one.

```bash
dotnet affected --from releases/v1.0.0 --to releases/v2.0.0
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

To build use the SDK you first need to activate it, by running:

```bash
source ./eng/activate.sh
```

You can then build using

```bash
dotnet build
```
