---
# RAW LESSON CAPTURE — filled in by the working agent. This is a source-of-truth record.
# This file is append only; never edit after
# You do NOT touch kb-index.md. This is KB agent's job.
date: 2026-09-20
id: 1
rev: 1
title: Dev box has only the .NET 10 SDK - scaffolding and running net8.0 projects needs three workarounds
author: main
area: Implementation
tags: [dotnet, net8, sdk, dotnet-new, sln, test, roll-forward]
---

## Trigger
When creating a new .NET project/solution or running `dotnet test` / the built app for this project (target framework must be `net8.0` per CLAUDE.md) on the development machine, and any of these appear:
- `dotnet new console -f net8.0` fails with `'net8.0' is not a valid value for -f. The possible values are: net10.0` (exit code 127)
- `dotnet new sln` produces `TMDEmulator.slnx` instead of `.sln`
- `dotnet test` / running the exe fails because no `Microsoft.NETCore.App 8.x` runtime is found

## Lesson
The machine only has the .NET 10 SDK and .NET 10 runtime (`dotnet --list-sdks` -> `10.0.x`; `--list-runtimes` -> `Microsoft.NETCore.App 10.0.x` only). The SDK can build `net8.0` (it downloads the 8.0 targeting/runtime packs from NuGet) but its `dotnet new` templates only offer `net10.0`, it defaults to the `.slnx` solution format, and the resulting `net8.0` binaries cannot run without either an 8.x runtime or roll-forward. Tests therefore execute on the .NET 10 runtime unless a .NET 8 runtime is installed - say so in reports.

## Do this
```bash
# scaffold: generate with the default template, then set net8.0 by hand in the .csproj
dotnet new sln -n X --format sln          # classic .sln (VS / older tools)
dotnet new console -n X -o src/X --no-restore
sed -i 's#net10.0#net8.0#' src/X/X.csproj # (and tests/*.csproj)

# build/test/run net8.0 output on the .NET 10 runtime - environment only, never in project files
export DOTNET_ROLL_FORWARD=LatestMajor
dotnet test X.sln

# release exe: runtime pack is fetched from NuGet, no local runtime needed
dotnet publish src/X/X.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Avoid
- Do not put `<RollForward>` in the csproj to make tests run: it would change behavior of the shipped exe/tests for everyone.
- Do not leave `net10.0` in the csproj files: it silently breaks the project requirement (.NET 8).

## Why / evidence
Observed 2026-09-20: `dotnet new console -f net8.0` -> "Invalid option(s): -f net8.0 ... net10.0 - Target net10.0"; SDK 10.0.112 at `/snap/dotnet-sdk-100`, runtime 10.0.12 only. With `DOTNET_ROLL_FORWARD=LatestMajor`, 161 tests built for `net8.0` ran and passed. `dotnet publish -r win-x64 --self-contained` produced a valid `MZ` PE `TMDEmulator.exe` (~67 MB).

## References
- `.agents/tasks/main-plan-1-r1.md` (Question/Issue I1)
- `.agents/tasks/log-1.md` (2026-09-20, I1 answer)
