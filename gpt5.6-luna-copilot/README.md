# TMD Emulator

The TMD Emulator is a development and training console application for simulating
the MZMK initialization-key export flow without a physical Thales Payshield device.
It is not intended for production key management.

## Build and test

From the repository root:

```powershell
dotnet restore TMDEmulator.sln
dotnet build TMDEmulator.sln --configuration Release
dotnet test TMDEmulator.sln --configuration Release
```

## Run

```powershell
dotnet run --project TMDEmulator\TMDEmulator.csproj
```

The application requires an interactive terminal. Use Up/Down to navigate, Enter
to select, and Esc to go back.

Terminal colors are read from `appsettings.json` beside the executable. If the
file is absent, the built-in black-background defaults are used. Invalid color
names are reported as configuration errors.

## Windows 11 publish

The approved deliverable is a framework-dependent `win-x64` executable:

```powershell
dotnet publish TMDEmulator\TMDEmulator.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false `
  --output publish\win-x64
```

The target machine must have the matching .NET 8 runtime installed.

## MZMK initialization export

The emulator embeds a randomly generated `secp256r1` development-only EC keypair
in PKCS#8 PEM format. The export screen displays its curve, private scalar, and
public point for internal testing.

The generated file name is:

```text
MZMK_Init_{FIRST_8_UPPERCASE_SHA256_SPKI_HEX}{yyyyMMddHHmmss}.der
```

The file content is uppercase HEX text representing the public-key SPKI DER
bytes, without a trailing newline. The `.der` extension is retained for
compatibility with the TMD-specific exchange format. The selected output
directory must already exist, and an existing target file requires confirmation
before it is overwritten.
