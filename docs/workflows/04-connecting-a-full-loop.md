# 04. Connecting a Full Loop

Use this workflow when turning disconnected content into a real game loop.

A full loop means:

1. player starts in an authored hub or intro state
2. player enters a dungeon
3. dungeon outcome sends the player somewhere intentional
4. the next boot/start location is correct

## Current recommended loop shape

For now, prefer this pattern:

- authored intro or hub
- one dungeon package
- success hub variant
- failure hub variant
- `currentStart` updated by outcome routes

This is enough to make real progress without building a huge state-mutation system first.

## Step-by-step

1. **Pick the entry point**
   Decide whether the player begins in:
   - `apartment-intro`
   - a hub variant
   - a special authored start state

2. **Define the dungeon handoff**
   The entry space should make the onward route clear.

3. **Define the outcome hubs**
   For each run result, choose:
   - destination dungeon id
   - destination floor number
   - label/banner text
   - whether it sets `currentStart`

4. **Persist the checkpoint intentionally**
   If the result should become the next default start, mark the route with `SetsCurrentStart: true`.

5. **Make the return space playable, not symbolic**
   Each return hub should have:
   - a readable name
   - at least one authored interaction
   - one clear onward route

6. **Test the whole path**
   At minimum prove:
   - the player reaches the expected hub after success/failure
   - `currentStart` is the expected hub after persistence sync
   - the hub can route back into the next run

## Recommended current style

Prefer:
- different hub variants for different outcomes
- minimal permanent state
- explicit route-selected progression

Only add finer-grained hub mutation when one hub really needs to change internally instead of routing to another variant.
