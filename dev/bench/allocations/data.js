window.BENCHMARK_DATA = {
  "lastUpdate": 1787416102662,
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
          "id": "284ffb87294beaa3541661237176b3a334516ec1",
          "message": "ci: fix the release workflow (#178)\n\n* ci: update actions/checkout to v4\n\nv2 runs on the Node 16 runtime, which GitHub has retired. The release builds through\nbuild.yml, so both files needed it.\n\n* ci: pack releases in the Release configuration\n\nbuild.yml ran a bare dotnet build and then packed --configuration Debug, and the\nrelease job published exactly those artifacts. Every package on nuget.org from v2\nonwards is an unoptimised build with DEBUG defined.\n\nThe configuration is now an input, defaulted to Debug so pull request CI is unchanged,\nand release.yml passes Release. The bin/Debug/net10.0 paths in the tool smoke tests and\nthe artifact upload had to move with it.\n\nVerified locally, since no CI run has ever built this configuration: the solution\nbuilds clean in Release under /WarnAsError, and pack produces all four packages with\ntheir symbol packages.\n\n* ci: make affected detection run on the releases it gates\n\nThe gate never engaged on a tag. last-successful-commit-action was asked for the last\nsuccessful run of release.yml on branch ${{ github.ref_name }}, which on a tag push is\nthe tag. No run had ever been on that tag, the lookup returned nothing, Detect Affected\nwas skipped for want of a commit_hash, and the empty hash then satisfied the condition\nguarding the push. Every tagged release published by falling through the gate rather\nthan passing it. Only a manual dispatch from main ever ran the detection.\n\nThe range is now the previous release: the nearest tag reachable from the released\ncommit's parent, which is what a release compares against and needs nothing outside\ngit to work out. Restricted to v* so it agrees with MinVerTagPrefix about which tags\nare releases -- the repository carries tags that are not, and one of those chosen as\nthe starting point would be a wrong answer nothing would report. With no earlier tag,\nthe first release publishes.\n\nThis drops nrwl/last-successful-commit-action, archived since 2023 and declaring\nnode12, and replaces ::set-output with $GITHUB_OUTPUT.\n\nskip_affected is gone. It never worked either: it was read as\ngithub.event.inputs.skip_affected, which is always a string, and the string 'false' is\ntruthy in an expression, so both settings of the box skipped detection and deployed.\nDetection is not optional now.\n\nWhat gets published is unchanged. The detection is a gate, not a filter: if anything is\naffected all four packages are pushed, and they have to be, because every package is\nversioned from the same tag and DotnetAffected.Core depends on DotnetAffected.Abstractions\nat that exact version.\n\nVerified against the real history: v6.2.0 resolves its range to v6.1.0, v6.0.0 to\nv6.0.0-preview-1, and the root commit to nothing. Both gate outcomes were exercised\nagainst this repository -- exit 0 with changes, exit 166 without.",
          "timestamp": "2026-08-16T11:36:13-03:00",
          "tree_id": "ff9c848c7be2384bf27ad42148b3567bce1f5de2",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/284ffb87294beaa3541661237176b3a334516ec1"
        },
        "date": 1786891690396,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6393045904,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498691544,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14389001840,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997337864,
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
          "id": "699ebb47f75710db5c9b739703687c5995e70f78",
          "message": "ci: default CI to the Release configuration (#179)\n\n* ci: default CI to the Release configuration\n\nPull request CI built Debug while releases packed Release, which left the configuration\nnobody ships as the only one anybody tests. A failure that only appears under\noptimisation would have waited for a tag to show itself.\n\nThe codebase has no #if DEBUG, no Debug.Assert and no MSBuild condition on\nConfiguration, so the two differ only in optimisation and the DEBUG symbol, and nothing\nhere depends on either.\n\nThe input stays, so a caller that wants Debug can still ask for it.\n\n* fix(test): stop the Tasks pack racing the build that starts it\n\nDotnetAffected.Tasks.Tests packs the Tasks SDK during its own build, so the tests\nconsume it as a package rather than as build output. The Exec hardcodes -c Release, and\nuntil the CI default changed the outer build was Debug, so the two wrote to bin/Debug\nand bin/Release and never met.\n\nOnce both are Release they share an output directory, and the nested pack and the outer\nbuild write DotnetAffected.Abstractions at the same time:\n\n    error MSB4018: System.IO.IOException: The process cannot access the file\n    '.../DotnetAffected.Abstractions/bin/Release/net8.0/DotnetAffected.Abstractions.deps.json'\n    because it is being used by another process.\n\nBeing a race it is not reliable about it. It failed on ubuntu and passed on macOS and\nWindows in the same CI run, and passes locally.\n\n--artifacts-path gives the nested build its own bin and obj. It is a command line global\nproperty, so it covers Tasks and everything it references, which is what the collision\nwas actually about. The package still lands where the test project looks for it: -o is\nexplicit and is unaffected.\n\nVerified by deleting src/DotnetAffected.Abstractions/bin/Release and running the Exec's\ncommand on its own. It no longer recreates the directory -- the deps.json now lands in\nobj/tasks-pack -- and the nupkg is still written to the test project's bin. Release build\nis clean and all Tasks tests pass on all three target frameworks.\n\nThe comment at the top of this target already warned about locks and races. The\nconfiguration split had been quietly holding this one off.",
          "timestamp": "2026-08-16T11:53:51-03:00",
          "tree_id": "b00dffc21f420a93c52795b3b974594881f07b47",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/699ebb47f75710db5c9b739703687c5995e70f78"
        },
        "date": 1786892704684,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6395291808,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498717296,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14386221040,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997339568,
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
          "id": "cb53bef37ba8eda1214c95f381586f5763fa221e",
          "message": "chore: adds nuget package metadata (#181)\n\nNone of the four packages set `Description`, `PackageProjectUrl`,\n`PackageTags`, `PackageReadmeFile` or `Copyright`, so they arrive on\nnuget.org as a title and a version and `dotnet pack` warned about the\nmissing readme on every run:",
          "timestamp": "2026-08-16T14:40:24-03:00",
          "tree_id": "2e9474614a2186412f8c1c8738839742bd63c0e4",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/cb53bef37ba8eda1214c95f381586f5763fa221e"
        },
        "date": 1786902703073,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6394597024,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498716824,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14373330752,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997338072,
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
          "id": "db14449f0daf36747c0ef018c958964fd31155b4",
          "message": "docs: add the dotnet-affected.com documentation site (#180)\n\nReplaces the README as the home for user-facing documentation with an\n[Astro Starlight](https://starlight.astro.build) site, published to\n`dotnet-affected.com`.\n\n## Why\n\nDocumentation lived in a 495 line README plus a second one for the\nMSBuild SDK, and both had drifted from the code: `--filter-file-path`\nwas undocumented, the pasted `--help` output predated `--exclude`,\n`--filter-file-path` and the `json` formatter, install instructions said\n`dotnet install`, and the SDK README claimed TFMs the packages no longer\nship.\n\nPages here are written from the source. The `--help` block is\nregenerated by running the tool, and every sample output comes from a\nreal invocation rather than being edited by hand.\n\n## What is in the site\n\n- **Getting started** — installation, quick start, how it works,\nincluding the architecture and the pipeline as mermaid diagrams.\n- **Guides** — choosing what to compare, build and test, project\ndiscovery, output formats, excluding projects, assumed changes, NuGet\npackage changes.\n- **Continuous integration** and a dedicated **GitHub Action** section\ncovering the action's inputs, the CLI flags they map to, its exit code\nhandling and the versioning rule that ties the action major to the tool\nmajor.\n- **MSBuild SDK** — overview, filtering by project properties,\nreference, troubleshooting. Folds in\n`src/DotnetAffected.Tasks/README.md`.\n- **Reference** — every CLI option and exit code.\n- **Upgrading from v6 to v7** — the working tree comparison,\n`.gitignore` aware discovery, the two exclusion options,\n`--assume-changes` failing on unknown projects, and the library API\nchanges.\n\n## Publishing\n\n`.github/workflows/docs.yml` builds on pull requests and publishes from\n`main`. The build fails on broken internal links, so a bad cross\nreference cannot reach the site.\n\nTwo manual steps before the first deploy succeeds:\n\n1. DNS for `dotnet-affected.com`: `A` records to the GitHub Pages\naddresses, or `ALIAS`/`ANAME` to `leonardochaia.github.io`.\n2. **Settings → Pages → Source** set to **GitHub Actions**. The `CNAME`\nfile alone is not enough.\n\n## Also here\n\nThe README is now 82 lines: what the tool is, how to install and run it,\nand links to the site.\n\n## Working on the site\n\n`docs/README.md` documents a container based workflow, so contributors\ndo not need Node installed.\n\n## Known gaps\n\n- The site tells people to use\n`leonardochaia/dotnet-affected-action@v7`, which is not tagged yet. The\naction release needs to land before or with this.\n- The CLI's `--help` still points at the README rather than the site.\n- Benchmarks are not published on the site yet.",
          "timestamp": "2026-08-16T22:06:04-03:00",
          "tree_id": "061f855c7c0de88a005cbad29baaa8d22b7c60eb",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/db14449f0daf36747c0ef018c958964fd31155b4"
        },
        "date": 1786929381189,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6370544672,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498716656,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14422708728,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997339168,
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
          "id": "cab3bddefe8c52ad39f9e3f9da6a72e97182ada8",
          "message": "docs: add Umami Cloud analytics to the docs site (#182)\n\nAdds the Umami Cloud tracking script to dotnet-affected.com.\n\nInjected on every Starlight page via the `head` option in\n`docs/astro.config.mjs`:\n\n```html\n<script defer src=\"https://cloud.umami.is/script.js\" data-website-id=\"04603e4d-e6d4-43f8-a2d5-da4072c3e72f\"></script>\n```\n\nNote: the script also loads under `astro dev` / `astro preview`, so\nlocal page views will land in the same Umami site. If that's not wanted,\n`data-domains=\"dotnet-affected.com\"` can be added to restrict reporting\nto the production host.",
          "timestamp": "2026-08-16T22:48:04-03:00",
          "tree_id": "b4f57afaae81bfd5c726e9636b7f292baa6808e1",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/cab3bddefe8c52ad39f9e3f9da6a72e97182ada8"
        },
        "date": 1786931944849,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6365104912,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498691760,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14384131552,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997338320,
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
          "id": "cf8fd997c6e3b633d16dc6edc467013fcd610378",
          "message": "ci: publish to NuGet via trusted publishing (#183)\n\nSwaps the long-lived `NUGET_API_KEY` secret in the release workflow for\n[NuGet trusted\npublishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing#github-actions-setup):\nthe job requests an OIDC token from GitHub, `NuGet/login` exchanges it\nwith nuget.org for an API key good for one hour and a single push, and\nthe push uses that.\n\n### Changes\n\n- `release` job gains `permissions: id-token: write`. Declaring any\npermission drops the rest to `none`, so `contents: read` is spelled out\ntoo, otherwise `actions/checkout` breaks.\n- A `NuGet/login@v1` step sits immediately before the push, behind the\nsame `should_deploy` gate. Placement is deliberate: the key expires in\nan hour, so requesting it at the top of the job would risk it going\nstale behind the affected-detection and artifact-download steps.",
          "timestamp": "2026-08-16T22:54:14-03:00",
          "tree_id": "40bf00b098f82145d6f10e29d8a68780464051fe",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/cf8fd997c6e3b633d16dc6edc467013fcd610378"
        },
        "date": 1786932599902,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6392211728,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498691320,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14384961376,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997287064,
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
          "id": "40f1c1a91e70620754aa612a860f03b7f65eb095",
          "message": "docs: publish the v6 to v7 performance comparison (#184)\n\nPublishes the v6.2.0 to v7 comparison as a Performance page, and\nsurfaces its headline figure where people decide whether to upgrade.\n\n## The data\n\nOne JSON snapshot per release under `docs/src/data/benchmarks/`, named\nafter the release — `v7.0.0.json` is the generator's output, unedited.\nEvery figure the page shows is computed from it at build time, so\nnothing is transcribed twice, and the release label comes from the file\nname rather than a field, which keeps the snapshot exactly as the run\nproduced it.\n\nSnapshots are never regenerated. A published comparison that changes\nunderneath a reader is not a comparison; a new release adds a file next\nto the old ones.\n\n## The page\n\n`/performance/`: headline figure, two charts, the numbers as a table,\nthe scaling exponents, and the method.\n\nBoth charts render to SVG at build time, so the page ships no client\nJavaScript.\n\n- **Speedup bars are zero based.** Bars encode length, and anchoring\nthem at 1x turned a 4.9x difference in the data into a 17x difference on\nscreen. A dashed reference line marks 1x, where nothing changed.\n- **Wall clock is a dumbbell on a log axis.** Position encoding, so\nthere is no zero to misrepresent, and the sizes span two orders of\nmagnitude — 5.9s to 727s on one linear axis leaves the two smallest\nsizes invisible.\n- **Colour is one hue at two steps**, the before/after pair for a\ndumbbell, validated against both site surfaces (`#ffffff`, `#17181c`) in\nboth modes. The speedup chart is a single series and carries no legend.\n- Hover text on every mark via SVG `<title>`, and the table view carries\nthe exact numbers with the spread behind each mean.\n\n## What the numbers say\n\n| Projects | v6.2.0 | v7 | Speedup |\n|---|---|---|---|\n| 250 | 7.80s | 5.90s | 1.3x |\n| 500 | 17.1s | 10.9s | 1.6x |\n| 1,000 | 79.9s | 28.1s | 2.8x |\n| 2,000 | 727s | 111s | 6.5x |\n\nBoth versions reported the same affected projects at every size, so this\nis not one of them doing less work.\n\nThe page states plainly that these are desktop numbers, comparable to\neach other and nothing else, and that part of the gain is discovery no\nlonger walking `.git` or gitignored paths rather than the graph work\nalone — the BenchmarkDotNet suites on `main` isolate that, and the page\nlinks to them.\n\n## Where it shows up\n\n- **Landing page**: a release callout reading the same snapshot, so the\nfigure follows the data, plus a Performance card in the grid.\n- **Upgrade guide**: a *Why upgrade* section leading with the table,\nabove the behaviour changes.\n- **Sidebar**: under Reference.\n\n## Adding the next release\n\nDrop `v7.1.0.json` into `docs/src/data/benchmarks/`. The page picks up\nthe newest by version order, the landing callout follows it, and older\nsnapshots are listed under Data.",
          "timestamp": "2026-08-17T16:34:24-03:00",
          "tree_id": "9b462cfb7350302fe852aa9205eaea75f8f4319f",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/40f1c1a91e70620754aa612a860f03b7f65eb095"
        },
        "date": 1786995872220,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6390912800,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498716824,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14380801600,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997338032,
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
          "id": "2322615cf5a76a2833ad1cc7983926851063ea0d",
          "message": "fix: handle file moving between projects correctly (#186)\n\n## The bug\n\n`GitChangesProvider` asks libgit2 for a diff and takes `change.Path`\nfrom every entry. Rename detection is on by default, so a file moved\nfrom one project to another comes back as **one** entry holding only the\nnew path:\n\n```\n--- default options\n    Renamed    path=projB/Moved.cs oldPath=projA/Moved.cs\n--- Similarity = None\n    Deleted    path=projA/Moved.cs\n    Added      path=projB/Moved.cs\n```\n\nTwo consequences for a move across projects:\n\n- the project the file left is never reported as changed, even though\nits compiled set shrank;\n- the old path never reaches `DeletedFilesOverlayFileSystem`, so the\nfile is not restored for evaluation.\n\nIt is also the expensive part of the diff — `git_diff_find_similar`\ncompares the content of every addition against every deletion. Moving\n3000 files:\n\n```\ndefault (rename detection on)   1.747s   entries=4999\nSimilarity = None               0.001s   entries=6000\n```\n\nNote `entries=4999` vs `6000`: git's rename limit kicks in, so the\ncurrent behaviour is not even consistent at scale.\n\n## The fix\n\nRename identity is of no use to this tool — both paths are changes — so\nboth `Diff.Compare` call sites now pass `Similarity =\nSimilarityOptions.None`.\n\n## Tests\n\n`MovedFileChangeDetectionTests` covers a file moved between two\nprojects, once committed (tree-to-tree diff) and once staged\n(working-tree diff), asserting both projects are reported as changed.\nBoth fail on `main` (1 changed file, 1 changed project) and pass with\nthe fix.\n\nFull suite green: Core 135, CLI 57, Tasks 4.",
          "timestamp": "2026-08-21T23:27:26-03:00",
          "tree_id": "e20a02e05f538a2403630510aa3909730202e67f",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/2322615cf5a76a2833ad1cc7983926851063ea0d"
        },
        "date": 1787366304841,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6395598696,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498717032,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14379140472,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 997338224,
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
          "id": "a55daad6c0efe05dd4c58b6c5d03c189dca6ef2b",
          "message": "perf: match predicted inputs as they are collected (#187)\n\n## The problem\n\n`PredictionChangedProjectsProvider` collects every file the predictors\nreport as an input, keyed by\ngraph node, and only then searches that index once per changed file:\n\n```csharp\nforeach (var file in normalizedFiles)\n    collector.PredictionsPerNode.Where(x => x.Value.Contains(file))\n```\n\nTwo costs. The index is held live for the whole of matching — one\n`HashSet<string>` per node holding\nevery predicted input under the repository. And the search is a lookup\nper node per changed file,\nwhich grows as the product of the size of the repository and the size of\nthe change: on a 4000 node\ngraph a 800 file change is 3.2M lookups, each allocating a fresh LINQ\nenumerator.\n\nThe set of changed files is known before prediction starts, so none of\nit is necessary.\n\n## The fix\n\n`FilesByProjectGraphCollector` now takes the changed files and matches\neach predicted input as it is\nreported, keeping only the nodes that hit. `PredictionsPerNode` is gone,\nthe search loop is gone, and\n`ProjectInputFilesCollector` — whose only job was the per-node set — is\ndeleted.\n\nThe checks in `AddInputFile` are ordered cheapest first, which turned\nout to matter more than the\nmatching itself. Most of what the predictors report is the SDK's own\nprops and targets: rooted, and\noutside the repository. Those are rejected by the existing\n`StartsWith(_repositoryPath)` test without\ntouching the string. Only what survives that pays for a\n`Path.GetFileName` compared against the\nchanged file names, and only what survives *that* pays for\n`Path.Combine` and `Path.GetFullPath` —\nwhich the old code ran on every input it kept.\n\nPutting the name check first instead, which is the obvious reading,\ncosts a substring allocation for\nevery SDK targets file and measured 3.6% *more* allocation than `main`\nat both sizes.\n\n## Results are the same\n\nTwo things the old loop did by accident and the new shape has to do on\npurpose:\n\n- It deduplicated by `ProjectInstance.FullPath`, not by node, so a multi\ntargeted project yielded one\nnode rather than one per inner build. The result now goes through\n`Deduplicate()` for that.\n- Its order came from the changed files. Matches collected across\nparallel predictors have no order,\nso the result walks `Graph.ProjectNodes` and filters, which is\ndeterministic.\n\nIt also removes a data race. `ProjectGraphPredictionExecutor` walks the\ngraph nodes in parallel *and*\nruns each project's predictors in parallel, all reporting into one\ncollector, so several predictors\nfor the same `ProjectInstance` were calling `Add` on a plain\n`HashSet<string>` concurrently. The\nmatches are now the only shared mutable state and they are behind a\nlock; everything else is built in\nthe constructor and only read.\n\n## Measurements\n\n16 logical / 8 physical cores net 10\n\n`MicroBenchmarks`, run back to back on an otherwise idle machine,\n`--warmupCount 5 --iterationCount 15`:\n\n| TotalProjects | | Mean | Allocated | Gen1 |\n|---|---|---|---|---|\n| 500 | before | 277.7 ms ± 2.71 | 475.59 MB | 1000 |\n| 500 | after | 258.5 ms ± 26.36 | 475.25 MB | 1000 |\n| 1000 | before | 582.1 ms ± 61.36 | 951.14 MB | 2000 |\n| 1000 | after | **432.8 ms ± 12.96** | 950.42 MB | **1000** |\n\nThe phase profiler, which isolates attribution from the graph build:\n\n| | before | after |\n|---|---|---|\n| 500 projects / 2000 nodes, attribute 400 files | 0.21s | **0.18s** |\n| 1000 projects / 4000 nodes, attribute 800 files | 0.51s | **0.33s** |\n\nThe profiler also runs the same call with a single changed file, which\npays for prediction but\nbarely for matching. At 1000 projects that goes 0.43s → 0.31s, so\nroughly two thirds of the saving\nis the cheaper collection and one third is the search loop no longer\nrunning.\n\nA note on the memory column, because it is easy to misread:\n`PhaseProfiler.Measure` reports\n`GC.GetTotalAllocatedBytes`, which is cumulative allocation over the\nphase, not live heap. It sits at\n~937 MB before and after, because the overwhelming majority of it is\nchurn inside MSBuildPrediction's\nitem expansion rather than the index this change removes. What moved is\nGen1 collections per 1000\noperations, halved at 1000 projects — fewer objects surviving gen0,\nwhich is the index no longer\nbeing held live.\n\n## Tests\n\nFull suite green on net8.0, net9.0 and net10.0: Core 135, CLI 57, Tasks\n4. Every generated test\nproject is multi targeted, so the deduplication path above is covered\nthroughout rather than by one\ncase.",
          "timestamp": "2026-08-22T13:17:29-03:00",
          "tree_id": "c772c0856ad61e17a34e896c80c1fae13cfabef6",
          "url": "https://github.com/leonardochaia/dotnet-affected/commit/a55daad6c0efe05dd4c58b6c5d03c189dca6ef2b"
        },
        "date": 1787416102349,
        "tool": "customSmallerIsBetter",
        "benches": [
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 6394166392,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 500, ChildrenPerProject: 20)",
            "value": 498328456,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MacroBenchmarks.MacroBenchmark(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 14376637952,
            "unit": "bytes"
          },
          {
            "name": "Affected.Cli.Benchmarks.MicroBenchmarks.AffectedAlgorithm(TotalProjects: 1000, ChildrenPerProject: 20)",
            "value": 996586976,
            "unit": "bytes"
          }
        ]
      }
    ]
  }
}