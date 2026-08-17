---
title: Choosing what to compare
description: Use --from to pick the baseline, and --uncommitted to decide what the working tree contributes.
sidebar:
  order: 2
---

Projects are discovered and evaluated from your **working tree**, so the working tree is always one end of the
comparison. You choose the other end with `--from`, and how much of the working tree counts with `--uncommitted`.

```bash
# HEAD against the working tree — the default
dotnet affected

# A branch against the working tree
dotnet affected --from chore/target-net7

# A tag against the working tree
dotnet affected --from releases/v1.0.0
```

`--from` accepts a branch name, a tag or a commit sha, and defaults to `HEAD`.

:::note[There is no `--to`]
Comparing two arbitrary revisions is not supported, because only the checked-out revision's project structure can be
analysed. To compare two revisions, check the later one out first:

```bash
git checkout releases/v2.0.0
dotnet affected --from releases/v1.0.0 --uncommitted none
```

`--to` still exists, is deprecated, and will be removed in v8 — see [below](#the---to-option).
:::

## Uncommitted work

`--uncommitted` decides what the working tree adds on top of the commits since `--from`:

```bash
# Everything, including files git does not track yet. The default.
dotnet affected --uncommitted all

# Staged changes only — what a pre-commit hook wants
dotnet affected --uncommitted staged

# Nothing: compare commits only, ignoring a dirty working tree
dotnet affected --from origin/main --uncommitted none
```

`staged` is what makes the tool usable from a pre-commit hook: the comparison then describes the commit that is about
to be made, not whatever else is lying around in the tree.

## In CI

CI checks out the revision being built, so the working tree is already the end of the comparison you want. `--from` is
the only ref to supply, and `--uncommitted none` keeps the answer dependent on the commits alone:

```bash
dotnet affected --from "$LAST_SUCCESSFUL_BUILD_COMMIT" --uncommitted none
dotnet test affected.proj
```

Without `--uncommitted none`, any step that writes to a tracked file before dotnet-affected runs — code generation, a
version stamp, a formatter — changes which projects are reported.

### Pull requests

Compare from the **merge base**: the commit the branch was actually cut from.

```bash
dotnet affected --from "$(git merge-base origin/main HEAD)" --uncommitted none
```

Not the base branch's tip. GitHub's `base.sha`, for instance, is the tip at the time the PR was opened; if the base
branch has moved since, comparing from it sweeps in every unrelated commit that landed in the meantime.

:::caution
The merge base needs history. Shallow clones — the default on most CI systems — do not have it. On GitHub Actions, use
`fetch-depth: 0`.
:::

### Branch builds

A push covers several commits, and if earlier builds failed the range must reach back to the last commit that built
successfully. [nrwl/last-successful-commit-action](https://github.com/nrwl/last-successful-commit-action) explains
[the problem](https://github.com/nrwl/last-successful-commit-action#problem) and supplies that value on GitHub
Actions. The [dotnet-affected action](https://github.com/leonardochaia/dotnet-affected-action) wires the two together
— see [GitHub Action](/github-action/).

## Deciding what to redeploy

The same mechanism answers "what do I need to redeploy since the last release?" — check out the release being
deployed, and compare from the previous one:

```bash
git checkout releases/v2.0.0
dotnet affected --from releases/v1.0.0 --uncommitted none
```

The checkout matters: projects are discovered from the working tree, so it has to be at the release being deployed.

This assumes your .NET dependencies mirror your system dependencies. If your services only talk over HTTP and share no
assemblies, the result will not reflect deployment impact. If they share DTO assemblies or generated HTTP clients, it
works well.

## The `--to` option

`--to` is obsolete and will be removed in v8. It is accepted only when it names the commit the working tree is already
checked out at — which makes it a no-op — and refused otherwise:

```text
--to was given 'v1.0.0', but the working tree is checked out at 3a3266a. [...] Check out 'v1.0.0' before running
and drop --to to compare against the working tree.
```

Using it at all prints a deprecation warning on stderr. Before v7, `--to` accepted any revision, and a project added
between the two ends was counted among the changed files while being reported under no project at all — silently, with
exit code `0`. Refusing the case is what makes that impossible.

Migrating is mechanical:

```bash
# Before
dotnet affected --from origin/main --to "$CURRENT_COMMIT_HASH"

# After — CI already has that commit checked out
dotnet affected --from origin/main --uncommitted none
```
