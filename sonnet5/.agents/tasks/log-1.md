---
story-id: 1
description: clarifications / supplementary items for the story
---

## Clarifications/Supplementary Items

2026-09-20:
- *Q1. EC curve and keypair source (needs your answer)** - The curve type can be extracted from the hardcoded testing keypair PKCS#8 content, so it is depends on the testing keypair. However, if you generating testing keypair for development, please use secp256r1 by default.

- **Q2. Exact meaning of "HEX" for the public key (needs your answer)** - `04||X||Y` point, upper case

- **I1. Dev machine only has .NET 10 (no .NET 8) - decision on how to verify** - use DOTNET_ROLL_FORWARD=LatestMajor
