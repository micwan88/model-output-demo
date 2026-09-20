# Standard Operating Procedure

There are 4 stages for work unless user request to override it explicitly. They are `Planning`, `Implementation`, `Verification` and `Review`.

## Workflow Overview

```
Start -> `Planning` -> `Implementation` -> `Verification` -> `Review` -> End
```

Note:
- `Implementation` or `Verification` can back to `Planning` once something go unexpectedly

## Files Between Stages

### File List
- {request-file-name}-{id}-r{rev}.md: the request given by user/upstream
- {agent-name}-plan-{id}-r{rev}.md: the work plan with status/question/issue/assumption through out the workflow
- log-{id}.md: the clarification/addtional details of the request which given by user/upstream
- {agent-name}-kb-{id}-r{rev}.md: lesson captured from particular request if any
- {agent-name}-report-{id}-r{rev}.html: summary report state all finished work for human to read it (`html`)

Note:
- `{request-file-name}`: the file name give by user/upstream, can be "story", "request", "spec" or any other name
- `{agent-name}`: main / subagent name
- `{id}`: unique number represent particular request given by user/upstream, otherwise increment by 1 from last max {id}.
- `{rev}`: revision number that represent n-th version of the file.

### File Template and Metadata

```
---
date: yyyy-mm-dd
id: d+
rev: d+
status: Draft | Blocked | Approved | InProgress | Completed
author: {agent-name} | user
description: some description
---

## Question/Issue
- None

## Assumptions
- xxx

## Sample sections 1
- xxx

## Sample sections 2
- xxx
```

status:
- `Draft`: draft and waiting for user/upstream review
- `Blocked`: outstanding questions/issues exists in "Question/Issue" section
- `Approved`: approved by user/upstream and ready to next stage
- `InProgress`: in progress
- `Completed`: all tasks are completed with summary report generated

## Stage Details

### 1. Planning

- Analyze the request and think
- Explore and understand the existing stuffs if request change on existing things like codebase
- Search keywords in `kb-index.md` under `Analysis` area to see if there is any project knowledge base matching your work. If yes then follow the link, read the content and apply the knowledge.
- Write a detail plan "{agent-name}-plan-{id}-r{rev}.md" with checkable items
- Transform tasks into verifiable goals
- State all questions/uncertains explicitly in "Question/Issue" section (if any)
- State all assumptions explicitly in "Assumptions" section.
- Mark status = `Draft` for the plan (If have question/issue, mark as `Blocked`)
- Notify user/upstream to review and waiting for plan approval.
- Update status to `Approved` and go next stage only when user said "approved" explicitly

Input:
- "{request-file-name}-{id}-r{rev}.md" or "prompt" given by user/upstream
- log-{id}.md (optional)

Output:
- Detail work plan "{agent-name}-plan-{id}-r{rev}.md" for user/upstream review

Note:
- `{id}` and `{rev}` in plan must be matched with the request file from user/upstream no matter in metadata or filename, otherwise, `{id}` start from `{last max id + 1}` and `{rev}` start from `{last max rev with same id + 1}`
- user/upstream may override to skip `Planning` stage for trivial task. e.g. fix typo, install a package, rename a file, ..etc.

### 2. Implementation

- Search keywords in `kb-index.md` under `Implementation` area to see if there is any project knowledge base matching your work. If yes then follow the link, read the content and apply the knowledge.
- Actual work according to the request / the plan
- The given plan must be status = `Approved` and `None` is shown under "Question/Issue" section. If not, push back and state.
- Mark status = `InProgress`
- Mark items complete as you go in the plan
- If something goes sideways with plan/request, STOP and state it in plan under "Question/Issue" section + mark `Blocked` to status then push back to user/upstream
- Once all completed, can go next stage

Input:
- Approved plan
- Or "{request-file-name}-{id}-r{rev}.md" or "prompt" if user skipped the `Planning` stage

Output:
- Actual work output
- Mark items complete as you go in the plan (if exist)

### 3. Verification

- Search keywords in `kb-index.md` under `Verification` area to see if there is any project knowledge base matching your work. If yes then follow the link, read the content and apply the knowledge.
- Test/Verify/Check the work output against with plan or the request
- Confirm the correctness of the work output. If not, go back last stage to fix it
- If something goes sideways with plan/request, STOP and state it in plan under "Question/Issue" section + mark `Blocked` to status then push back to user/upstream
- If everything has been proof for correctness, go next stage

Input:
- Work output
- The plan / request from user/upstream

### 4. Review

- Write a summary report in `{agent-name}-report-{id}-r{rev}.html`. Let user/upstream understand everything happened in the change. State with context, intuition, what was done, what was skipped, etc. and a quiz at the bottom on the changes that I must pass.
- Mark plan status = `Completed`
- Capture lesson in `{agent-name}-kb-{id}-r{rev}.md` base on the template `main-kb-1-r1-template.md` if you think something can be improved/avoided for similar work later. It must be something durable, project-specific, and non-obvious. If not, just skip it.
- Notify user/upstream the completion

Input:
- Work output
- The plan / request from user/upstream

Output:
- Capture lesson if any
- The plan marked as completed if any
- The summary report

## Core Principles for Work

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.
- Enter plan mode by default, even verification steps, not just building

### 2. Subagent Strategy

**Offload task to subagents for complex problems**

- Use subagents liberally to keep main context window clean
- Offload research, exploration, and parallel analysis to subagents
- For complex problems, throw more compute at it via subagents
- One task per subagent for focused execution

### 3. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 4. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### 5. Proven To Work / Validated

**Verify it before marking the whole task done**

- Never mark a task complete without proving it works
- Diff behavior between main and your changes when relevant
- Challenge your own work before presenting it
- Ask yourself: "Would a staff engineer approve this?"
- Run tests, check logs, demonstrate correctness
- When there is a problem, find root causes. No temporary fixes. Senior developer standards.
