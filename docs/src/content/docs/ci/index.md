---
title: Continuous integration
description: Running dotnet-affected in CI, and skipping work when nothing changed.
sidebar:
  order: 1
---

dotnet-affected runs anywhere `dotnet` does. Install it as a local tool, restore it, and call it like you would
locally — there is nothing CI-specific about the tool itself. What CI adds is the need to pick a baseline and to skip
steps when there is nothing to do.

## The shape of a job

```bash
dotnet tool restore

dotnet affected \
    --from "$LAST_SUCCESSFUL_BUILD_COMMIT" \
    --uncommitted none

dotnet build affected.proj
dotnet test affected.proj
```

## Choosing the baseline

CI checks out the revision being built, and projects are discovered from that working tree — so the end of the
comparison is already right and `--from` is the only ref to supply. See
[Choosing what to compare](/guides/commit-ranges/) for branch, pull-request and release-to-release setups.

Three things bite in practice:

- **Builds cover pushes, not commits.** A push can carry several commits, and if earlier builds failed the baseline
  must reach back to the last commit that built successfully. `nrwl/last-successful-commit-action` documents
  [the problem](https://github.com/nrwl/last-successful-commit-action#problem) and provides that value on GitHub
  Actions.
- **Shallow clones lack history.** Most CI systems clone with depth 1 by default, and the baseline you want simply is
  not there — nor is the merge base a pull request needs. Fetch full history — on GitHub Actions, `fetch-depth: 0`.
- **Earlier steps can change the answer.** Code generation, a version stamp or a formatter writing to a tracked file
  before dotnet-affected runs counts as a change. Pass `--uncommitted none` so the result depends only on the commits.

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

A ready-made action exists for [GitHub Actions](/github-action/). For other systems, install and run the tool
directly; contributions that simplify this for other providers are welcome.
