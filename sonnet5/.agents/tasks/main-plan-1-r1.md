---
date: 2026-09-20
id: 1
rev: 1
status: Completed
author: main
description: work plan for story-1-r1 - TMD Emulator main program (project skeleton, rich terminal UI, Export MZMK Initialization Key)
---

## Question/Issue

- None

Resolved (answers from user in `log-1.md`, 2026-09-20; plan approved explicitly by user):
- **Q1 curve/keypair:** curve is whatever the hardcoded PKCS#8 keypair contains (read from the key, not hardcoded). When I generate the dev keypair, use **secp256r1** (P-256).
- **Q2 "HEX":** the public key HEX is the **`04||X||Y` uncompressed point, upper case**. My reading, applied consistently: the same point HEX is shown on screen and written to the `.der` file; the 8-char fingerprint is still SHA-256 over the **SPKI DER** bytes as the story states (upper-case hex, first 8 chars). Private scalar HEX also upper case.
- **I1 .NET 8 runtime:** run with `DOTNET_ROLL_FORWARD=LatestMajor` (environment only, not in project files); the report will state tests ran on the .NET 10 runtime.

## Assumptions

Scope/interpretation (challenge any of these):
1. This story delivers only the emulator shell, config, hardcoded keypair, and the **Export MZMK Initialization Key** function. **View Keys**, **Finalize Remote MZMK**, **Uninstalling a MZMK** are placeholder screens. The objective "save the key value in clear in file" is treated as background for later stories; nothing besides the exported public-key file is persisted here.
2. Layout: `TMDEmulator/TMDEmulator.sln`, app in `TMDEmulator/src/TMDEmulator/`, tests in `TMDEmulator/tests/TMDEmulator.Tests/`. "Visual studio project files" = `.sln` + SDK-style `.csproj` (buildable by `dotnet build`, openable in VS).
3. **No third-party runtime library.** The UI is a small custom renderer on `System.Console` (16 `ConsoleColor`s), and crypto uses in-box .NET (`ECDsa.ImportFromPem`, `ExportSubjectPublicKeyInfo`, `SHA256`, `System.Text.Json`). Rationale: no VT/P-Invoke setup needed for Windows, no dependency to vet in a crypto-adjacent project, and the required UI (two frames, arrow-key menu, one text prompt, one Yes/No) is small. Only test packages are added: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`. Alternative if you prefer: Spectre.Console (heavier, and its Layout does not natively do interactive prompts inside a frame).
4. "Grey-to-white highlight with black text" = solid light-grey (`Gray`) row background with black text (a gradient is not possible with a console palette). Default theme: background `Black`, text `White`, highlight `Gray` on `Black`, errors `Red`.
5. Config: `tmdemulator.json` beside the exe (copied to output), colors given by `ConsoleColor` name. Missing file => built-in defaults. Malformed file or unknown color name => fall back to defaults for the affected values and show the problem in RED in the status bar at startup.
6. Menu behaviour: Up/Down wraps around; Esc = same as the "Back" item; Esc at the top-level menu does nothing (leave via `Exit`). `Ctrl+C` restores console colors/cursor before exiting.
7. Screen layout: **Main frame** (title/breadcrumb + content) above a **Bottom frame** (status bar) whose first line is the key legend (`↑/↓ Navigate  Enter Select  Esc Back`) and whose following line(s) show validation/errors in RED. Terminal smaller than a minimum size shows a "terminal too small" message instead of corrupting the layout. Redraw on every key and when the window size changes.
8. Export path prompt: pre-filled with the current directory (absolute), editable. Relative input is resolved to an absolute path. **If the folder does not exist, that is a validation error (RED); the folder is not created silently.** Empty input, illegal path characters, and write failures (permission, etc.) are also shown in RED and the prompt stays open. Esc cancels back to the menu.
9. File name: `MZMK_Init_{FP8}{yyyyMMddHHmmss}.der` exactly as written (no separator between fingerprint and timestamp), timestamp in **local time**. Content = upper-case hex text of the `04||X||Y` point (130 hex chars for P-256), ASCII, no BOM, no trailing newline. If the file exists, a Yes/No prompt is shown (Esc = No); No returns to the path prompt/menu without writing.
10. Publish: primary deliverable is a self-contained single-file `win-x64` `TMDEmulator.exe`. Code is kept cross-platform (CLAUDE.md lists Windows 11 / Server 2025 / Linux); a `linux-x64` publish is also produced for smoke-testing here.
11. Unit-test target: every class except the thin `ConsoleTerminal` adapter (raw `System.Console` calls) is covered; everything else is driven through a `FakeTerminal` that records a character/color grid so colors and layout can be asserted.

## Design Summary

```
TMDEmulator/
├── TMDEmulator.sln
├── src/TMDEmulator/
│   ├── TMDEmulator.csproj            net8.0, Exe, AssemblyName=TMDEmulator, Nullable on
│   ├── tmdemulator.json              default theme (CopyToOutputDirectory)
│   ├── Program.cs                    composition root only
│   ├── Config/    AppConfig, ConfigLoader           (json -> Theme + load errors)
│   ├── Terminal/  ITerminal, ConsoleTerminal, Theme (ConsoleColor set)
│   ├── Ui/        ScreenHost (loop + screen stack + frame layout + status bar),
│   │              IScreen, MenuScreen, PlaceholderScreen, ExportInitKeyScreen,
│   │              LineEditor (text input), MenuDefinition (the menu tree)
│   ├── Keys/      TestKeyPair (const PKCS#8 PEM, TEST ONLY), EcKeyInspector
│   │              (curve, D hex, point hex, fingerprint)
│   └── Export/    MzmkInitExporter (file name + write + exists check; TimeProvider injected)
└── tests/TMDEmulator.Tests/          xunit; FakeTerminal, FakeTimeProvider (tiny subclass)
```

Key rules: screens are state machines fed `ConsoleKeyInfo`s and drawing via `ITerminal`; the crypto/export logic has no UI dependency; the clock is injected via .NET's built-in `TimeProvider`.

## Tasks

### A. Scaffolding
- [x] A1. Create `TMDEmulator/` with `.sln`, console project, xunit test project, project reference, both added to the sln
- [x] A2. Set `net8.0`, `Nullable`, `ImplicitUsings`, `AssemblyName=TMDEmulator`; copy `tmdemulator.json` to output
- [x] A3. Goal: `dotnet build TMDEmulator/TMDEmulator.sln` succeeds with **0 warnings, 0 errors**

### B. Keys and export logic (no UI)
- [x] B1. Generate a P-256 (or Q1 answer) keypair with `openssl`; write the PKCS#8 PEM into `TestKeyPair` with a TEST-ONLY comment
- [x] B2. `EcKeyInspector`: load PEM -> curve name, private scalar `D` hex, public point `04||X||Y` hex, SPKI DER bytes, 8-char fingerprint (SHA-256 of SPKI DER)
- [x] B3. `MzmkInitExporter`: build file name from fingerprint + injected clock; validate folder; detect existing file; write public point hex as ASCII (no BOM/newline)
- [x] B4. Goal: fingerprint, point hex and `D` match values computed independently by `openssl` (see Verification V3)

### C. Terminal and framework
- [x] C1. `Theme` + `AppConfig` + `ConfigLoader` (defaults, overrides, error reporting per Assumption 5)
- [x] C2. `ITerminal` + `ConsoleTerminal` (size, key read, positioned colored write, clear, cursor hide/show, restore on exit)
- [x] C3. `ScreenHost`: two-frame layout, legend line, red message line(s), screen stack (push/pop), resize + too-small handling, key loop, `Ctrl+C` restore
- [x] C4. `MenuScreen`: arrow-key navigation with wrap, grey-row/black-text highlight, Enter select, Esc back
- [x] C5. `LineEditor`: printable chars, Backspace/Delete, Left/Right/Home/End, pre-filled editable default

### D. Screens
- [x] D1. `MenuDefinition` tree exactly as in the story: 1 Remote MZMK Setup (Export MZMK Initialization Key / Finalize Remote MZMK / Uninstalling a MZMK / Back), 2 Key Management (View Keys / Back), 3 Exit
- [x] D2. `PlaceholderScreen` for View Keys, Finalize Remote MZMK, Uninstalling a MZMK (Esc back)
- [x] D3. `ExportInitKeyScreen`: show Curve / Private key (HEX scalar) / Public key (HEX); path prompt (default = current absolute directory); validation in RED under legend; overwrite Yes/No; success message; return to menu after saving
- [x] D4. `Program.cs` wires config -> theme -> host -> root menu (first screen at startup = level-1 menu)

### E. Unit tests (written alongside B-D, not after)
- [x] E1. `ConfigLoader`: missing file, valid override, unknown color, malformed JSON
- [x] E2. `EcKeyInspector` with a known-answer vector pinned from `openssl` output
- [x] E3. `MzmkInitExporter`: file name format with fixed clock, content, exists/overwrite paths, missing folder, unwritable target
- [x] E4. `LineEditor` editing behaviours
- [x] E5. `MenuScreen` / `ScreenHost` via `FakeTerminal`: initial screen, navigation + wrap, highlight colors, Enter/Esc, Exit, placeholders, legend text, too-small terminal
- [x] E6. Export flow end to end via `FakeTerminal`: happy path, empty/invalid/nonexistent path shows RED message under legend, overwrite Yes and No, Esc cancel

### F. Deliverable
- [x] F1. `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` -> `TMDEmulator/publish/win-x64/TMDEmulator.exe` (+ `tmdemulator.json`); valid PE (`MZ`) header confirmed, NOT executed (no Windows here)
- [x] F2. `dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true` -> `TMDEmulator/publish/linux-x64/TMDEmulator`, used for the smoke test

## Implementation Notes (deviations / refinements from the sketch above)

- Extra small helper classes not in the sketch: `ScreenBuffer` (off-screen colored grid; only changed cells are flushed, so no flicker and tests can assert colors), `Canvas` (clipped, styled drawing region), `SelectionList` (shared by menus and the Yes/No prompt), `TextUtil` (wrap / ellipsis). `AppConfig` is a record returned by `ConfigLoader`.
- Status frame has **two** wrapped message lines under the legend (not one): the "Saved: <full path>" and "Folder does not exist: <path>" messages exceed one line on an 80-column terminal. Longer than two lines ends with an ellipsis.
- Minimum terminal size is 60x22; below that a red "Terminal too small" message is shown and keys are ignored (except Ctrl+C). The rightmost column is deliberately unused (writing the last cell scrolls consoles).
- `Ctrl+C` is read as a key (`TreatControlCAsInput`) and exits from any screen; the terminal is restored in a `finally`.
- The story targets .NET 8 but `dotnet new` on the installed .NET 10 SDK only offers `net10.0`; the templates were generated and `TargetFramework` was set to `net8.0` by hand in both csproj files.
- `dotnet new sln` on SDK 10 defaults to `.slnx`; `--format sln` was used for a classic `.sln` (opens in older VS / tools).
- Menu labels are shown without the outline numbers of the story (`1.`, `2.` ...), which are read as structure, not text.

## Verification Plan (Stage 3)

| # | Check | Pass condition |
|---|-------|----------------|
| V1 | `dotnet build` (Release) | 0 errors, 0 warnings |
| V2 | `dotnet test` (with coverlet) | all pass; coverage reported honestly, `ConsoleTerminal` the only intended gap |
| V3 | Independent crypto cross-check with `openssl pkey -pubout -outform DER \| sha256sum` and `openssl pkey -text` | fingerprint, point hex, private scalar, curve all equal the program's output |
| V4 | `dotnet publish` win-x64 and linux-x64 | `TMDEmulator.exe` produced; Linux build starts |
| V5 | Smoke run of the Linux build in a pseudo-terminal (tmux/`script`, if available) with scripted keys | menu shows at startup, arrows navigate, Enter/Esc work, exported `.der` file has expected name and hex content |
| V6 | Walk through every bullet of the story against a checklist | each bullet marked met / not met with evidence |

Limits I already know about (will be stated in the report, not hidden):
- I **cannot run the Windows `.exe` on this Linux machine**, so Windows 11 rendering/keys are not verified by me; I will hand you a short manual smoke checklist for Win 11.
- Tests run on .NET 10 runtime unless I1 is resolved by installing .NET 8.

## Verification Results (Stage 3, 2026-09-20)

| # | Result |
|---|--------|
| V1 | PASS - clean Release build (`--no-incremental`): 0 errors, 0 warnings |
| V2 | PASS - 161/161 tests. Line coverage 92.8% overall (452/487); 452/453 = 99.8% excluding `Program` and `ConsoleTerminal` (0% by design, smoke-tested). Only miss: last-resort `"unknown"` curve-name fallback in `EcKeyInspector.DescribeCurve` (not reachable on Linux) |
| V3 | PASS - file exported by the real program vs `openssl`: public point, private scalar, fingerprint `11D3BE29`, curve prime256v1 all equal |
| V4 | PASS - both publishes succeed; `TMDEmulator.exe` starts with `MZ` |
| V5 | PASS - 40/40 checks on the final published Linux binary in a pseudo-terminal (pyte): menus, colors, navigation, export, red validation under legend, placeholders, Exit item, Ctrl+C, resize, too-small, bad config, redirected stdin refused |
| V6 | PASS with caveat - every story bullet met; "runnable on Win 11" is built but NOT run by me |
| extra | 17/17 deliberate mutations of the product code were caught by the tests (run on a throwaway copy; real tree untouched) |

Issues found and fixed during implementation/verification (none outstanding):
- Test helper named `Keys` clashed with namespace `TMDEmulator.Keys` -> renamed `Press`.
- Single-line status message cut off the file name of "Saved: <path>" at 80 columns -> two wrapped message lines.
- Coverage showed 4 untested behaviours (Key Management "Back", stray key during overwrite question, unknown-curve name, explicit-parameter key) -> tests added; unused `MenuScreen.SelectedIndex` removed.
- One weak test (illegal-path flow test with an either/or assertion) and one mislabelled test were removed/rewritten rather than left as false comfort.
- A smoke-script failure on "Exit" was a script assumption (root menu keeps its highlight), not an app defect; confirmed by asserting the highlight before pressing Enter.

Open item for the user (not a blocker): file content of the `.der` is the `04||X||Y` point HEX (my reading of Q2 applied to the file too). If the real TMD expects SPKI-DER-as-HEX in the file, change `MzmkInitExporter.Write` only.

## Story Traceability

| Story requirement | Plan item |
|-------------------|-----------|
| New folder `TMDEmulator`, app project, test project, VS project files, 3rd-party libs declared | A1-A2, Assumption 3 |
| Built via dotnet cli | A3, V1, V4 |
| Rich terminal, arrow keys, Enter, Esc = Back, two frames + status bar | C2-C4, Assumptions 6-7 |
| Black bg / white text / grey highlight w/ black text / configurable | C1, C4, Assumptions 4-5 |
| Status bar legend; validation + errors in RED under legend | C3, D3 |
| Menu tree as specified; 1st-level menu at startup | D1, D4 |
| Constant class with hardcoded EC PKCS#8 PEM keypair | B1 |
| Export MZMK Init Key: show curve/private/public; path prompt w/ default; fingerprint; file name; hex saved as `.der`; overwrite prompt; back to menu | B2-B3, D3 |
| Placeholders for View Keys / Finalize / Uninstall | D2 |
| Exe runnable on Win 11 | F1, Assumption 10 (Win 11 run not verifiable by me) |
| Unit tests cover all classes; all pass | E1-E6, V2 |

## Out of Scope (this story)

- Real key storage, key import, MZMK finalize/uninstall, View Keys content (later sub-stories)
- Any use of the test keypair beyond export; no encryption/decryption logic
- Installer, CI pipeline, signing of the exe
