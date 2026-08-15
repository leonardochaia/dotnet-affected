---
title: Installation
description: Install dotnet-affected as a local or global .NET tool.
sidebar:
  order: 1
---

dotnet-affected is distributed as a [.NET tool](https://learn.microsoft.com/dotnet/core/tools/global-tools).

## Local tool (recommended)

Installing it as a local tool records the version in your repository's tool manifest, so every developer and every CI
run uses the same version.

```bash
dotnet new tool-manifest
dotnet tool install dotnet-affected
```

This creates (or updates) `.config/dotnet-tools.json`, which you should commit. On a fresh clone, restore the tools
before using them:

```bash
dotnet tool restore
```

## Global tool

```bash
dotnet tool install --global dotnet-affected
```

A global install is convenient for trying the tool out, but it is not pinned per repository — different machines can
end up on different versions.

## Verify the installation

Run the tool from the root of your repository:

```bash
dotnet affected --help
```

You should see the usage output described in the [CLI reference](/reference/cli/).

:::note
The tool needs to run against a Git repository, since it uses `git diff` to determine what changed. If you run it
somewhere without a `.git` directory, point it at your repository with `--repository-path`.
:::

## Without installing: the MSBuild SDK

dotnet-affected can also run directly from MSBuild, without installing the CLI, using the
[`DotnetAffected.Tasks` SDK](/msbuild-sdk/). MSBuild restores it like any other package, so there is nothing to
install up front.
