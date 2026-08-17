---
title: Exit codes
description: What dotnet-affected returns, and how to use exit code 166 to skip unnecessary work.
sidebar:
  order: 2
---

| Code  | Meaning                                                              |
|-------|-----------------------------------------------------------------------|
| `0`   | Projects changed and/or are affected. Output files were written.       |
| `166` | Nothing changed and nothing is affected. No output files were written. |
| other | An error occurred. The message is printed to stderr.                   |

## Nothing changed: `166`

When the diff produces no changed projects **and** no affected projects, the tool prints
`No affected projects where found for the current changes` and exits with `166`. Since no `affected.proj` is written,
running `dotnet build affected.proj` afterwards would fail on a missing file — so guard it:

```bash
dotnet affected # ...other args
if [ "$?" -eq 0 ]; then
    dotnet build affected.proj
fi
```

:::caution
`166` means *nothing changed*, which is not the same as *nothing is affected*. If projects changed but no other project
depends on them, the exit code is `0` — those changed projects still need to be built.
:::

## GitHub Actions

With the [dotnet-affected action](https://github.com/leonardochaia/dotnet-affected-action), the same idea is expressed
as a step condition:

```yaml
- name: Install dependencies
  if: success() && steps.affected.outputs.affected != ''
  run: dotnet restore affected.proj
```
