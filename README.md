<h1 align="center">dotnet-affected</h1>

<p align="center">
  Build and test only the projects your changes actually affect.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/dotnet-affected">
    <img src="https://img.shields.io/nuget/v/dotnet-affected?logo=nuget&label=dotnet-affected" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/DotnetAffected.Tasks">
    <img src="https://img.shields.io/nuget/v/DotnetAffected.Tasks?logo=nuget&label=DotnetAffected.Tasks" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/dotnet-affected">
    <img src="https://img.shields.io/nuget/dt/dotnet-affected?label=downloads" alt="Downloads"></a>
  <a href="https://github.com/leonardochaia/dotnet-affected/actions/workflows/dotnet.yml">
    <img src="https://github.com/leonardochaia/dotnet-affected/actions/workflows/dotnet.yml/badge.svg" alt="Build"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT"></a>
</p>

<p align="center">
  <b><a href="https://dotnet-affected.com">📖 Documentation</a></b>
</p>

---

A .NET tool that works out which MSBuild projects a set of changes affects, so large repositories and
monorepos can build and test a fraction of themselves instead of all of it.

It compares your working tree against a baseline commit, maps the changed files to the projects that
reference them, and walks the MSBuild project graph to find everything that depends on them.

## Install

```shell
dotnet new tool-manifest
dotnet tool install dotnet-affected
```

## Use

```shell
$ dotnet affected --from origin/main --uncommitted none
WRITE: /repo/affected.proj
```

`affected.proj` is an [MSBuild Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal)
project, so it behaves like a solution:

```shell
dotnet build affected.proj
dotnet test affected.proj
```

When nothing changed, no file is written and the tool exits `166` — check for it and skip the rest of
the pipeline.

## Documentation

| | |
|---|---|
| [Getting started](https://dotnet-affected.com/getting-started/installation/) | Install, first run, how it works |
| [Guides](https://dotnet-affected.com/guides/build-and-test/) | Discovery, output formats, exclusions, NuGet changes |
| [CLI reference](https://dotnet-affected.com/reference/cli/) | Every option and exit code |
| [MSBuild SDK](https://dotnet-affected.com/msbuild-sdk/) | Run it from MSBuild, without the CLI |
| [GitHub Action](https://dotnet-affected.com/github-action/) | Run it in GitHub Actions |
| [Upgrading to v7](https://dotnet-affected.com/upgrading/v6-to-v7/) | What changed, and what to update |

## Features

- Detects which projects changed, from any file they reference — not just source files.
- Detects NuGet package changes when using Central Package Management.
- Picks up `Directory.Build.props`/`.targets` and other imported files.
- Outputs a Traversal project, a Solution Filter, plain text or JSON.
- Supports `.csproj`, `.fsproj` and `.vbproj`, SDK and non-SDK style.

## Contributing

Issues and pull requests are welcome. See
[Building the project](https://dotnet-affected.com/contributing/building/) to get set up.

## License

[MIT](LICENSE)
