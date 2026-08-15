---
title: MSBuild SDK
description: Run dotnet-affected directly from MSBuild with the DotnetAffected.Tasks SDK, no CLI required.
sidebar:
  order: 1
---

`DotnetAffected.Tasks` is an MSBuild project SDK that computes the affected projects during evaluation and hands them
to [Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) for execution. There
is no CLI to install and no intermediate `affected.proj` — you run `dotnet build` against one file and only the
affected projects are built.

## Set it up

Create a project file **at the root of the Git repository**. The name is yours to choose; `ci.props` is the
convention.

```xml
<Project Sdk="DotnetAffected.Tasks/6.2.0;Microsoft.Build.Traversal/4.1.82">
</Project>
```

Then build, test or clean through it:

```bash
dotnet build ./ci.props
dotnet test ./ci.props
```

Instead of repeating versions in every file, pin them in `global.json`:

```json
{
    "msbuild-sdks": {
        "DotnetAffected.Tasks": "6.2.0",
        "Microsoft.Build.Traversal": "4.1.82"
    }
}
```

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
</Project>
```

:::note
An MSBuild project SDK obtained from NuGet always needs a version — either appended to the SDK name or declared in
`global.json`. Since MSBuild 15.6 the package is restored automatically. See
[How to use project SDKs](https://learn.microsoft.com/visualstudio/msbuild/how-to-use-project-sdk).
:::

## What it does during a build

The SDK adds `DotnetAffectedCheck` as an **initial target**, so it runs before anything else. The target computes the
changed and affected projects, clears any existing `ProjectReference` items and replaces them with the result. From
there the Traversal SDK builds exactly those projects.

Two consequences follow:

- Anything that *feeds* the check — the commit range, assumed changes, filter classes — must be in place **before**
  `DotnetAffectedCheck` runs: at evaluation time, or in a target with `BeforeTargets="DotnetAffectedCheck"`.
- Anything that *post-processes* the result belongs in a target with `AfterTargets="DotnetAffectedCheck"`.

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <Target Name="_PrintAffected" AfterTargets="DotnetAffectedCheck">
        <Message Text="Found $(DotnetAffectedProjectCount) projects:" Importance="high" />
        <Message Text="  >> %(ProjectReference.Identity)" Importance="high" />
    </Target>
</Project>
```

## Choosing the comparison

By default the comparison is the working directory against `HEAD`, exactly like the CLI. Set the range with
properties:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <DotnetAffectedFromRef>origin/main</DotnetAffectedFromRef>
        <DotnetAffectedToRef>$(BUILD_SOURCEVERSION)</DotnetAffectedToRef>
    </PropertyGroup>
</Project>
```

Or bypass Git entirely and assume changes, by name or by glob:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <ItemGroup>
        <!-- Every csproj matching the pattern -->
        <DotnetAffectedAssumeChanges Include="$(MSBuildThisFileDirectory)**/project-*.csproj" />
        <!-- Or by project name — this throws if no project matches -->
        <DotnetAffectedAssumeChanges Include="project3" />
    </ItemGroup>
</Project>
```

:::caution
Setting `DotnetAffectedAssumeChanges` together with `DotnetAffectedFromRef`/`DotnetAffectedToRef` logs a warning and
uses only the assumed changes.
:::

See the [SDK reference](/msbuild-sdk/reference/) for every input and output, and
[Filtering by project properties](/msbuild-sdk/filtering/) for narrowing the result using each project's own evaluated
properties.
