---
description: 'Implement the next developer story — write code, write tests, verify the build'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal', 'githubRepo']
---

# Neo Implement

Follow the workflow defined in `.github/agents/neo-dev.agent.md`.

1. If `artifacts/STATE.md` exists, read it. If Current Phase is not "Implementation",
   warn: "STATE.md shows phase is {phase}. /neo-implement expects Implementation phase.
   Continue anyway?" Wait for confirmation.
2. Read the story index in `artifacts/stories/` to identify the next story to work on. **Priority order:** first check for any `IN_PROGRESS` story (resume it before starting a new one), then pick the next `TODO` story. If the index uses wave-based format, pick from the current wave (all stories in a wave can be built in parallel). Skip any `BLOCKED` stories. If no story index exists, stop and tell the user: "No stories found. Run `/neo-break` or `/neo-plan` first."
3. If the user specified a story number, implement that one (regardless of status, unless it is `DONE`).
4. Read the full story file.
5. Verify that dependencies from previous stories are in place.
6. If `artifacts/architecture/conventions.md` exists, read it for codebase patterns. If not, tell the user: "Tip: Run `/neo-scan` to generate a conventions doc for consistent implementation."
7. Implement following the story's Implementation Steps.
8. Write tests per the story's Testing Requirements.
9. Run the build and test suite.
10. Run the **Acceptance Gate** (see `.github/agents/neo-dev.agent.md`): re-read the story file, verify each acceptance criterion is satisfied by code and tests, check off `- [ ]` to `- [x]` for each verified criterion and DoD item in the story file. If any criterion is unmet, fix it before proceeding.
11. Run the **Post-Story Sanity Check** (see `.github/agents/neo-dev.agent.md`): check for leftover artifacts, unused imports, naming consistency, scope creep, and test quality. Fix any issues found.
12. Update the story index to mark this story as DONE.
13. If `artifacts/STATE.md` exists, update it with a Context Handoff block:
    ```
    ## Context Handoff
    **Date:** {today}
    **Story completed:** {story number and title}
    **Steps completed:** {list of implementation steps finished}
    **Steps remaining:** {list of steps not yet done, if story is partial}
    **Next action:** {exact command or step to resume}
    **Notes:** {any decisions made, blockers hit, or context the next session needs}
    ```
14. Report what was built, then run the **Wave Orchestration** logic below.

If the story has more than 3 implementation steps or touches more than 5 files, suggest: "This story is complex. Consider implementing in a fresh context to maximize token budget."

If the story file contains unclear or conflicting requirements, stop and ask the user.

## Wave Orchestration

After completing a story and reporting it, determine what to do next by re-reading the story index:

### Case 1 — More TODO stories in the current wave

If the current wave has remaining TODO stories:

- **Single remaining story:** Auto-continue — go back to step 4 with the next story. No user prompt needed. Announce:
  ```
  Continuing with Story {N}: {Title} (same wave, no dependencies to wait for).
  ```
- **Multiple remaining stories:** These can be built in parallel. Offer the user a choice:
  ```
  Wave {W} has {N} more TODO stories that can be built in parallel:
  - Story {A}: {Title}
  - Story {B}: {Title}

  Options:
  1. Continue sequentially — I'll implement them one by one.
  2. Parallel sessions — I'll implement Story {A} now. Use the prompt below
     in a separate session for Story {B}:

     > Implement story {B} for {feature}. Read the story file at
     > `artifacts/stories/{feature}-{B}.md` and conventions from
     > `artifacts/architecture/conventions.md`. Follow the story's
     > Implementation Steps. When done, run the Acceptance Gate and
     > Post-Story Sanity Check, then mark it DONE in the story index.
  ```
  If the user chooses sequential, auto-continue to the next story without prompting again for subsequent stories in the wave.

### Case 2 — Current wave complete, next wave exists

All stories in the current wave are DONE. Check if the next wave's dependencies are satisfied (all depended-on stories are DONE).

- **Next wave has 1 story:** Auto-continue to it.
- **Next wave has multiple stories:** Present parallel session prompts for each, as in Case 1. Auto-continue with the first story.

Announce wave transitions:
```
Wave {W} complete ✓ — moving to Wave {W+1} ({N} stories).
```

### Case 3 — All stories DONE

All waves are complete. Announce:
```
All {N} stories implemented ✓

Next steps:
- Run `/neo-verify` for final checks
- Then `/neo-ship` to prepare the PR
```

Update `artifacts/STATE.md` to reflect all stories complete.
