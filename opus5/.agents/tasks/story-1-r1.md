---
date: 2026-09-19
id: 1
rev: 1
status: Draft
author: Mic
description: story to creating TMD Emulator main program
---

## Background

In the project, the proper way to exchange key between external system and our system is through TMD (thales payshield trusted management device) which is a secure physical device. However, somehow it is hard to arrange this physical device for development, testing or customer training purpose.

Therefore, it is better to have a emulator in `controlled environment` like internal development and testing env (or even demo/training env). The story aims to create a TMD emulator - a standalone program for such purpose.

We split the whole emulator program into multiple phases/stages (sub-stories) and this story is a sub-story which focus on the items described as below section.

## Objectives/Intention

- Create new .NET project for TMD emulator
- Make this new project can be built via dotnet cli
- Create TMD emulator main program in C# - standalone rich terminal program (Expected the UI layout would be similar to copilot CLI / Claude code / Pi code something, user can navigate the option through arrow keys)
- Showing a option menu when first start of the program and waiting user input for selection
- As the program is for testing/internal purpose, so can somehow less restriction on security precautions. Say we will save the key value in clear in file

## Story Details

.NET C# project:
- Create a new folder `TMDEmulator` under project root
- Create a rich terminal program project for TMD Emulator
- Create a unit test project for this emulator
- Create visual studio project files so that can be build by dotnet cli
- Declare any 3rd party lib required if any

TMD Emulator main program:
- Color-mode rich terminal program which like the popular AI IDE tool, Claude code, copilot CLI, user can use arrow keys to navigate between the menu options
   - Up/Down to navigate option
   - Press Enter to select
   - Press Esc back to previous screen = Back option
   - Make the whole terminal like have two frames - Main and Bottom frame for status bar
   - Rich terminal style highlighted the option - box/row style background being grey but the text being black, Try follow the style from how Copilot CLI and Claude Code work
- For color
   - Black background
   - White text
   - Grey-to-white highlight with black text
   - Color is configurable in config
- Status bar in the bottom of the screen which showing navigation legend
- Showing 1st level options menu when startup
- Options menu layer as below: (Will add more in later story)
   1. Remote MZMK Setup
      1. Export MZMK Initialization Key
      2. Finalize Remote MZMK
      3. Uninstalling a MZMK
      4. Back
   2. Key Management
      1. View Keys
      2. Back
   3. Exit
- A constant class for us to put hardcoded internal testing EC keypair in PKCS#8 - PEM format
- Menu function - Export MZMK Initialization Key:
   - The screen showing the hardcode keypair detail included:
      1. Curve type
      2. Private key (HEX scalar)
      3. Public key (HEX)
   - Asking user to input output path of saving the "MZMK Initialization" DER file. Showing currently absolute directory as the default value.
   - Generate 8 chars short fingerprint by performing SHA-256 of the SubjectPublicKeyInfo (SPKI) DER and get the first 8 chars
   - Export the public key (SPKI) in HEX (not DER format) and save it as `MZMK_Init_{8_chars_short_fingerprint}{YYYYMMDDHHmmss}.der` under the folder which given by user
   - Back to menu after saved
   - The initialization output HEX text with a `.der` extension (I know it is incorrect and confusing, but it is TMD specific thing)
   - Ask user if overwritten or not when file exist
- Validate all user input and show validation error in RED color
   - in Status bar section under the legend
- Show any error in RED color
   - in status bar section under the legend
- Menu function - View Keys, Finalize Remote MZMK, Uninstalling a MZMK
   - Empty page first, will do it in later sub-story
- Output deliverable would be exe and target can be run under Win 11
- Deferred pages only display placeholders for later

TMD Emulator unit test:
- Create corresponding unit test and try to cover all class, util, function have been created

## Success Criteria

- A new project has been created and matched with the detail required
- The project can be built by dotnet cli
- A color-mode rich terminal program has been created and match the details required
- Unit test all passed

## Amendments

None