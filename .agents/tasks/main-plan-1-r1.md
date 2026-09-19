---
date: 2026-09-19
id: 1
rev: 1
status: Completed
author: main
description: Work plan for story-1-r1 — TMD Emulator project skeleton, rich terminal main program, MZMK Initialization export, unit tests
---

## Question/Issue
- None. Q1–Q5 were answered in `log-1.md` (2026-09-19), and the plan was approved by the user on 2026-09-19.

## Resolved Decisions (from log-1.md)
- Q1: The curve is **derived from the PKCS#8 content**, so the code must not hardcode it. The embedded test keypair is generated on **secp256r1**.
- Q2: .NET 8 is available. The build and tests use the .NET 8 SDK 8.0.131 (runtime 8.0.31) at `/snap/dotnet-sdk-80/current/usr/lib/dotnet/dotnet`. The default `dotnet` on PATH is SDK 10, so it is not used.
- Q3: The deliverable is a self-contained, single-file `win-x64` exe.
- Q4: Spectre.Console (MIT) is approved.
- Q5: The filename is implemented literally: `MZMK_Init_{fp}{yyyyMMddHHmmss}.der`.
- Note: `tmux` isn't installed, so the V5 smoke test uses a Python `pty` harness instead.

## Assumptions

Crypto / export:
- A1. The fingerprint is `SHA-256(SPKI DER)` rendered as **uppercase** hex, taking the first 8 hex characters (= the first 4 bytes).
- A2. The export file content is the SPKI DER as **uppercase hex text**, ASCII, on one line, with no trailing newline and no PEM armour. It keeps the `.der` extension, as the story states.
- A3. The timestamp in the filename is **local time** at the moment of saving, in 24-hour `yyyyMMddHHmmss` format.
- A4. On-screen values (the curve is read from the key, per Q1; the scalar and coordinate lengths come from the curve's key size):
  - "Curve type": the curve name plus its OID, e.g. `NIST P-256 (secp256r1) – OID 1.2.840.10045.3.1.7`.
  - "Private key (HEX scalar)": the raw scalar `d`, left-padded to the curve's byte length (32 bytes for P-256).
  - "Public key (HEX)": the uncompressed EC point `04 || X || Y`. The SPKI hex, which is the file content, is not shown on screen.
- A5. I will generate the hardcoded keypair once, during implementation, with OpenSSL (`openssl genpkey -algorithm EC -pkeyopt ec_paramgen_curve:prime256v1`). I'll paste it as a PKCS#8 PEM (`-----BEGIN PRIVATE KEY-----`) into a constants class. The class will carry an explicit comment: **"TEST ONLY – never use in production"**. Clear-text key handling is accepted for this emulator, per the story.

Output path input:
- A6. The input field is **pre-filled** with the absolute current working directory. Enter accepts it, or the user can edit it.
- A7. A relative path is resolved against the CWD.
- A8. The following are shown as red errors in the status bar:
  - an empty value
  - invalid path characters
  - a path that points to a file rather than a directory
  - a directory that doesn't exist (**not auto-created**)
  - I/O or permission failures on write
- A9. The overwrite prompt is a Yes/No selection navigated with the arrow keys, for consistency with the menus.
  - Yes → overwrite.
  - No → return to the path input.
  - The filename is timestamped to the second, so a collision is unlikely, but the prompt is implemented and tested anyway.
- A10. After a successful save, the program returns to the Remote MZMK Setup menu and shows a success message with the full file path in the status bar.

Navigation / UI:
- A11. Esc on the **top-level** menu does nothing; the program exits only through the "Exit" option. Ctrl+C exits cleanly, and the terminal is always restored (cursor visible, alternate screen buffer left).
- A12. Esc on the path input cancels and returns to the previous menu. A "Back" menu item behaves exactly like Esc.
- A13. "Grey-to-white highlight with black text" means the selected row gets a **solid light-grey background** (default `#C0C0C0`) with black text across the full width of the Main frame. A horizontal gradient is not attempted.
- A14. The status bar (Bottom frame) has:
  - line 1: the navigation legend, e.g. `↑/↓ Navigate · Enter Select · Esc Back`
  - line 2: the message line (errors in red, info/success in the normal/green colour)
  - The message is cleared on the next key press.
- A15. Full-screen redraw on each key press and on terminal resize. If the terminal is smaller than 80×24, a "please enlarge the terminal" notice is shown instead of the UI.
- A16. The "Finalize Remote MZMK", "Uninstalling a MZMK" and "View Keys" pages show a placeholder ("Not yet implemented – planned for a later story") with the title of the page. Esc goes back.

Config:
- A17. Colours are configured in `appsettings.json` next to the exe, under a `Theme` section. It is read with `System.Text.Json`, so no extra configuration packages are needed. Values accept `#RRGGBB` or a named colour.
  - Default keys: `Background` = black, `Foreground` = white, `HighlightBackground` = `#C0C0C0`, `HighlightForeground` = black, `Error` = red, `Success` = green, `StatusBarBackground`, `StatusBarForeground`.
  - A missing file uses the defaults silently.
  - A malformed file or an invalid colour uses the defaults, and a red warning is shown in the status bar at startup.

Project / build:
- A18. Target `net8.0`, with `Nullable` enabled, `ImplicitUsings` enabled and `TreatWarningsAsErrors=true`. The exe assembly name is `TMDEmulator`.
- A19. The solution file is a classic **`.sln`** (created with `dotnet new sln --format sln`). SDK 10 defaults to `.slnx`, which older Visual Studio versions cannot open.
- A20. The test stack is **xUnit v2 + Microsoft.NET.Test.Sdk + coverlet.collector**, with plain xUnit asserts (FluentAssertions is avoided because of its v8 licence change). `Spectre.Console.Testing` is used for render assertions if Q4 = Spectre.
- A21. NuGet package versions are pinned to exact, latest-stable versions compatible with `net8.0`. They are recorded in the report.
- A22. Out of scope for this story: the real MZMK finalize/uninstall logic, key storage and View Keys content, logging, and localisation.
- A23. `.agents/assets/file-structure.md` is **not** modified. `TMDEmulator/` falls under its generic "other project file" entry.

## Target Structure

```
TMDEmulator/
├── TMDEmulator.sln
├── src/TMDEmulator/
│   ├── TMDEmulator.csproj
│   ├── appsettings.json                 ← Theme colours (copied to output)
│   ├── Program.cs                       ← entry: load config, run app, restore terminal
│   ├── Config/     ThemeConfig.cs, ConfigLoader.cs
│   ├── Crypto/     TestKeys.cs (PEM constant), EcKeyInfo.cs (curve/d/Q hex),
│   │               KeyFingerprint.cs, MzmkInitExporter.cs (filename + hex file write)
│   ├── Tui/        IConsoleIO.cs + SystemConsoleIO.cs (key/size/output abstraction),
│   │               Theme.cs, ScreenFrame.cs (Main + Bottom layout), StatusBar.cs,
│   │               MenuNode.cs, MenuScreen.cs, TextInput.cs, ConfirmPrompt.cs, App.cs (nav stack)
│   └── Screens/    ExportMzmkInitScreen.cs, PlaceholderScreen.cs, PathValidator.cs
└── tests/TMDEmulator.Tests/
    ├── TMDEmulator.Tests.csproj
    └── one test class per production class (Program/SystemConsoleIO excluded — thin I/O shims)
```

## Plan (checkable)

### Phase 1 – Project scaffolding
- [x] 1.1 Create `TMDEmulator/` with `.sln` (classic format), `src/TMDEmulator` console project and `tests/TMDEmulator.Tests` xUnit project, all `net8.0`; add both to the solution; add a test→src project reference.
- [x] 1.2 Add pinned packages (Spectre.Console / Spectre.Console.Testing per Q4; xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, coverlet.collector).
- [x] 1.3 Configure csproj: Nullable, TreatWarningsAsErrors, `appsettings.json` CopyToOutput, `InternalsVisibleTo` tests, publish properties for win-x64 (per Q3).
- [x] 1.4 **Goal:** `dotnet build TMDEmulator/TMDEmulator.sln -c Release` → 0 warnings, 0 errors.

### Phase 2 – Crypto core (no UI)
- [x] 2.1 Generate the secp256r1 (per Q1) test keypair once and embed the PKCS#8 PEM in `TestKeys`, with the TEST-ONLY comment.
- [x] 2.2 `EcKeyInfo`: load the PEM, derive and expose the curve name/OID from the key (any named curve; unit-tested with P-256, P-384 and P-521), the private scalar hex (padded), the public point hex (`04||X||Y`), and the SPKI DER bytes.
- [x] 2.3 `KeyFingerprint`: `SHA-256(SPKI DER)` → first 8 uppercase hex characters.
- [x] 2.4 `MzmkInitExporter`: build the filename `MZMK_Init_{fp}{yyyyMMddHHmmss}.der` (with an injectable clock), check whether the file exists, and write the uppercase SPKI hex (with overwrite flag).
- [x] 2.5 **Goal:** the fingerprint and file content match an **independent OpenSSL computation** (`openssl pkey -pubout -outform DER | sha256sum`, `xxd -p -u`). These known-answer values are captured as unit-test vectors.

### Phase 3 – Config & theme
- [x] 3.1 `ThemeConfig` + `ConfigLoader` (defaults / file / malformed / invalid-colour handling, per A17).
- [x] 3.2 Ship a default `appsettings.json`.
- [x] 3.3 **Goal:** the tests cover all four load outcomes, and invalid config never crashes the app.

### Phase 4 – TUI framework
- [x] 4.1 `IConsoleIO` abstraction (ReadKey, window size, write/clear, cursor, alternate screen) + the real `SystemConsoleIO`.
- [x] 4.2 `ScreenFrame`: the Main frame with a title/breadcrumb and the Bottom status-bar frame (legend + message line), black background, themed colours.
- [x] 4.3 `MenuScreen`: ↑/↓ navigation (wraps top↔bottom), Enter selects, Esc = Back, full-width highlighted row.
- [x] 4.4 `TextInput`: an editable line (chars, Backspace/Delete, ←/→, Home/End), pre-fill, Enter submits, Esc cancels.
- [x] 4.5 `ConfirmPrompt` (Yes/No).
- [x] 4.6 `App`: menu tree per the story, a navigation stack, Exit, resize redraw, minimum-size notice, and terminal restore on exit/Ctrl+C.
- [x] 4.7 **Goal:** scripted key sequences, fed through a fake `IConsoleIO`, drive every menu path in the story's menu tree. Assertions check the resulting screen, selection and status message.

### Phase 5 – Screens
- [x] 5.1 `ExportMzmkInitScreen`: display the curve, private scalar and public key; path input with `PathValidator`; overwrite confirm; save; success message; back to the menu.
- [x] 5.2 `PlaceholderScreen` for Finalize Remote MZMK, Uninstalling a MZMK, and View Keys.
- [x] 5.3 All validation and runtime errors are rendered in red on the status-bar message line.
- [x] 5.4 **Goal:** tests cover the happy path and each validation error in A8, plus overwrite Yes/No, Esc cancel, and I/O failure.

### Phase 6 – Unit test completion
- [x] 6.1 Every production class has a test class. `Program` and `SystemConsoleIO` are excluded as thin I/O shims, and the report states this.
- [x] 6.2 Run `dotnet test` with coverlet. **Goal:** all tests pass. Line coverage of the `TMDEmulator` assembly is reported, with a target of ≥ 80% excluding the shims. Any uncovered areas are listed with reasons.

## Verification Plan

- [x] V1 `dotnet build` of the solution (Release) → 0 warnings, 0 errors.
- [x] V2 `dotnet test` → all pass on the .NET 8 runtime (per Q2); coverage figure recorded.
- [x] V3 Crypto known-answer cross-check against OpenSSL (Phase 2.5).
- [x] V4 `dotnet publish -r win-x64` (per Q3) succeeds, and the output is a valid PE `TMDEmulator.exe` + `appsettings.json`.
- [x] V5 Interactive smoke test on Linux inside a pseudo-terminal (`tmux` was unavailable, so a Python `pty` + `pyte` VT-emulator harness was used). Walk every menu, export a file, trigger each validation error, and capture screen snapshots for the report.
- [ ] V6 **Open – user action.** **Limitation:** I cannot run the exe on Windows 11 from this Linux machine. Please do a short manual smoke test on Win 11 (Windows Terminal and the classic console host). I'll include a 2-minute checklist in the report.

### Results (2026-09-19)
- V1: `dotnet build TMDEmulator.sln -c Release` (SDK 8.0.131) gave **0 warnings, 0 errors** from a clean tree. SDK 10.0.112 also builds it with 0/0.
- V2: **141/141 tests pass** on the .NET 8.0.31 runtime. Coverage is **100% of lines (453/453) and 99.5% of branches (204/205)**. `Program` and `SystemTerminal` are excluded with `[ExcludeFromCodeCoverage]`. The one uncovered branch is in `ScreenFrame.Wrap`: a single double-width character wider than the whole frame, which can't happen at the 80-column minimum.
- V2 mutation check: lowercasing the fingerprint and stripping the P-521 leading zero caused 13 test failures. The tests were then restored and all pass.
- V3: the fingerprints and SPKI HEX for P-256, P-384 and P-521 match OpenSSL 3.5.5. The smoke test also checks the saved file byte-for-byte against the OpenSSL SPKI DER.
- V4: `dotnet publish src/TMDEmulator -c Release -r win-x64` produces `TMDEmulator.exe` (PE32+ console, x86-64, 65.4 MB, self-contained single file) with `appsettings.json` beside it.
- V5: **49/49 smoke checks pass** against the real binary in a 100×30 pty. They cover the menus, Esc and Back, colours, validation errors, save, the overwrite prompt (No and Yes), placeholders, the too-small notice and resize, Exit, Ctrl+C, terminal restore, custom, invalid and malformed config, and the redirected-stdin guard.
- Known environment note: the default `dotnet` on this machine (SDK 10 snap) cannot *run* the `net8.0` tests, because the .NET 8 runtime is in a separate snap. Use `/snap/dotnet-sdk-80/current/usr/lib/dotnet/dotnet`. This is captured in `main-kb-1-r1.md`.

## Implementation Notes / Deviations from Plan
- **File layout:** the structure differs slightly from the Target Structure above.
  - `IConsoleIO`/`SystemConsoleIO` were named `ITerminal`/`SystemTerminal`.
  - `StatusBar.cs` was folded into `ScreenFrame` (the status bar is 4 rows of the same layout).
  - `MenuNode` became `MenuItem` (in `MenuScreen.cs`).
  - New small model files: `Role`, `BodyLine`/`Span`, `ScreenResult`/`StatusMessage`, `IScreen`, `FrameRenderable`, and `Screens/MainMenu.cs` (builds the menu tree).
- **A4 padding:** the planned `LeftPad` helper was removed as dead code. .NET already exports `D`, `Q.X` and `Q.Y` at the curve's fixed length. The P-521 vector, whose scalar starts with `00`, proves this.
- **A17 refinement:** an invalid colour falls back to the default *for that key only*; other valid keys still apply. An extra `Border` colour key (default grey) was added for the frame lines.
- **Addition – redraw coalescing:** the smoke test showed about 8.4 ms and 3.4 KB of output per key, redrawn once for every key. Pasting a path would redraw once per character. `App` now handles all queued keys before drawing, which brought a 200-key burst down to one redraw (about 1.8 ms/key including harness wait).
- **Addition – non-interactive guard:** `Program` exits with code 1 and a message if stdin or stdout is redirected. Without it, `Console.KeyAvailable` throws on redirected input.
- **Fix found by smoke test:** the malformed-config warning began with the full file path, so the status bar truncated the actual problem. It now reads `appsettings.json could not be read; default colours used. <reason>`.
- **Build setting:** `InvariantGlobalization=true` was set so the Linux build doesn't depend on ICU. This affects no user-visible text; the timestamp uses fixed-format digits.
- **Publish settings:** `SelfContained` and `PublishSingleFile` are set in the csproj whenever a RuntimeIdentifier is given. So `dotnet publish -c Release -r win-x64` alone produces the Q3 deliverable.

## Review Deliverables

- [x] `main-report-1-r1.html`: context, requirement trace, test-key values, real terminal captures, deviations, package versions, verification evidence, the Win 11 checklist, and an 8-question quiz.
- [x] `.agents/project-kb/src/main-kb-1-r1.md`: the SDK 10 vs .NET 8 runtime snap split on the build machine.
- [x] Plan status → `Completed`. V6 (the Win 11 manual run) is the only open item and needs the user; nothing has been committed to git.
