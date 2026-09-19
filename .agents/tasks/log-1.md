---
story-id: 1
description: clarifications / supplementary items for the story
---

## Clarifications/Supplementary Items

2026-09-19:
- **Q1 – EC curve for the hardcoded test keypair.** The curve can be extracted from the PKCS#8 content and so it depends on the value. However, if generate a testing keypair, please use secp256r1 by default
- **Q2 – .NET 8 runtime is not installed on this build machine.** - just installed and can use .net80 now
- **Q3 – Form of the Win 11 exe.** - self-contained, single-file `win-x64`** (`dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`). Testers get one `TMDEmulator.exe` plus `appsettings.json`, with no .NET install needed. It is about 70 MB.
- **Q4 – Rich-terminal library (third-party dependency).** - approved for Spectre.Console (MIT)
- **Q5 – Output filename format.** The story gives `MZMK_Init_{8_chars_short_fingerprint}{YYYYMMDDHHmmss}.der`, which has **no separator** between the fingerprint and the timestamp (e.g. `MZMK_Init_3FA9C01B20260919143005.der`) - implement it literally, as written

