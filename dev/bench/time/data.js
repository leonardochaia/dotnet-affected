window.BENCHMARK_DATA = {
  "lastUpdate": 1786851377383,
  "repoUrl": "https://github.com/leonardochaia/dotnet-affected",
  "entries": {
    "dotnet-affected (time)": [
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
        "date": 1786832587532,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 11051737468.333334,
            "unit": "ns",
            "range": "± 236905692.68368807"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 416345308,
            "unit": "ns",
            "range": "± 2984684.407244927"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 26573730619.666668,
            "unit": "ns",
            "range": "± 388311106.245779"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 863251521.6666666,
            "unit": "ns",
            "range": "± 5441294.664108381"
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
        "date": 1786851377027,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 13737915389,
            "unit": "ns",
            "range": "± 335015354.71674883"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 542041492,
            "unit": "ns",
            "range": "± 7264757.777153138"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 34492150038,
            "unit": "ns",
            "range": "± 1367786948.799004"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 1158387192,
            "unit": "ns",
            "range": "± 4142680.5720611624"
          }
        ]
      }
    ]
  }
}