# Project File Structure

```
# Main files

Project Folder
├── CLAUDE.md
├── .agents/tasks/
│   ├── story-1-r1-template.md        ← request template
│   ├── log-1-template.md             ← log template
│   ├── story-{id}-r{rev}.md          ← request with `unique ID` and `revision` from user
│   ├── log-{id}.md                   ← clarifications for request with `unique ID` from user
│   ├── xxxx-plan-{id}-r{rev}.md      ← work plan correspond to `unique ID + revision` request
│   ├── xxxx-report-{id}-r{rev}.html  ← work report correspond to `unique ID + revision` request
│   ├── ...
│   └── archived/                     ← archive folder for tasks related files
├── .agents/assets/                   ← static project files
│   ├── file-structure.md             ← project file structure
│   ├── sop.md                        ← project sop
│   └── ...
├── xxxxx
└── xxxxx                             ← other project file


# Project knowledge base files

Project Folder
└── .agents/project-kb/                     ← project knowledge base (kb) folder
    ├── src/
    │   ├── main-kb-1-r1-template.md        ← lesson captured template
    │   ├── {agent-name}-kb-{id}-r{rev}.md  ← lesson captured by agent (source of truth)
    │   ├── ...
    │   └── archived/                       ← archived lesson source folder
    ├── kb/
    │   └── ...                             ← compiled kb files
    └── kb-index.md                         ← kb entry page (kb index)

```
