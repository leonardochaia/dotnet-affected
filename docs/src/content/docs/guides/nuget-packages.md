---
title: NuGet package changes
description: How changes to Directory.Packages.props turn into affected projects.
sidebar:
  order: 7
---

A dependency bump is not a source change, but it still requires rebuilding whatever consumes it. dotnet-affected
detects package version changes and treats the projects that reference those packages as affected.

## What triggers detection

Package detection runs when the diff contains a file named `Directory.Packages.props`. The tool then evaluates each
affected project **twice** — once at the `--from` baseline, once at the working tree — and compares the resulting
package sets. Those are the same two revisions the file diff uses, so an uncommitted version bump counts exactly like
any other uncommitted change, subject to `--uncommitted`.

Which items are compared depends on how the project manages versions:

| Project setup                                              | Items compared     |
|------------------------------------------------------------|--------------------|
| [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management) (default) | `PackageVersion`   |
| `ManagePackageVersionsCentrally` set to `false`             | `PackageReference` |

Packages are reported as **added**, **removed** or **updated**, and the counts show up in verbose output:

```text
1 files have changed referenced by 1 projects
3 NuGet Packages have changed
7 projects are affected by these changes
```

## Which projects become affected

A project is affected by a package change only when it actually references that package — then its dependents follow
through the graph as usual.

This matters with Central Package Management: `Directory.Packages.props` typically declares versions for the whole
repository, and bumping one entry would otherwise mark every project as affected. Instead, only the projects that
reference that package (and their dependents) are selected.

## Details that affect the comparison

- **Conditions are part of a package's identity.** Two `PackageVersion` entries for the same package under different
  `Condition`s are tracked separately, so a change under one condition does not look like a change to the other.
- **`VersionOverride` is honoured** when `EnablePackageVersionOverride` is not disabled — the overriding version is
  what gets compared for that project.
- **Implicitly defined packages are ignored** — items carrying `IsImplicitlyDefined="true"`, such as the ones the SDK
  adds for you, never register as changes.
- **Imports are followed.** A `Directory.Packages.props` that imports other files is compared as the fully evaluated
  result, and when a package/condition pair is declared more than once, the last value wins — the same rule MSBuild
  applies.

## Nested `Directory.Packages.props`

Repositories may have several, at different depths. Only the outermost changed file in any given branch of the tree is
used: if the one at the repository root changed, it alone drives the comparison; otherwise, nested files that are not
contained by another changed file each apply to the projects beneath them.

:::note
Detection is keyed on the file name `Directory.Packages.props`. Version changes made anywhere else — a `PackageVersion`
moved into a hand-rolled `Versions.props`, for instance — are only picked up as an ordinary file change of whichever
projects import that file.
:::
