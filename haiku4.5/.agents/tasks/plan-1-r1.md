# TMD Emulator Implementation Plan
## Story 1, Revision 1 | Status: Ready for Review

---

## Phase Overview

This project will be implemented in **4 sequential phases**:

1. **Foundation** - Project structure, dependencies, configuration
2. **Core UI Framework** - Terminal rendering, menu system, navigation
3. **Feature Implementation** - Cryptography layer, export functionality
4. **Testing & Validation** - Unit tests, integration tests, verification

---

## PHASE 1: Project Foundation & Architecture

### 1.1 Project Structure Setup

Create directory hierarchy under project root:

```
TMDEmulator/
├── TMDEmulator.Main/                 # Main console application
│   ├── Program.cs
│   ├── TMDEmulator.Main.csproj
│   ├── appsettings.json
│   ├── UI/
│   │   ├── Terminal/
│   │   │   ├── TerminalRenderer.cs
│   │   │   ├── MenuScreen.cs
│   │   │   ├── ColorScheme.cs
│   │   │   └── TerminalInput.cs
│   │   └── Screens/
│   │       ├── BaseScreen.cs
│   │       ├── MainMenuScreen.cs
│   │       ├── RemoteMZMKScreen.cs
│   │       ├── KeyManagementScreen.cs
│   │       ├── ExportMZMKScreen.cs
│   │       └── PlaceholderScreen.cs
│   ├── Crypto/
│   │   ├── KeyConstants.cs
│   │   ├── ECKeyManager.cs
│   │   └── KeyExporter.cs
│   ├── Models/
│   │   ├── MenuItem.cs
│   │   ├── MenuNode.cs
│   │   └── ExportResult.cs
│   └── Utilities/
│       ├── PathHelper.cs
│       └── ValidationHelper.cs
├── TMDEmulator.Tests/                # Unit test project
│   ├── TMDEmulator.Tests.csproj
│   ├── UI/
│   │   ├── TerminalRendererTests.cs
│   │   └── MenuScreenTests.cs
│   ├── Crypto/
│   │   ├── ECKeyManagerTests.cs
│   │   └── KeyExporterTests.cs
│   └── Utilities/
│       └── ValidationHelperTests.cs
└── TMDEmulator.sln                   # Solution file
```

### 1.2 Dependencies & Libraries

| Library | Version | Purpose |
|---------|---------|---------|
| `System.Security.Cryptography` | Built-in (.NET 8+) | EC key handling, SHA-256 hashing |
| `Spectre.Console` | 0.49.0+ | Rich terminal UI, colors, styling, box drawing |
| `xUnit` | 2.6.0+ | Unit testing framework |
| `Moq` | 4.20.0+ | Mocking library for tests |
| `Microsoft.Extensions.Configuration` | 8.0.0 | Configuration management |
| `Microsoft.Extensions.DependencyInjection` | 8.0.0 | Dependency injection |

### 1.3 Configuration Strategy

**appsettings.json** structure:

```json
{
  "UI": {
    "Colors": {
      "BackgroundColor": "Black",
      "TextColor": "White",
      "HighlightBackgroundColor": "Grey23",
      "HighlightTextColor": "Black",
      "ErrorColor": "Red",
      "StatusBarBackgroundColor": "DarkSlateGray"
    },
    "Display": {
      "FrameBorder": "Rounded",
      "StatusBarHeight": 2
    }
  },
  "Paths": {
    "DefaultExportPath": "<current-directory>"
  }
}
```

**Configuration Loading:**
- Use `IConfiguration` from `Microsoft.Extensions.Configuration`
- Load at startup in `Program.cs`
- Inject via dependency injection into UI components

---

## PHASE 2: Core UI Framework

### 2.1 Terminal Rendering Engine

**TerminalRenderer.cs** - Manages screen layout and rendering:
- Two-frame architecture: Main content frame + Status bar frame
- Dimensions management (rows/columns)
- Frame drawing with configurable borders
- Color application per ColorScheme
- Clear/refresh screen logic

**ColorScheme.cs** - Color management:
- Centralized color constants from config
- Methods: `GetHighlightStyle()`, `GetErrorStyle()`, `GetNormalStyle()`
- Text/background color pairing
- Theme validation

