# DotnetAffected.Tasks

An MSBuild project SDK that computes the affected projects during evaluation and hands them to
[Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) for execution.

There is no CLI to install and no intermediate `affected.proj`: you run `dotnet build` against one file, and only the
projects your changes affect are built.

**📖 [Documentation](https://dotnet-affected.com/msbuild-sdk/)**

## Minimal setup

Create a project file at the root of the Git repository — `ci.props` by convention:

```xml
<Project Sdk="DotnetAffected.Tasks/7.0.0;Microsoft.Build.Traversal/4.1.82">
</Project>
```

Then build, test or clean through it:

```shell
dotnet build ./ci.props
dotnet test ./ci.props
```

The SDK registers `DotnetAffectedCheck` as an initial target. It replaces the `ProjectReference` items with the
changed and affected projects, so Traversal builds exactly those.

## Read more

| | |
|---|---|
| [Overview](https://dotnet-affected.com/msbuild-sdk/) | Setup, what happens during a build, choosing the comparison |
| [Filtering by project properties](https://dotnet-affected.com/msbuild-sdk/filtering/) | Narrow the result using each project's own evaluated properties |
| [Reference](https://dotnet-affected.com/msbuild-sdk/reference/) | Every property, item and target |
| [Troubleshooting](https://dotnet-affected.com/msbuild-sdk/troubleshooting/) | Common failures and what they mean |

`LibGit2Sharp-Integration.md` in this directory covers how the native LibGit2Sharp libraries are resolved at build
time. It is for contributors working on this project, not for consumers of the SDK.
