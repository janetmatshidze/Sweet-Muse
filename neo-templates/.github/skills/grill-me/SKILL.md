---
name: grill-me
description: Interview the user relentlessly about a plan or design until reaching shared understanding, resolving each branch of the decision tree. Use when user wants to stress-test a plan, get grilled on their design, or mentions "grill me".
---

Interview me relentlessly about every aspect of this plan until
we reach a shared understanding. Walk down each branch of the design
tree resolving dependencies between decisions one by one.

If a question can be answered by exploring the codebase, explore
the codebase instead.

For each question, provide your recommended answer.

Create the `artifacts/grill-me/` directory if it doesn't exist.

Document the shared understanding we reach in a markdown file in 
`artifacts/grill-me/` named `{topic-name}-Grill-Me-{N}.md`. If the 
topic has multiple branches, create a separate file for each branch 
(e.g. `{topic-name}-Grill-Me-1a.md`, `{topic-name}-Grill-Me-1b.md`, 
etc.). Create an index file at `artifacts/grill-me/{topic-name}-Grill-Me-index.md` 
that links to all the files for this topic and summarizes the shared 
understanding we reached.
