---
title: Filtering by project properties
description: Use AffectedFilterClass to inspect each affected project's evaluated properties and filter on them.
sidebar:
  order: 2
---

The affected set is computed from the dependency graph, which knows nothing about what a project *is*. Filter classes
let you decide using each project's own evaluated MSBuild properties — "only ship client libraries", "never build
back-office code", and so on.

## Declaring a filter class

An `AffectedFilterClass` item is a template: a name plus the list of properties you want read from every affected
project.

```xml
<ItemGroup>
    <AffectedFilterClass Include="No Backoffice">
        <IsBackofficeLibrary />
        <!-- more properties... -->
    </AffectedFilterClass>
</ItemGroup>
```

For every affected project, the SDK emits one `AffectedFilterInstance` item per declared class:

- **`Identity`** is the project's full path.
- Each declared property carries that project's evaluated value.
- **`AffectedFilterClassName`** carries the class's `Include` value, so several classes can coexist.

A property the project does not define keeps the value written in the class declaration — which makes the declaration
a place to put defaults. Note that this applies to properties that *do not exist*; a property evaluating to an empty
string exists, and the empty value wins.

## Filtering on the result

Filter instances are available in a target running after the check, where `ProjectReference` can still be adjusted:

```xml
<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <ItemGroup>
        <AffectedFilterClass Include="No Backoffice">
            <IsBackofficeLibrary />
        </AffectedFilterClass>
    </ItemGroup>

    <Target Name="_FilterAffected" AfterTargets="DotnetAffectedCheck">
        <ItemGroup>
            <ProjectReference Remove="@(AffectedFilterInstance)"
                              Condition="'%(AffectedFilterInstance.IsBackofficeLibrary)' == 'true'" />
        </ItemGroup>

        <Message Text="Filtered: %(AffectedFilterInstance.Identity)"
                 Condition="'%(AffectedFilterInstance.IsBackofficeLibrary)' == 'true'"
                 Importance="high" />
    </Target>
</Project>
```

## Allow-listing instead

The opposite shape — clear everything, then add back what qualifies — works just as well, and is the natural way to
combine several classes:

```xml
<ItemGroup>
    <AffectedFilterClass Include="Rule -> IsShippingClientLibrary">
        <IsShippingClientLibrary />
    </AffectedFilterClass>
    <AffectedFilterClass Include="Rule -> ForceClientLibrary">
        <ForceClientLibrary />
    </AffectedFilterClass>
</ItemGroup>

<Target Name="_OnlyClientLibraries" AfterTargets="DotnetAffectedCheck">
    <ItemGroup>
        <ProjectReference Remove="@(ProjectReference)" />
        <ProjectReference Include="@(AffectedFilterInstance)"
                          Condition="'%(AffectedFilterInstance.IsShippingClientLibrary)' == 'true'
                                     OR '%(AffectedFilterInstance.ForceClientLibrary)' == 'true'" />
    </ItemGroup>
</Target>
```

:::note
With two classes declared there are two `AffectedFilterInstance` items per project, so the re-added list contains
duplicates. MSBuild removes duplicate `ProjectReference` items, so this is harmless.
:::

## Cost

Reading properties means each affected project is evaluated. Declaring filter classes therefore does more work than a
plain run — keep the property list to what you actually branch on.