**TerminalInput.cs** - Input handling:
- Async input capture from console
- Arrow key detection (↑↓)
- Enter key handling (confirmation)
- Esc key detection (back/exit)
- Non-blocking input for responsive UI

**MenuScreen.cs** - Navigation state machine:
- Current selection index tracking
- Menu item list management
- Arrow navigation logic
- Selection confirmation
- Screen transition handling

### 2.2 Screen Hierarchy

**BaseScreen.cs** (abstract):
- Abstract methods: `Render()`, `HandleInput(ConsoleKeyInfo)`
- State management: `IsActive`, `ParentScreen`, `ChildScreen`
- Utility: `GetStatusBarText()`, `ClearWithColor()`
- Navigation support: `GoBack()`, `GoNext(nextScreen)`

**MainMenuScreen.cs:**
- Initial startup screen
- Three menu options: Remote MZMK Setup, Key Management, Exit
- Back functionality: Esc exits application
- Status bar: "↑↓ Navigate | Enter Select | Esc Exit"

**RemoteMZMKScreen.cs:**
- Four menu options: Export MZMK Initialization Key, Finalize Remote MZMK, Uninstalling a MZMK, Back
- Status bar: "↑↓ Navigate | Enter Select | Esc Back"
- Routing to child screens

**KeyManagementScreen.cs:**
- Two menu options: View Keys, Back
- Status bar identical to RemoteMZMKScreen
- Placeholder for View Keys (deferred)

**ExportMZMKScreen.cs:**
- Main complex screen - detailed implementation in Phase 3
- Multi-step workflow
- Error display in red

**PlaceholderScreen.cs:**
- Template for deferred features
- Displays: "This feature will be available in a future update"
- Back button functional

---

## PHASE 3: Feature Implementation

### 3.1 Cryptographic Foundation

**KeyConstants.cs** - Hardcoded testing keypair storage:

```csharp
public static class KeyConstants
{
    public const string TestingECPrivateKeyPem = @"-----BEGIN PRIVATE KEY-----
    [base64 encoded PKCS#8 EC key here]
    -----END PRIVATE KEY-----";
    
    public const string CurveName = "P-256";  // or P-384, P-521
}
```

**ECKeyManager.cs** - Key parsing and analysis:
- Load EC key from PEM string using `ECDsa.Create()` with PKCS#8 format
- Extract private key scalar (hex format)
- Extract public key point (X, Y coordinates in hex)
- Curve type detection
- Methods:
  - `LoadPrivateKey(pemString)` → ECDsa object
  - `GetPrivateKeyHex()` → scalar as hex string
  - `GetPublicKeyHex()` → point as hex string
  - `GetCurveType()` → string representation

**KeyExporter.cs** - SPKI export and fingerprinting:
- Generate SubjectPublicKeyInfo (SPKI) DER encoding
- Calculate SHA-256 hash of SPKI DER
- Extract first 8 characters as fingerprint
- Build filename: `MZMK_Init_{fingerprint}{timestamp}.der`
- Save hex-encoded SPKI as `.der` file (with .der extension despite hex content - per TMD spec quirk)
- Methods:
  - `GenerateSPKIDer(publicKey)` → byte[]
  - `ComputeFingerprint(spiDerBytes)` → string (8 chars)
  - `ExportToFile(outputPath, spiHex, timestamp)` → bool with exception handling

### 3.2 Export MZMK Initialization Key Screen Workflow

**ExportMZMKScreen.cs workflow:**

1. **Display Key Details Section:**
   - Show hardcoded keypair information in a box:
     - "Curve Type: [value]"
     - "Private Key (HEX Scalar): [value, formatted in rows]"
     - "Public Key (HEX): [value, formatted in rows]"

2. **User Input Section:**
   - Prompt: "Output path for MZMK Initialization file:"
   - Default value: current working directory (absolute path)
   - Input validation:
     - Path must exist (directory validation)
     - Path must be writable (test write attempt)
     - Display validation errors in RED
   - Status bar: "Enter Path or press Esc to cancel"

3. **File Conflict Handling:**
   - When file exists:
     - Display: "File already exists: [filename]"
     - Ask: "Overwrite? (Y/N)"
     - Handle confirmation

