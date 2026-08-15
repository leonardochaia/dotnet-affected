# Benchmarks

[BenchmarkDotNet](https://benchmarkdotnet.org/) suites for dotnet-affected, plus a phase profiler for
locating hotspots.

## What is measured

| Suite             | Covers                                                                              |
|-------------------|-------------------------------------------------------------------------------------|
| `MicroBenchmarks` | The affected detection algorithm only, against a pre-built `ProjectGraph`.            |
| `MacroBenchmarks` | The full `dotnet affected` invocation: discovery, MSBuild graph construction, git diff, output. |

Both seed a throwaway git repository containing a generated tree of `csproj` files, sized by the
`TotalProjects` parameter (500 and 1000), and both record allocations via `[MemoryDiagnoser]`.

## Running locally

```bash
dotnet run -c Release -f net10.0 --project benchmarks/dotnet-affected.Benchmarks -- --filter '*'
```

`--job short` trades precision for a much faster run, which is what CI uses. The macro suite at 1000
projects takes tens of seconds *per operation*, so a full default-job run is a long wait.

```bash
dotnet run -c Release -f net10.0 --project benchmarks/dotnet-affected.Benchmarks -- --filter '*Macro*' --job short
```

The project also has three non-BenchmarkDotNet modes, which do a single timed pass and print a
breakdown. Use these to find where time goes, not to measure how much:

```bash
dotnet run -c Release -f net10.0 --project benchmarks/dotnet-affected.Benchmarks -- profile 250 500 1000
dotnet run -c Release -f net10.0 --project benchmarks/dotnet-affected.Benchmarks -- graph 500
dotnet run -c Release -f net10.0 --project benchmarks/dotnet-affected.Benchmarks -- predictors 1000
```

## Continuous benchmarking

[`.github/workflows/benchmark.yml`](../.github/workflows/benchmark.yml) runs the suites on every push
to `main`, and on demand via **Actions → Benchmarks → Run workflow** (which takes a `--filter` and a
`--job` so you can run a single suite without editing the workflow).

Every run:

1. Writes the results table to the workflow's job summary.
2. Uploads the full BenchmarkDotNet artifacts — JSON, CSV, HTML, markdown — as `benchmark-results`.
3. On `main` only, pushes two series to the `benchmark-data` branch via
   [github-action-benchmark](https://github.com/benchmark-action/github-action-benchmark), which
   compares them against the recorded history and comments on the commit when a threshold is
   crossed.

### The two series, and why they are separate

**Timings** land in `dev/bench/time` with a 200% alert threshold and do not fail the build. GitHub's
hosted runners are shared VMs with no guarantee about the underlying hardware, so absolute numbers
are not comparable between runs and a tight threshold produces nothing but false positives. Treat the
chart as a trend line: a real regression shows up as a step in the curve, not as a single spike.

**Allocated bytes** land in `dev/bench/allocations` with a 105% threshold and *do* fail the build.
Allocation counts are deterministic — the same code allocates the same bytes on any machine — so a 5%
move is a genuine change in behaviour and worth interrupting for. This is the series to watch.

The split exists because github-action-benchmark's BenchmarkDotNet adapter reads only
`Statistics.Mean`; it ignores `MemoryDiagnoser` output entirely. The workflow extracts allocations
from the same report with `jq` and feeds them in as a `customSmallerIsBetter` series.

### Where the results live

Results are stored on an orphan `benchmark-data` branch, which nothing serves. GitHub Pages allows
one source per repository and the documentation site owns it, so the chart pages that
github-action-benchmark generates alongside the data would never be reachable. They are ignored; the
data is what matters.

Each series is a single file — `dev/bench/time/data.js` and `dev/bench/allocations/data.js` — holding
the full run history as one assignment:

```js
window.BENCHMARK_DATA = { lastUpdate: ..., entries: { ... } }
```

That history is what the thresholds compare against, and it is also the input the documentation site
renders. A consumer has to strip the assignment prefix before parsing it as JSON.

### One-time setup

The branch has to exist before the first run:

```bash
git switch --orphan benchmark-data && git commit --allow-empty -m "chore: initialise benchmark-data" && git push -u origin benchmark-data
```

The workflow also needs **Settings → Actions → General → Workflow permissions** set to read *and
write*, so it can push results and comment on commits.

### Publishing to the documentation site

The [Starlight site](../docs) reads these files at build time, checking out `benchmark-data`
alongside the docs sources and rendering the series with its own components so the charts match the
site's theme.

There is deliberately no trigger from this workflow to the docs build. Benchmark figures are part of
what a release advertises, and releases already rebuild the documentation, so the chart is refreshed
on the cadence that matters. Between releases the published chart lags `main`, while the branch
itself stays current and keeps gating every merge.

### Cost

A full run is roughly 25–40 minutes on a hosted runner, dominated by the macro suite at 1000
projects. Runs are serialised through a `benchmarks` concurrency group so they cannot race each other
onto `benchmark-data`.

## Publishing numbers

If benchmark figures are ever quoted outside of CI — a README table, a blog post, a comparison —
take them from a run on a known idle machine and publish BenchmarkDotNet's environment header
alongside them (BenchmarkDotNet version, OS, CPU, SDK, runtime, job configuration). It is emitted at
the top of every report. A table without it cannot be reproduced or trusted, and CI numbers in
particular should never be presented as absolute.
