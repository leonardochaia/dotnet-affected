window.BENCHMARK_DATA = {
  "lastUpdate": 1786890422232,
  "repoUrl": "https://github.com/leonardochaia/dotnet-affected",
  "entries": {
    "dotnet-affected (allocations)": [
      {
        "commit": {
          "author": {
            "email": "leonardochaia@users.noreply.github.com",
            "name": "Leonardo Chaia",
            "username": "leonardochaia"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "bd7c5187c334d6511011c0c3609d58e1cddd62ee",
          "message": "ci: run and publish benchmarks from GitHub Actions (#175)\n\nAdds a workflow that runs the BenchmarkDotNet suites on every push to main\nand on demand, writes the results table to the job summary, and uploads the\nfull artifacts.\n\nOn main it also records two series on an orphan benchmark-data branch via\ngithub-action-benchmark. They are split because the thresholds have to be\ndifferent: wall clock gets 200% and does not fail the build, since hosted\nrunners are shared VMs and anything tighter is all false positives, while\nallocated bytes get 105% and do fail. Two runs of the suites moved\nallocations by at most 0.24%, so a 5% gate sits well outside the noise.\n\nThe allocations series exists at all because github-action-benchmark's\nBenchmarkDotNet adapter reads only Statistics.Mean and ignores\nMemoryDiagnoser, so the numbers are extracted from the same report with jq\nand fed in as a custom series.\n\nNothing serves benchmark-data. GitHub Pages allows one source per\nrepository and the documentation site will own it, so the charts the action\ngenerates alongside the data are ignored; the docs build renders the series\nitself at release time.",
          "timestamp": "2026-08-15T19:13:44-03:00",
          "tree_id": "66159d4267ffd895e9eb06e575305f2344cd593d",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/bd7c5187c334d6511011c0c3609d58e1cddd62ee"
        },
        "date": 1786832589632,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6392855832,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498668864,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14420272704,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997238392,
            "unit": "bytes"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "leonardochaia@users.noreply.github.com",
            "name": "Leonardo Chaia",
            "username": "leonardochaia"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "fed689178accae52eda720d8b152c2bbf699ce86",
          "message": "feat!: build every comparison from the working tree (#176)\n\n* feat!: build every comparison from the working tree\n\nThe project graph is always built from the working tree, so --to could\nname a revision whose project structure was never analysed. A project\nadded between the two ends was counted among the files that changed and\nthen reported under no project, silently, with exit code 0.\n\n--to is now validated to name the commit the working tree is checked out\nat, and refused otherwise. It is deprecated and will be removed in v8.\n\n--uncommitted <all|staged|none> replaces it as the way to choose what the\nworking tree contributes, which also covers running inside a pre-commit\nhook.\n\nDirectory.Packages.props is now read from the same two revisions the file\ndiff uses. It previously treated --from as the new side and HEAD as the\nold, which inverted the reported versions and missed uncommitted version\nbumps entirely.\n\nProject files that changed while belonging to no project in the graph are\nnow reported as warnings, naming whether --exclude-discovery, the filter\nfile or gitignore kept them out.\n\nBREAKING CHANGE: --from now includes uncommitted changes, pass\n--uncommitted none for the previous behaviour. --to no longer accepts a\nrevision other than the one checked out. IChangesProvider.GetChangedFiles\ntakes an UncommittedChanges instead of a to ref.\n\n* docs: recommend --uncommitted none for CI\n\nA result that depends only on the commits cannot be changed by a step\nthat writes to a tracked file before dotnet-affected runs, such as code\ngeneration or a version stamp.\n\n* test: normalize separators when matching changed file paths\n\nChanged files keep the separators git reports, so on Windows they read\nas C:\\repo\\Project/Project.csproj while MSBuild's FullPath is fully\nbackslashed. The production matching already normalizes first.",
          "timestamp": "2026-08-16T00:24:43-03:00",
          "tree_id": "3d9b12c68785c32ed0e36554a9acaaf9bc2ecc41",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/fed689178accae52eda720d8b152c2bbf699ce86"
        },
        "date": 1786851378553,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6369596504,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498692088,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14420948176,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997338192,
            "unit": "bytes"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "leonardochaia@users.noreply.github.com",
            "name": "Leonardo Chaia",
            "username": "leonardochaia"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "ba0ba830ed83ecce1d407901c6ab5395f2c5d9c8",
          "message": "chore: remove version.json (#177)\n\nNothing reads it. Versioning is MinVer's, configured by MinVerTagPrefix in\nDirectory.Build.props and the package reference in package-defaults.props, and it\nderives every package version from the git tags. There is no Nerdbank.GitVersioning\nreference anywhere in the repository for version.json to configure.\n\nIt has claimed 2.2.0-preview since 2021 while four majors shipped around it, which is\nthe clearest evidence available that it never mattered. Removing it leaves the\ncomputed version untouched: 6.2.1-preview.0.19 before and after, the v6.2.0 tag plus\nthe commits since.\n\nLeft in place it is a second place to look for the version and the obvious thing to\nedit when cutting v7, where editing it would do nothing at all.",
          "timestamp": "2026-08-16T11:15:54-03:00",
          "tree_id": "c313f4638186192aef41513547c2094b49c462c6",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/ba0ba830ed83ecce1d407901c6ab5395f2c5d9c8"
        },
        "date": 1786890421918,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6380180088,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498717088,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 13967433392,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997337992,
            "unit": "bytes"
          }
        ]
      }
    ]
  }
}