4. **Export Execution:**
   - Generate SPKI DER from public key
   - Calculate 8-char fingerprint from SHA-256(SPKI)
   - Create filename with timestamp (YYYYMMDDHHmmss format)
   - Export SPKI as hex-encoded content with .der extension
   - Save to user-specified path

5. **Success/Error Feedback:**
   - Success: Display file path in status bar, return to menu
   - Error: Display error message in RED color, remain on screen or return to menu
   - Show operation status before return (e.g., "File saved successfully. Press any key to continue...")

### 3.3 Input Validation & Error Handling

**ValidationHelper.cs:**
- Methods:
  - `IsValidDirectoryPath(path)` → bool
  - `IsDirectoryWritable(path)` → bool
  - `ValidateOutputPath(path)` → (bool isValid, string errorMessage)
  - `SanitizeFilePath(path)` → string

**Error Display Strategy:**
- All validation errors displayed in status bar in RED
- Remain on screen to allow user correction
- Error text persists until next operation or user navigation

---

## PHASE 4: Testing & Validation

### 4.1 Unit Test Structure

| Test File | Target Coverage | Key Tests |
|-----------|-----------------|-----------|
| `TerminalRendererTests` | 85%+ | Frame layout, color application |
| `MenuScreenTests` | 90%+ | Navigation, selection, key handling |
| `ECKeyManagerTests` | 100% | PEM parsing, hex conversion, key extraction |
| `KeyExporterTests` | 100% | SPKI generation, fingerprint computation, file output |
| `ValidationHelperTests` | 95%+ | Path validation, writeability checks |

### 4.2 Integration Testing

- Full workflow: Menu → Remote MZMK → Export → File saved → Back
- Error paths: Invalid path input → validation error → retry
- Navigation at each level with Esc key

### 4.3 Manual Verification

**UI Verification Checklist:**
- [ ] Colors render correctly (black bg, white text, grey highlights)
- [ ] Arrow key navigation works in all menus
- [ ] Esc key returns to previous menu or exits
- [ ] Status bar displays correct help text
- [ ] All placeholder screens display correctly
- [ ] Terminal handles resize gracefully

**Cryptography Verification:**
- [ ] EC key loads from PEM without exceptions
- [ ] Private key hex conversion matches expected format
- [ ] Public key hex conversion is valid EC point
- [ ] SPKI DER generation is valid DER structure
- [ ] SHA-256 fingerprint is consistent
- [ ] Exported file is readable and contains hex data
- [ ] Filename follows convention: `MZMK_Init_{8chars}{timestamp}.der`

---

## Key Architectural Decisions & Trade-offs

### 1. Spectre.Console vs Manual ANSI Implementation
**Decision:** Use Spectre.Console
- **Pros:** Cross-platform compatibility (Win/Linux), built-in color/box support, maintenance-free
- **Cons:** External dependency
- **Rationale:** Reduces code complexity and ensures consistent rendering across target platforms

### 2. Dependency Injection vs Static Configuration
**Decision:** Use Microsoft.Extensions.Configuration with DI
- **Pros:** Testable, configurable without recompile, follows .NET conventions
- **Cons:** Slightly more setup code
- **Rationale:** Aligns with .NET 8 best practices and corporate standards

### 3. Single-File PEM Constants vs External Key File
**Decision:** Hardcoded PEM in KeyConstants.cs
- **Pros:** Simple, no external file management, spec requirement for "internal testing"
- **Cons:** Less secure (appropriate for testing only)
- **Rationale:** Story explicitly states this is for testing/internal use

### 4. Async/Await for Input vs Blocking IO
**Decision:** Use async input handling
- **Pros:** Responsive UI, prevents UI freezing
- **Cons:** Slightly more complex code
- **Rationale:** Better user experience, especially for validation operations

### 5. SPKI Output as Hex with .der Extension
**Decision:** Implement exactly as specified (hex content, .der extension)
- **Pros:** Meets TMD compatibility requirement
- **Cons:** Potentially confusing (actual content is hex, not binary DER)
- **Rationale:** TMD specification requirement, noted as "TMD specific thing"

---

## Implementation Sequencing & Dependencies

