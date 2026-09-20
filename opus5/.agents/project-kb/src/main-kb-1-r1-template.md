---
# RAW LESSON CAPTURE — filled in by the working agent. This is a source-of-truth record.
# This file is append only; never edit after
# You do NOT touch kb-index.md. This is KB agent's job.
date: <YYYY-MM-DD>
id: <corresponding id>
rev: <corresponding rev>
title: <one-line summary of the lesson>
author: <agent-name - main / subagent name>
area: <best-guess category — the KB agent may recategorise>
tags: [<keyword>, <keyword>]
---
 
## Trigger
<The situation a future agent will be in when this applies. Write it as "When ...".
Be concrete about the symptom, error, task type, or file — this is what makes it matchable.>
 
## Lesson
<The core insight as a durable fact. One short paragraph. State what is *true* about
this project — do not narrate what you did today.>
 
## Do this
<The concrete action to take: command, config, code, or decision. Copy-pasteable where possible.>
 
## Avoid
<The anti-pattern / what not to do. Delete this section if not applicable.>
 
## Why / evidence
<Root cause + the specific proof: exact error text, file:line, commit hash, or observed
behaviour. This is what lets the KB agent (and future agents) trust it without re-deriving.>
 
## References
<Code paths, docs, or related capture filenames.>

```
# Example

---
date: 2026-01-14
id: 1
rev: 1
title: Incorrect property value while the app is running under container image
author: main
area: Testing
tags: [container, testing]
---

## Trigger
Testing the app under container image but found that the app running with incorrect properties

## Lesson
The app.properties included in container image during build is a generic one (or it is a template) only and so it is required to specify property values via environment variable at startup.

## Do this
`docker run -it -e app.task=xxx xxx`

## Avoid
Don't set the testing property value to the template app.properties as it is config template only

## Why / evidence
The container image is build with template app.properties

## References
NIL

```