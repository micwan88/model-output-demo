---
story-id: 1
description: clarifications / supplementary items for the story
---

## Clarifications/Supplementary Items

2026-09-21:
- Please confirm whether the solution file should be created at the repository root and named `TMDEmulator.sln`, with the application project in `TMDEmulator/` and tests in `TMDEmulator.Tests/`. This is the recommended layout because the repository currently has no solution or project files. - yes, please
- Please confirm whether a self-contained `win-x64` publish is required for the Windows executable, or whether a framework-dependent executable is sufficient. The recommended default is a self-contained Release publish for Windows 11. - framework-dependent executable, please
- Please confirm the required casing and text representation for the exported public-key HEX. The recommended default is uppercase hexadecimal with no additional DER encoding and no trailing newline. - yes, please
- Please confirm the expected short-fingerprint casing. The recommended default is the first eight uppercase hexadecimal characters of `SHA-256(SPKI DER)`. - yes, please
- Please confirm whether the output directory must already exist or may be created by the application. The recommended default is to require an existing directory and report an error otherwise. - yes, please
- The story requests a hardcoded EC keypair but does not provide the actual test key material or curve. Implementation is blocked until an approved internal test keypair and curve are supplied, or permission is given to generate a deterministic development-only keypair. - please generate a random testing key and then use it (by default use secp256r1 as curve for that random key), also, The curve type can be extracted from the hardcoded testing keypair PKCS#8 content, so it is depends on the testing keypair.
- The story requests configurable terminal colors but does not define the configuration file or precedence rules. The recommended default is an `appsettings.json` file beside the executable, with validated values and built-in defaults when the file is absent. - yes, please