### Build Order:
1. **Solution & Project Files** (no dependencies)
2. **Models & Constants** (Models/MenuItem.cs, Crypto/KeyConstants.cs)
3. **Configuration** (appsettings.json, configuration loading in Program.cs)
4. **Cryptography Layer** (ECKeyManager.cs, KeyExporter.cs) - depends on KeyConstants
5. **UI Framework** (ColorScheme.cs, TerminalRenderer.cs, TerminalInput.cs) - depends on config
6. **Screen Hierarchy** (BaseScreen.cs → MainMenuScreen.cs, RemoteMZMKScreen.cs, etc.)
7. **Feature Screens** (ExportMZMKScreen.cs) - depends on crypto layer + UI framework
8. **Main Program** (Program.cs) - connects everything
9. **Tests** (parallel to implementation)

---

## Dependencies - NuGet Packages

```xml
<!-- Main Application -->
<PackageReference Include="Spectre.Console" Version="0.49.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
<PackageReference Include="Moq" Version="4.20.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

---

## Technical Considerations

### Cryptography
- **EC Key Format:** PKCS#8 PEM required (standard for .NET interop)
- **SPKI Generation:** Use built-in ECPublicKeyParameters to DER encoding
- **SHA-256:** Use `System.Security.Cryptography.SHA256`
- **Hex Encoding:** Use `Convert.ToHexString()` (.NET 8+)
- **Security Note:** This is for testing only; production would require hardware key storage

### Terminal UI
- **Color Rendering:** Spectre.Console abstracts platform differences
- **Resize Handling:** Cache terminal dimensions, refresh on resize events
- **Input Blocking:** Use async Tasks to prevent UI freeze during validation
- **Unicode Support:** Spectre supports box-drawing characters across platforms

### Cross-platform Considerations
- **Path Separators:** Use `Path.Combine()` not hardcoded `/` or `\`
- **Line Endings:** System.Console handles automatically
- **Temporary Files:** If needed, use `Path.GetTempPath()` with care
- **Execution:** Test on Windows 11; Linux support automatic via .NET 8

---

## Potential Challenges & Mitigation

### Challenge 1: EC Key PKCS#8 Format Conversion
**Mitigation:** Use test vectors to validate parsing; test PEM loading before feature implementation

### Challenge 2: Cross-platform Terminal Color Support
**Mitigation:** Leverage Spectre.Console's built-in compatibility layer; manual testing on target OSs

### Challenge 3: File Write Permissions & Validation
**Mitigation:** Implement comprehensive path validation helper; test against locked directories

### Challenge 4: UI Responsiveness During Crypto Operations
**Mitigation:** Use async/await; show progress indicator for SHA-256 if file is large

### Challenge 5: Hex Encoding Output Format
**Mitigation:** Document TMD spec quirk; include comments in code explaining .der extension issue

---

## Success Verification Checklist

- [ ] .sln builds without errors via `dotnet build`
- [ ] .sln builds for Win11 via `dotnet publish -c Release -r win-x64`
- [ ] All unit tests pass: `dotnet test`
- [ ] Main menu displays with correct colors and navigation
- [ ] Export MZMK flow complete with file creation
- [ ] Exported file contains valid hex-encoded SPKI data
- [ ] Filename format matches specification
- [ ] Error messages display in red
- [ ] Esc returns to previous menu at all levels
- [ ] All placeholder screens functional
- [ ] No unhandled exceptions in normal workflows
- [ ] Code follows corporate developer standards (procedures, traceability, maintainability)

---

## Critical Files for Implementation

- `/micwan/workspaces/ai-work/haiku4.5-output-demo/TMDEmulator/TMDEmulator.sln`
- `/micwan/workspaces/ai-work/haiku4.5-output-demo/TMDEmulator/TMDEmulator.Main/Program.cs`
- `/micwan/workspaces/ai-work/haiku4.5-output-demo/TMDEmulator/TMDEmulator.Main/Crypto/KeyConstants.cs`
- `/micwan/workspaces/ai-work/haiku4.5-output-demo/TMDEmulator/TMDEmulator.Main/UI/Screens/ExportMZMKScreen.cs`
- `/micwan/workspaces/ai-work/haiku4.5-output-demo/TMDEmulator/TMDEmulator.Main/UI/Terminal/TerminalRenderer.cs`

---

**Plan Status:** Ready for Review  
**Next Step:** User approval to begin Phase 1 implementation
