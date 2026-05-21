# 01. Planning a Content Slice

Before writing code or content, decide what kind of slice you are building.

## Rule of thumb

Every slice should answer all four of these questions:

1. **Where does the player start?**
2. **What is the player trying to do?**
3. **What outcome changes where they go next?**
4. **What is the smallest proof that this slice works?**

If those answers are vague, the slice is too big.

## Good slice shapes

### A hub slice

Use when you are adding:

- a new authored landing space
- a new return state after success/failure
- NPC text or prop interactions
- a portal choice area

Expected proof:
- player can boot or route into the hub
- the hub has authored interactions
- the hub has at least one valid onward route

### A dungeon slice

Use when you are adding:

- a new generated dungeon package
- a new authored dungeon floor
- a new objective or route condition
- a new ecology theme

Expected proof:
- player can enter the dungeon
- the dungeon runs through its intended loop
- the dungeon exits to the intended destination

### A loop slice

Use when you are adding:

- a complete hub -> dungeon -> return cycle
- a success/failure branch
- a progression checkpoint change

Expected proof:
- all destinations are reachable
- outcome routing is correct
- `WorldState.currentStart` ends up where you expect

## Decide the minimum content contract first

Write down:

- dungeon ids involved
- floor ids involved
- items required
- exits required
- success destination
- failure destination
- any persistent checkpoint change

If you cannot name those concretely, do not start implementation yet.

## Keep slices small

Good:
- “Add a post-boss failure hub variant and route empty-handed returns there.”
- “Add one authored NPC room to the court offices hub.”
- “Add one item-grant prop that unlocks a route.”

Too big:
- “Build the whole court system.”
- “Implement story progression.”
- “Make the dungeon ecosystem rich.”

## Pre-implementation checklist

Before you start, confirm:

- there is one obvious starting dungeon id
- there is one obvious destination per outcome
- current content seams can probably support the slice
- you know which `GameplayCheck` should prove it
- you know which existing package is the best copy-from-here reference
