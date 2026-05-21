# 06. Build Order for the Actual Game

This is the practical production order for building the real game from the current toolkit state.

It is not a lore outline.
It is not a dream roadmap.
It is the recommended sequence for making playable, testable progress without outrunning the framework.

---

## Core principle

Build in this order:

1. **one stable loop**
2. **one richer loop**
3. **repeatable content patterns**
4. **only then broader content volume**

Do not try to author lots of game areas before the loop shape is trustworthy.

---

## Stage 1 — Lock the first real loop

Goal: make the current game structure feel intentional and replayable.

### Target shape

- `apartment-intro`
- `hub-start` (`Court Offices` / `First Antechamber`)
- `tutorial`
- `hub-success` (`Records Stair`)
- `hub-failure` (`Waiting Gallery`)

### What to add next in this stage

- at least one more meaningful authored interaction in each hub if needed
- clear text identity for why success and failure lead to different rooms
- one honest “play again / continue deeper” decision point after return

### Exit condition for Stage 1

Stage 1 is done when:

- the loop is understandable in play
- each hub state feels authored rather than placeholder
- return routing and `currentStart` are reliable
- the loop is well-covered by `GameplayCheck`

---

## Stage 2 — Add the first true objective-driven dungeon slice

Goal: stop relying only on “reach exit” as the meaning of a run.

### Recommended next content

Build one dungeon where the run has a clearer authored purpose, such as:

- retrieve a file
- recover a stamp/seal
- find a clerk/room/key item
- reach a named chamber

### Why this stage matters

This is the point where the game stops feeling like a tutorial dungeon wearing new clothes.

### Framework implication

If the current exit/item model can support the objective cleanly, use it.
If not, that is the signal to add the smallest objective hook.

### Exit condition for Stage 2

Done when one dungeon has:

- a named objective
- a meaningful success state
- a meaningful failure state
- different return destinations or return messaging

---

## Stage 3 — Establish 3-5 canonical reusable content patterns

Goal: prove that game content can be produced repeatedly, not heroically.

### Recommended patterns to establish

1. **hub variant**
2. **generated objective dungeon**
3. **authored NPC/prop room**
4. **item-grant interaction**
5. **inventory-gated outcome route**

These do not need to be big.
They need to be copyable.

### Why this stage matters

Once these patterns are stable, actual game production becomes mostly composition instead of reinvention.

### Exit condition for Stage 3

Done when a new content slice can mostly be made by copying an existing package/doc pattern and changing the authored details.

---

## Stage 4 — Expand dungeon ecology

Goal: make dungeon identity feel distinct.

### Add next

- richer spawn/ecology authoring
- more distinct enemy pools per dungeon
- more distinct item pools per dungeon
- special rooms / side rooms / event rooms

### Constraint

Do not expand this layer until Stage 2 proved what a run is actually about.
A rich ecology on top of vague objectives is expensive drift.

### Exit condition for Stage 4

Done when different dungeon packages feel mechanically and tonally different without engine surgery.

---

## Stage 5 — Expand hub and narrative tooling only when content demands it

Goal: add finer-grained state only when the authored game truly needs it.

### Add this only if needed

- same hub mutates internally instead of routing to another variant
- same NPC changes dialogue across states without moving hubs
- doors/portals unlock inside one hub state
- multi-step event scripting becomes common

### Principle

Prefer:
- route-selected hub variants first
- internal hub mutation second

This keeps the system simpler for longer.

### Exit condition for Stage 5

Done when a concrete content need forces a more granular hub/event system, and the new seam is introduced to support that need directly.

---

## Stage 6 — Combat and strange-content expansion

Goal: support the more distinctive PMD/Kafka content later in production.

### Add when needed

- broader ability/effect framework
- reusable enemy behavior patterns
- hazards/statuses/summons
- lightweight cutscene/event sequencing

### Why it is later

This is powerful, but it should be driven by specific content that cannot be built cleanly with the current systems.

### Exit condition for Stage 6

Done when new weird enemies, hazards, and surreal beats can be authored without giant bespoke branches.

---

## Recommended next 10 concrete slices

If the goal is to keep moving productively, this is the order I recommend right now:

1. add one more meaningful interaction/decision in the current return hubs
2. add one objective-driven dungeon variant beyond the plain tutorial loop
3. route that dungeon to distinct success/failure hubs
4. add one authored special room or NPC room to that dungeon flow
5. add one new item whose purpose is specific to that loop
6. add one new enemy that reinforces that dungeon’s identity
7. add one more hub variant or return state if the loop wants it
8. add the smallest objective hook the content truly needs
9. add the smallest special-room injection seam the content truly needs
10. only then start increasing content volume

---

## What not to do yet

Avoid doing these too early:

- large-scale art polish
- lots of new empty hubs
- many new dungeons without distinct purposes
- giant story scripting systems before the need is real
- complex persistent hub-state mutation before route-selected variants stop being enough

---

## Simple decision rule

When choosing the next task, ask:

**Does this make the playable loop clearer, richer, or easier to extend?**

If yes, it is probably a good next slice.
If not, it is probably premature.
