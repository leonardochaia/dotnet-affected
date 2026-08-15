---
title: Continuous integration
description: Running dotnet-affected in CI, and skipping work when nothing changed.
sidebar:
  order: 1
---

dotnet-affected runs anywhere `dotnet` does. Install it as a local tool, restore it, and call it like you would
locally — there is nothing CI-specific about the tool itself. What CI adds is the need to pick a commit range and to
skip steps when there is nothing to do.

## The shape of a job

```bash
dotnet tool restore

dotnet affected \
    --from "$LAST_SUCCESSFUL_BUILD_COMMIT" \
    --to "$CURRENT_COMMIT_HASH"

dotnet build affected.proj
dotnet test affected.proj
```

## Choosing the range

Pick the range from what your CI provider gives you — see [Comparing commit ranges](/guides/commit-ranges/) for
branch, pull-request and release-to-release comparisons.

Two things bite in practice:

- **Builds cover pushes, not commits.** A push can carry several commits, and if earlier builds failed the range must
  reach back to the last commit that built successfully. `nrwl/last-successful-commit-action` documents
  [the problem](https://github.com/nrwl/last-successful-commit-action#problem) and provides that value on GitHub
  Actions.
- **Shallow clones lack history.** Most CI systems clone with depth 1 by default, and the refs you want to compare
  simply are not there. Fetch full history — on GitHub Actions, `fetch-depth: 0`.

## Skipping the rest of the pipeline

When nothing changed and nothing is affected, the tool writes no output file and exits with
[`166`](/reference/exit-codes/). Guard the following steps rather than letting them fail on a missing `affected.proj`:

```bash
dotnet affected # ...other args
if [ "$?" -eq 0 ]; then
    dotnet build affected.proj
fi
```

For deployment pipelines, the same run answers "what do I need to ship?" — compare the previous release tag against
the current one.

## Providers

A ready-made action exists for [GitHub Actions](/ci/github-actions/). For other systems, install and run the tool
directly; contributions that simplify this for other providers are welcome.
