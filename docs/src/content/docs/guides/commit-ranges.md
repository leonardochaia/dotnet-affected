---
title: Comparing commit ranges
description: Use --from and --to to compare branches, tags or commits instead of your working directory.
sidebar:
  order: 2
---

By default dotnet-affected compares the current `HEAD` against your working directory. `--from` and `--to` replace that
with an explicit range. Both accept a branch name, a tag or a commit SHA.

```bash
# Compares HEAD against the working directory
dotnet affected

# Compares HEAD against the branch chore/target-net7
dotnet affected --from chore/target-net7

# Compares chore/target-net7 against main
dotnet affected --from chore/target-net7 --to main
```

:::caution
`--to` requires `--from`. Passing `--to` on its own is rejected with
`--from is required when using --to`.
:::

When `--from` is given without `--to`, the comparison runs against `HEAD`.

## In CI

CI systems build a *push*, not a single commit, so a build usually covers several commits — and if previous builds
failed, the range should stretch back to the last commit that built successfully. Feed the range from your CI
provider's environment variables:

```bash
# Replace the variables with whatever your CI system provides
dotnet affected \
    --from "$LAST_SUCCESSFUL_BUILD_COMMIT" \
    --to "$CURRENT_COMMIT_HASH"
dotnet test affected.proj
```

For pull requests, compare the target branch against the PR head:

```bash
dotnet affected --from origin/main --to "$CURRENT_COMMIT_HASH"
dotnet test affected.proj
```

[nrwl/last-successful-commit-action](https://github.com/nrwl/last-successful-commit-action) explains the
last-successful-build problem in depth and provides the value on GitHub Actions. The
[dotnet-affected GitHub Action](https://github.com/leonardochaia/dotnet-affected-action) wires the two together.

:::note
Make sure the refs you compare actually exist in the clone. Shallow clones — the default on many CI systems — often
lack the history the range needs. On GitHub Actions, use `fetch-depth: 0`.
:::

## Deciding what to redeploy

The same mechanism answers "what do I need to redeploy since the last release?":

```bash
dotnet affected --from releases/v1.0.0 --to releases/v2.0.0
```

This assumes your .NET dependencies mirror your system dependencies. If your services only talk over HTTP and share no
assemblies, the result will not reflect deployment impact. If they share DTO assemblies or generated HTTP clients, it
works well.
