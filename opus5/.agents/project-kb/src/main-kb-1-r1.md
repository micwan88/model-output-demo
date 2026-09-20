---
date: 2026-09-19
id: 1
rev: 1
title: Default `dotnet` (SDK 10 snap) cannot run net8.0 tests on the build machine; use the SDK 8 snap host
author: main
area: Verification
tags: [dotnet, net8.0, snap, sdk, test-runtime, build-environment]
---

## Trigger
When you run `dotnet test` (or `dotnet run`) for a `net8.0` project on the Linux build machine and it fails with
"You must install or update .NET to run this application ... Framework: 'Microsoft.NETCore.App', version '8.0.0'"
and the only framework listed is `10.0.x at [/snap/dotnet-sdk-100/...]`, even though .NET 8 is "installed".

## Lesson
On this machine every .NET install is a separate snap: `dotnet-sdk-100` (SDK 10.0.112), `dotnet-sdk-80` (SDK 8.0.131, runtime 8.0.31)
and `dotnet-runtime-80`. `/usr/local/bin/dotnet` points at the SDK 10 snap. A dotnet host only searches its own
`shared/` folder, so the SDK 10 host **builds** `net8.0` projects fine (it downloads the 8.0 targeting pack) but cannot
**run** them. The .NET 8 runtime in the other snaps is invisible to it, and `dotnet --list-runtimes` shows only 10.0.

## Do this
Use the SDK 8 snap's host for anything that executes net8.0 code (tests, run, publish verification):
```
D8=/snap/dotnet-sdk-80/current/usr/lib/dotnet/dotnet
$D8 --list-runtimes          # expect Microsoft.NETCore.App 8.0.x
$D8 build TMDEmulator/TMDEmulator.sln -c Release
$D8 test  TMDEmulator/TMDEmulator.sln -c Release --collect:"XPlat Code Coverage"
```
The SDK 8 host also creates classic `.sln` files by default (SDK 10's `dotnet new sln` creates `.slnx`).

## Avoid
- Don't "fix" it with `DOTNET_ROLL_FORWARD=Major`. The tests would then run on .NET 10, not the net8.0 target runtime.
- Don't add a `global.json` pinning SDK 8. The default `dotnet` on PATH is SDK 10, and every plain `dotnet` command would then fail.

## Why / evidence
`dotnet test` with SDK 10.0.112 on story 1 aborted with "Framework: 'Microsoft.NETCore.App', version '8.0.0' (x64) ...
The following frameworks were found: 10.0.12 at [/snap/dotnet-sdk-100/36/usr/lib/dotnet/shared/Microsoft.NETCore.App]".
With `/snap/dotnet-sdk-80/current/usr/lib/dotnet/dotnet`, all 141 tests pass on runtime 8.0.31.
`ls -l /usr/local/bin/dotnet` → `/snap/dotnet-sdk-100/current/usr/bin/dotnet`.

## References
- `.agents/tasks/main-plan-1-r1.md` (Resolved Decisions Q2, Verification results)
- `.agents/tasks/log-1.md` (Q2)
