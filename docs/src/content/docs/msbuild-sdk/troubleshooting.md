---
title: SDK troubleshooting
description: Common failures when building through the DotnetAffected.Tasks SDK.
sidebar:
  order: 4
---

## `Path '/x/y/z' doesn't point at a valid Git repository or workdir`

The build is running from a directory that is not the repository root. `DotnetAffectedRoot` defaults to
`$(MSBuildStartupDirectory)` — the directory you invoked `dotnet build` from, not the location of the project file.

Either build from the root, or set the property explicitly:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <DotnetAffectedRoot>$(MSBuildThisFileDirectory)..\..</DotnetAffectedRoot>
    </PropertyGroup>
</Project>
```

## `MSB4062: The "DotnetAffected.Tasks.AffectedTask" task could not be loaded`

The SDK ships a task assembly per target framework and has to pick one. Files using
`DotnetAffected.Tasks` or `Microsoft.Build.Traversal` are framework-agnostic — they delegate the real build — so
there is often no `TargetFramework` to go on, and the SDK infers it:

1. the value of `MicrosoftNETBuildTasksTFM`, if set;
2. otherwise, from `MSBuildVersion` — `net6.0` for 17.0 or newer, `net5.0` for 16.11 or newer.

When the inferred framework is not one the package ships, loading fails. Pin it:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
    </PropertyGroup>
</Project>
```

:::note
An explicit `<TargetFramework>` always wins over the inference above.
:::

## Inputs appear to be ignored

`DotnetAffectedCheck` is an **initial target**: it runs before everything else. Properties and items it reads —
`DotnetAffectedFromRef`, `DotnetAffectedToRef`, `DotnetAffectedAssumeChanges`, `AffectedFilterClass` — must exist by
then.

Set them at evaluation time (a top-level `PropertyGroup`/`ItemGroup`), or in a target declared with
`BeforeTargets="DotnetAffectedCheck"`. A target using `AfterTargets="DotnetAffectedCheck"` runs too late to influence
the result; that hook is for post-processing `ProjectReference` items.

## `DotnetAffectedToRef is deprecated and will be removed in v8`

The task warns whenever the property is set. Projects are discovered and evaluated from the working tree, so the
property is only accepted when it names the commit already checked out — making it a no-op — and the build fails
otherwise. Remove it, and use `DotnetAffectedUncommitted` to choose what the working tree contributes.

## `<path> changed, but no project was found for it`

A changed project file that is not in the graph is reported as an MSBuild warning naming why: `--exclude-discovery`
matched it, the filter file does not reference it, or git ignores the path it is under. The build carries on — the
file counts among the changes while nothing is reported as changed or affected by it, which is exactly the case that
used to pass unnoticed.

## Nothing is built and no error is reported

If no project changed and nothing is affected, the replaced `ProjectReference` list is empty and Traversal has nothing
to build — a successful build that does nothing. Print the count to make this visible:

```xml
<Target Name="_ReportAffected" AfterTargets="DotnetAffectedCheck">
    <Message Text="dotnet-affected selected $(DotnetAffectedProjectCount) projects" Importance="high" />
</Target>
```

## Disabling the SDK temporarily

Set `UsingDotnetAffectedTasks` to anything other than `true` (or empty) to skip the check and leave `ProjectReference`
items untouched — useful for reproducing a full build with the same file:

```bash
dotnet build ci.props -p:UsingDotnetAffectedTasks=false
```
