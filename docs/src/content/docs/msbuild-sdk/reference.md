---
title: SDK reference
description: Every property, item and target exposed by the DotnetAffected.Tasks SDK.
sidebar:
  order: 3
---

## Target

### `DotnetAffectedCheck`

The target that computes the affected projects. It is registered as an **initial target**, so it runs before every
other target in the build. It replaces all `ProjectReference` items with the changed and affected projects.

Hook `BeforeTargets="DotnetAffectedCheck"` to set inputs, `AfterTargets="DotnetAffectedCheck"` to post-process the
result.

## Inputs

| Name                           | Kind     | Type       | Description                                                                                                   |
|--------------------------------|----------|------------|----------------------------------------------------------------------------------------------------------------|
| `UsingDotnetAffectedTasks`     | Property | bool       | Whether `DotnetAffectedCheck` runs. Empty or `true` runs it; any other value disables the SDK. Default: `true` |
| `DotnetAffectedRoot`           | Property | string     | Repository root, where `.git` lives. Default: `$(MSBuildStartupDirectory)`                                     |
| `DotnetAffectedFromRef`        | Property | string     | Branch or commit to compare the working tree against. Default: empty, meaning `HEAD`                           |
| `DotnetAffectedUncommitted`    | Property | string     | What the working tree contributes on top of the commits since `FromRef`: `All`, `Staged` or `None`. Default: `All` |
| `DotnetAffectedToRef`          | Property | string     | **Deprecated, removed in v8.** Only accepted when it names the commit the working tree is checked out at        |
| `DotnetAffectedHonourGitIgnore`| Property | bool       | Whether discovery skips paths git ignores. Set to `false` to search everything. Default: `true`                |
| `DotnetAffectedAssumeChanges`  | Item     | item list  | Projects to treat as changed instead of using Git. Globs expand to paths; bare values match by project name    |
| `AffectedFilterClass`          | Item     | item list  | Property templates read from each affected project — see [Filtering](/msbuild-sdk/filtering/)                  |

## Outputs

| Name                          | Kind     | Type      | Description                                                                    |
|-------------------------------|----------|-----------|---------------------------------------------------------------------------------|
| `DotnetAffectedProjectCount`  | Property | int       | Number of changed plus affected projects                                        |
| `ProjectReference`            | Item     | item list | The changed and affected projects; replaces whatever was there before           |
| `AffectedFilterInstance`      | Item     | item list | One item per project per filter class, carrying the requested property values   |

## Extensibility hooks

| Property                      | Description                                                                     |
|-------------------------------|----------------------------------------------------------------------------------|
| `CustomBeforeAffectedProps`   | MSBuild projects imported **before** the SDK's `.props`                          |
| `CustomAfterAffectedProps`    | MSBuild projects imported **after** the SDK's `.props`                           |
| `CustomBeforeAffectedTargets` | MSBuild projects imported **before** the SDK's `.targets`                        |
| `CustomAfterAffectedTargets`  | MSBuild projects imported **after** the SDK's `.targets`                         |

These keep ad-hoc targets out of your `.props` file:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <CustomAfterAffectedTargets>$(CustomAfterAffectedTargets);$(MSBuildThisFileDirectory)ci.targets</CustomAfterAffectedTargets>
    </PropertyGroup>
</Project>
```

```xml
<!-- ci.targets -->
<Project>
    <Target Name="_DotnetAffectedCheck" AfterTargets="DotnetAffectedCheck">
        <ItemGroup>
            <ProjectReference Remove="$(MSBuildThisFileDirectory)src/DevTools/**/*.csproj" />
        </ItemGroup>
    </Target>
</Project>
```

All of [Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal)'s extensibility
options apply as well.

## Referencing the SDK as a package

If you cannot use the `Sdk` attribute — for example because you need the package resolved through Central Package
Management — import the SDK files by path instead:

```xml
<Project Sdk="Microsoft.Build.Traversal/4.1.82">
    <ItemGroup>
        <!-- With Central Package Management -->
        <PackageReference Include="DotnetAffected.Tasks" GeneratePathProperty="true" />
        <PackageVersion Include="DotnetAffected.Tasks" Version="6.2.0" />

        <!-- Without it -->
        <!-- <PackageReference Include="DotnetAffected.Tasks" Version="6.2.0" GeneratePathProperty="true" /> -->
    </ItemGroup>

    <Import Project="$(PKGDotnetAffected_Tasks)/Sdk/Sdk.props" />
    <Import Project="$(PKGDotnetAffected_Tasks)/Sdk/Sdk.targets" />
</Project>
```
