---
date: 2026-09-21
id: 1
rev: 1
status: Completed
author: main
description: Implementation plan for the TMD Emulator foundation and MZMK initialization export
---

## Question/Issue

- None. Clarifications are recorded in [log-1.md](./log-1.md).

## Assumptions

- The project targets `.NET 8` and uses SDK-style projects so all operations work through the `dotnet` CLI.
- The application remains a standalone console executable and does not add a web host, database, service, or external runtime dependency.
- The UI uses the built-in `System.Console` APIs rather than a third-party terminal framework. This keeps keyboard behavior, frame rendering, color configuration, and deployment deterministic; no third-party package is required unless implementation testing proves a terminal capability cannot be achieved reliably.
- The test project uses xUnit and references the application project. Tests will focus on deterministic application services and input validation; interactive rendering will receive targeted smoke coverage rather than brittle pixel-by-pixel assertions.
- The application treats the PEM key material as an internal test fixture. It will display the requested key details but will not write private-key material to the generated initialization file.
- The exported file content is the SPKI DER byte array converted to HEX text, despite the `.der` extension, exactly as required by the story.
- File names use the local system clock in `yyyyMMddHHmmss` format and the computed fingerprint. Existing files are never overwritten without an explicit confirmation.
- Navigation is keyboard-driven: Up/Down changes the selected option, Enter activates it, and Esc performs the equivalent of the visible Back option where a parent screen exists.
- Deferred pages are functional placeholders with a title and status message, and do not implement cryptographic behavior prematurely.
- Error and validation messages are surfaced in the status-bar area in red; normal navigation/status text remains separate from error state.
- The implementation will preserve the codebase's stated cross-platform `.NET 8` target while documenting the Windows 11 publish command required by this story.
- The solution is created at the repository root as `TMDEmulator.sln`, with the application in `TMDEmulator/` and tests in `TMDEmulator.Tests/`.
- The Windows deliverable is framework-dependent and published for `win-x64`.
- Exported public-key HEX and the eight-character SHA-256 fingerprint prefix use uppercase hexadecimal, with no trailing newline in the output file.
- The export directory must already exist; the application does not create it.
- A newly generated random `secp256r1` keypair is embedded as the internal development/test PKCS#8 PEM fixture.
- Terminal colors are loaded from `appsettings.json` beside the executable, with validated built-in defaults when the file is absent.

## Plan

### 1. Establish the solution and project layout

- [x] Create a root solution file and the `TMDEmulator/` console project targeting `net8.0`.
- [x] Create `TMDEmulator.Tests/` targeting `net8.0`, add the selected test framework, and reference the application project.
- [x] Add project settings needed for nullable reference types, implicit usings, deterministic builds, and warning visibility consistent with the repository.
- [x] Add only the package references that are proven necessary; otherwise document that the implementation uses the .NET BCL exclusively.
- [x] Verify the new projects restore and build through `dotnet restore` and `dotnet build`.

### 2. Define the application boundaries and configuration

- [x] Add a configuration model for terminal colors and rendering defaults.
- [x] Load and validate `appsettings.json` when present, retain safe built-in defaults when it is absent, and report invalid configured color values explicitly.
- [x] Keep console rendering, menu navigation, file prompting, and cryptographic/export logic in separate classes so the latter can be unit tested without an interactive terminal.
- [x] Ensure all filesystem and cryptographic failures become actionable status-bar errors and are not silently converted into successful results.

### 3. Implement the terminal shell and navigation

- [x] Render a black-background terminal with a main frame and a bottom status-bar frame.
- [x] Render the navigation legend in the status bar and keep the active error/validation message beneath or alongside it without overwriting unrelated state.
- [x] Implement selected-row highlighting with configurable grey/white background and black text, plus white normal text.
- [x] Implement the first-level menu:
  - `Remote MZMK Setup`
  - `Key Management`
  - `Exit`
- [x] Implement the nested menu entries and Back behavior exactly as specified.
- [x] Implement placeholder screens for `View Keys`, `Finalize Remote MZMK`, and `Uninstalling a MZMK`.
- [x] Handle terminal dimensions and redraws defensively so rendering does not crash on a small or redirected console; report unsupported interactive input clearly.

### 4. Implement test-key handling and MZMK initialization export

- [x] Add a dedicated constant/source for the approved PKCS#8 PEM EC keypair.
- [x] Parse the PEM with the .NET cryptography APIs and expose only the display values required by the screen: curve, private scalar HEX, and public point HEX.
- [x] Export the public key as SPKI DER bytes and compute the short fingerprint from the SHA-256 digest of those exact bytes.
- [x] Convert the SPKI DER bytes to the agreed HEX text representation and generate the required file name:
  `MZMK_Init_{fingerprint}{yyyyMMddHHmmss}.der`.
- [x] Prompt for an output directory with the current absolute directory as the default, validate the response, and apply the confirmed existing-directory policy.
- [x] Detect an existing target file and require explicit overwrite confirmation; cancel safely when the user declines.
- [x] Return to the parent menu after a successful save and show a clear success status without exposing private material in the output file.

### 5. Add unit and focused integration coverage

- [x] Test PEM parsing, curve identification, private/public HEX formatting, SPKI export, and fingerprint calculation against fixed expected values.
- [x] Test file-name formatting, output HEX content, timestamp/fingerprint composition, existing-file protection, and invalid directory handling.
- [x] Test configuration parsing and invalid color/configuration values.
- [x] Test menu model transitions, selection bounds, Enter actions, Esc/Back behavior, placeholder routing, and Exit behavior without requiring a live terminal.
- [x] Add a small end-to-end/service-level test that exports a fixture key into a temporary directory and verifies the exact text output.

### 6. Verify build and Windows deliverable

- [x] Run `dotnet test` and confirm all tests pass.
- [x] Run a Release build through the solution.
- [x] Publish the application for Windows 11 using the approved self-contained/framework-dependent choice.
- [ ] Perform a manual terminal smoke test covering startup, navigation, error display, placeholder pages, successful export, declined overwrite, and Exit; this requires a real interactive terminal outside the headless automation session.
- [x] Confirm the published output contains the executable and required configuration file, and that the generated `.der` file contains HEX text rather than binary DER.
- [x] Update directly related usage documentation with build, test, publish, configuration, and key-material handling instructions.

## Acceptance Checklist

- [x] A new `TMDEmulator` application project and unit-test project exist in the agreed layout.
- [x] `dotnet build` and `dotnet test` succeed from the repository root.
- [x] Startup opens the first-level menu and keyboard navigation works as specified.
- [x] The terminal has the two-frame layout, configurable colors, highlighted selection, navigation legend, and red status errors.
- [x] All requested menu entries exist; deferred entries are clearly marked placeholders.
- [x] The approved EC keypair is displayed in the requested formats.
- [x] The initialization export computes the fingerprint from SPKI DER, writes SPKI HEX text with the required file name, validates paths, and protects existing files.
- [x] A Windows 11 executable can be produced through the documented `dotnet publish` command.
- [x] Unit tests cover the created non-UI classes and all tests pass.

## Out of Scope

- Real TMD/Thales Payshield communication or hardware integration.
- Production key ceremony, secure key storage, HSM integration, or production-grade secret protection.
- Implementing the deferred key-management and remote-MZMK operations beyond placeholders.
- Adding a GUI, web API, persistence layer, or automated terminal screenshot comparison.
