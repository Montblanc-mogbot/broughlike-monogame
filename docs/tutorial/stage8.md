# Broughlike tutorial - Stage 8

Source: https://nluqo.github.io/broughlike-tutorial/stage8.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Animation, Screenshake, & Sounds](stage7.html)

# Stage 8 - Spells

If you try playing the game a bunch you might notice it's weighted a bit in the monster's favor. The player bump attack is limiting and not especially exciting. In my opinion, what really makes a broughlike special is your abilities or spells. You can see this especially in 868-HACK and Cinco Paus. Proper use of spells is what adds most of the depth and personality.



There's 15 spells in this section to demonstrate the diversity of what can be done with very little code. What I want to stress is: it's your game now. Do all 15 if you want, or pick and choose a few, or go off and implement something completely different. It's up to you.



First, let's write a single spell to give you an idea of what that looks like. Then we'll knock out the framework for casting spells and lastly I'll show you the rest of the 15 spells one by one.

**spell.js**

```js
spells = {
    WOOP: function(){
        player.move(randomPassableTile());
    }
};
```

We're going to store our spells in an object literal called, unsurprisingly,

`spells`

.



`WOOP`

warps the player to a random passable tile. This

`WOOP`

function has a one line body. Pretty simple huh?



Notice the function operates on the

`player`

object. All of our spells will be player-cast, but it's not hard at all to have spells castable by either players or monsters by passing in a "caster" entity and applying everything to that. I'll leave that as an exercise for the reader.



We're going to let the player select spells with the number keys 1-9.

**index.html**

```js
    document.querySelector("html").onkeypress = function(e){
        if(gameState == "title"){                              
            startGame();                
        }else if(gameState == "dead"){                             
            showTitle();                                        
        }else if(gameState == "running"){             
            if(e.key=="w") player.tryMove(0, -1);
            if(e.key=="s") player.tryMove(0, 1);
            if(e.key=="a") player.tryMove(-1, 0);
            if(e.key=="d") player.tryMove(1, 0);
```

```js
            if(e.key>=1 && e.key<=9) player.castSpell(e.key-1);
```

```js
        };
    };
```

Here's an interesting example of JavaScript's type coercion. The value

`e.key`

will come in as a string like "2".



Is the string "2" greater than or equal to 1? Strictly speaking, that doesn't make any sense, but JavaScript will try to make it work anyway. Your "2" will be coerced into the number 2 and then you

can

try comparing it with 1.



So if the pressed key is 1-9, we're passing that key number minus 1 (type coercion comes into play yet again) to a new function called

`castSpell`

. That number will represent an index into our array of spells. We're subtracting 1 because array indices start at 0 instead of 1.

## Spell framework

Now we'll add the code to load the player spells, add a new spell, and cast them. The player will initially start out with a single spell and will gain additional spell slots through acquiring treasure.



First let's initialize

`numSpells`

so that the player starts out with a single spell.

**game.js**

```js
function startGame(){                                           
    level = 1;
    score = 0;
```

```js
    numSpells = 1;
```

```js
    startLevel(startingHp);

    gameState = "running";
}
```

Then the bulk of the code to handle adding/casting spells:

**monster.js**

```js
class Player extends Monster{
    constructor(tile){
        super(tile, 0, 3);
        this.isPlayer = true;
        this.teleportCounter = 0;
```

```js
        this.spells = shuffle(Object.keys(spells)).splice(0,numSpells);
```

```js
    }

    tryMove(dx, dy){
        if(super.tryMove(dx,dy)){
            tick();
        }
    }
```

```js
    addSpell(){                                                       
        let newSpell = shuffle(Object.keys(spells))[0];
        this.spells.push(newSpell);
    }

    castSpell(index){                                                   
        let spellName = this.spells[index];
        if(spellName){
            delete this.spells[index];
            spells[spellName]();
            playSound("spell");
            tick();
        }
    }
```

```js
}
```

It's sometimes confusing to see different variables with the same name, so let's make sure we understand the difference. The global object

`spells`

holds the spell functions. The player spells (shown above as

`this.spells`

) is an array of spell

names

, which we can use to index into the global object. It's an inventory of sorts.



The new line in our player constructor does quite a bit...

- gets all the spell names using`Object.keys`

- shuffles them

- grabs some number of spells from that shuffled list using`splice`

- assigns them to`this.spells`

The method

`addSpells`

looks similar, but it only grabs one random spell and then adds it to the player spells array.



The method

`castSpell`

takes an index (remember the player pressing 1-9 earlier?) and tries to find that index in the player spells array. It may not exist, which is OK! That's why we do a check on the result.



If found, we

`delete`

the element and leave an empty array slot. We call the spell function, play our spell sound, and do a

`tick`

. You could also skip the tick if you don't want monsters to act after a spell is cast.

## Drawing spells

Now let's draw our spell list on the sidebar:

**game.js**

```js
function draw(){
    if(gameState == "running" || gameState == "dead"){
        ctx.clearRect(0,0,canvas.width,canvas.height);
```

```js
...
```

```js
        drawText("Level: "+level, 30, false, 40, "violet");
        drawText("Score: "+score, 30, false, 70, "violet");
```

```js
        for(let i=0; i<player.spells.length; i++){
            let spellText = (i+1) + ") " + (player.spells[i] || "");                        
            drawText(spellText, 20, false, 110+i*40, "aqua");        
        }
```

```js
    }
```

```js
}
```

If your recall the way we used

`drawText`

before, this should be pretty straightforward... with the exception of the expression:



`(i+1) + ") " + (player.spells[i] || "")`



We're adding 1 back to our spell index to make it like normal human counting, adding a parentheses and space, and then adding the spell name.



If the spell has been deleted, we want to handle that with the "OR" operator

`||`

and instead simply add an empty string. You don't need to know all the details of how

`||`

works here (some more type coercion is involved), but I will say it's kind of like the English word "or". Do the first thing

or

, if that doesn't work, do the second thing. The result with our first spell should be:



![woop](https://nluqo.github.io/broughlike-tutorial/screens/woop.png)

Test out that code and you should see the above

and

you should be able to cast your first spell.

## Gaining new spells

Let's connect spells to treasure.

**tile.js**

```js
class Floor extends Tile{
    constructor(x,y){
        super(x, y, 2, true);
    };

    stepOn(monster){        
        if(monster.isPlayer && this.treasure){   
            score++;
```

```js
            if(score % 3 == 0 && numSpells < 9){                         
                numSpells++;                
                player.addSpell();            
            }
```

```js
            playSound("treasure");           
            this.treasure = false;
            spawnMonster();
        }
    }
}
```

Every 3 treasures acquired results in a new spell all the way up to 9 slots.



So that's the framework in a nutshell.

## Spell 2: QUAKE

For this spell only, I'll show the surrounding code. But all the spell functions will be added as properties of the

`spells`

object in the same way: add a comma after the last function, break to a new line, then add the new function.



Later, if things start breaking the first thing you should check is your commas!

**spell.js**

```js
spells = {
    WOOP: function(){
        player.move(randomPassableTile());
```

```js
    }
```

```js
    },
    QUAKE: function(){                  
        for(let i=0; i<numTiles; i++){
            for(let j=0; j<numTiles; j++){
                let tile = getTile(i,j);
                if(tile.monster){
                    let numWalls = 4 - tile.getAdjacentPassableNeighbors().length;
                    tile.monster.hit(numWalls*2);
                }
            }
        }
        shakeAmount = 20;
    }
```

```js
};
```

`QUAKE`

iterates over each tile and, if a monster is present, deals it 2 damage for each adjacent wall. We're reusing the screenshake and it's way more satisfying here.



One note on testing. You might want to set

`numSpells`

initially to 9 while testing so you have easier access to the available spells on the first level.

## Spell 3: MAELSTROM

**spell.js**

```js
    MAELSTROM: function(){
        for(let i=0;i<monsters.length;i++){
            monsters[i].move(randomPassableTile());
            monsters[i].teleportCounter = 2;
        }
    }
```

`MAELSTROM`

iterates over all monsters and teleports them to a random tile just like

`WOOP`

did for the player. Then it sets the monster teleport counter, so we get the same behavior as when monsters spawn in the first time.

## Spell 4: MULLIGAN

**spell.js**

```js
    MULLIGAN: function(){
        startLevel(1, player.spells);
    }
```

**game.js**

```js
function startLevel(playerHp){
```

```js
function startLevel(playerHp, playerSpells){
```

```js
    spawnRate = 15;              
    spawnCounter = spawnRate;      

    generateLevel();

    player = new Player(randomPassableTile());
    player.hp = playerHp;
```

```js
    if(playerSpells){
        player.spells = playerSpells;
    }
```

```js
    randomPassableTile().replace(Exit);
}
```

`MULLIGAN`

resets the level without increasing the level count or resetting the player's spells and sets the player's HP to 1. This is the first of many spells that can be used to farm treasure.

## Spell 5: AURA

**spell.js**

```js
    AURA: function(){
        player.tile.getAdjacentNeighbors().forEach(function(t){
            t.setEffect(13);
            if(t.monster){
                t.monster.heal(1);
            }
        });
        player.tile.setEffect(13);
        player.heal(1);
    }
```

`AURA`

heals both the player and any adjacent monsters. It also uses a new tile method

`setEffect`

which will add a short lived sprite to that tile.

## Effects

We're going to draw 4 sprites for effects.Our heal effect is just a few bright green circles with a couple scattered green pixels.Image: https://nluqo.github.io/broughlike-tutorial/art/heal.pngExplosion effects are usually made by drawing concentric bubbly shapes that are white, yellow, orange, red, and black.Image: https://nluqo.github.io/broughlike-tutorial/art/boom.pngWe make a bolt effect by drawing a wiggly line, outlining it in white, and then doing some basic antialiasing to smooth it out (simply drawing a midpoint color between the two in a few spots).Image: https://nluqo.github.io/broughlike-tutorial/art/bolt-horizontal.pngRotate 90 degrees to make the same effect in a vertical orientation.Image: https://nluqo.github.io/broughlike-tutorial/art/bolt-vertical.png

Now the code to draw effects.

**tile.js**

```js
class Tile{
    constructor(x, y, sprite, passable){
        this.x = x;
        this.y = y;
        this.sprite = sprite;
        this.passable = passable;
    }
```

```js
...
```

```js
    draw(){
        drawSprite(this.sprite, this.x, this.y);

        if(this.treasure){                      
            drawSprite(12, this.x, this.y);                                             
        }
```

```js
        if(this.effectCounter){                    
            this.effectCounter--;
            ctx.globalAlpha = this.effectCounter/30;
            drawSprite(this.effect, this.x, this.y);
            ctx.globalAlpha = 1;
        }
```

```js
    }
```

```js
    setEffect(effectSprite){                                  
        this.effect = effectSprite;
        this.effectCounter = 30;
    }
```

```js
}
```

In

`setEffect`

, we simply save the passed in

`effectSprite`

for later use. And we set

`effectCounter`

to 30, which determines the length of the effect in frames (30 frames should be about half a second).



As long as

`effectCounter`

is greater than 0, we decrement its value, and then draw the effect sprite.



Now here's something we haven't seen before:

`ctx.globalAlpha`

. This lets us control the transparency of all sprites drawn. A value of 1 means fully opaque and 0 means fully transparent. By setting it to

`this.effectCounter/30`

we ensure that the effect fades out. We reset the global alpha to 1 at the end to avoid affecting any other sprites.

## Spell 6: DASH

**spell.js**

```js
    DASH: function(){
        let newTile = player.tile;
        while(true){
            let testTile = newTile.getNeighbor(player.lastMove[0],player.lastMove[1]);
            if(testTile.passable && !testTile.monster){
                newTile = testTile;
            }else{
                break;
            }
        }
        if(player.tile != newTile){
            player.move(newTile);
            newTile.getAdjacentNeighbors().forEach(t => {
                if(t.monster){
                    t.setEffect(14);
                    t.monster.stunned = true;
                    t.monster.hit(1);
                }
            });
        }
    }
```

`DASH`

moves the player in the direction of their last move or attack until they are blocked by a wall or monster. If the player was able to move, adjacent monsters are damaged, they are stunned, and an effect is drawn.



We're using

`getNeighbor`

to move in the

`lastMove`

direction (defined below) one tile at a time in a

`while`

loop. It's a common approach to use

`while(true)`

until you meet some condition and then

`break`

out of the loop. We'll use it again later. Just be careful when writing such a loop that you have a proper way to break out; otherwise you can easily end up in an infinte loop and crash your browser!



Here's the code to track

`lastMove`

.

**monster.js**

```js
class Monster{
    constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
        this.teleportCounter = 2;
        this.offsetX = 0;                                                   
        this.offsetY = 0;
```

```js
        this.lastMove = [-1,0];
```

```js
    }
```

```js
...
```

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
```

```js
            this.lastMove = [dx,dy];
```

```js
...
```

```js
        }
    }
```

## Spell 7: DIG

**spell.js**

```js
    DIG: function(){
        for(let i=1;i<numTiles-1;i++){
            for(let j=1;j<numTiles-1;j++){
                let tile = getTile(i,j);
                if(!tile.passable){
                    tile.replace(Floor);
                }
            }
        }
        player.tile.setEffect(13);
        player.heal(2);
    }
```

`DIG`

replaces all walls (not including the outer wall) with floors. The player is healed for 2 health and an effect is drawn on the player tile.

## Spell 8: KINGMAKER

**spell.js**

```js
    KINGMAKER: function(){
        for(let i=0;i<monsters.length;i++){
            monsters[i].heal(1);
            monsters[i].tile.treasure = true;
        }
    }
```

`KINGMAKER`

heals all monsters and generates a treasure on their tile.

## Spell 9: ALCHEMY

**spell.js**

```js
    ALCHEMY: function(){
        player.tile.getAdjacentNeighbors().forEach(function(t){
            if(!t.passable && inBounds(t.x, t.y)){
                t.replace(Floor).treasure = true;
            }
        });
    }
```

`ALCHEMY`

turns all adjacent walls that are not part of the outer wall into floors with a teasure.

## Spell 10: POWER

**spell.js**

```js
    POWER: function(){
        player.bonusAttack=5;
    }
```

**monster.js**

```js
class Monster{
    constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
        this.teleportCounter = 2;
        this.offsetX = 0;                                                   
        this.offsetY = 0;     
        this.lastMove = [-1,0];
```

```js
        this.bonusAttack = 0;
```

```js
    }
```

```js
...
```

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            this.lastMove = [dx,dy];
            if(!newTile.monster){
                this.move(newTile);
            }else{
                if(this.isPlayer != newTile.monster.isPlayer){
                    this.attackedThisTurn = true;
                    newTile.monster.stunned = true;
```

```js
                    newTile.monster.hit(1);
```

```js
                    newTile.monster.hit(1 + this.bonusAttack);
                    this.bonusAttack = 0;
```

```js
                    shakeAmount = 5;

                    this.offsetX = (newTile.x - this.tile.x)/2;
                    this.offsetY = (newTile.y - this.tile.y)/2;
                }
            }
            return true;
        }
    }
```

`POWER`

makes the next player attack do 6 damage by using a new variable called

`bonusAttack`

.

## Spell 11: BUBBLE

**spell.js**

```js
    BUBBLE: function(){
        for(let i=player.spells.length-1;i>0;i--){
            if(!player.spells[i]){
                player.spells[i] = player.spells[i-1];
            }
        }
    }
```

`BUBBLE`

duplicates spells. It iterates over the player spells in reverse and copies a spell from the previous element if the current element is empty.

## Spell 12: BRAVERY

**spell.js**

```js
    BRAVERY: function(){
        player.shield = 2;
        for(let i=0;i<monsters.length;i++){
            monsters[i].stunned = true;
        }
    }
```

`BRAVERY`

gives the player a free turn by iterating over all monsters and stunning them.



It also adds a new property called

`shield`

, which will prevent any damage until the turn after next. To support the

`shield`

property, we need to do three things:

- prevent the player from taking damage by returning early in the`hit`method if the`shield`is greater than 0

- adding a player version of`update`that decrements the`shield`every turn

- calling that`update`method

The reason we have two distinct versions of

`update`

is that player will never need regular monster AI behavior, but we still need a place to update player variables once per turn.

**monster.js**

```js
    hit(damage){
```

```js
        if(this.shield>0){           
            return;                                                             
        }
```

```js
        this.hp -= damage;                                           
        if(this.hp <= 0){
            this.die();
        }

        if(this.isPlayer){                                                     
            playSound("hit1");                                              
        }else{                                                       
            playSound("hit2");                                              
        }      
    }
```

```js
...
```

```js
class Player extends Monster{
   constructor(tile){
       super(tile, 0, 3);
       this.isPlayer = true;
       this.teleportCounter = 0;
       this.spells = shuffle(Object.keys(spells)).splice(0,numSpells);
   }
```

```js
    update(){          
        this.shield--;                                                      
    }
```

**game.js**

```js
function tick(){
    for(let k=monsters.length-1;k>=0;k--){
        if(!monsters[k].dead){
            monsters[k].update();
        }else{
            monsters.splice(k,1);
        }
    }
```

```js
    player.update();
```

```js
    if(player.dead){    
        addScore(score, false);
        gameState = "dead";
    }

    spawnCounter--;                           
    if(spawnCounter <= 0){                     
        spawnMonster();
        spawnCounter = spawnRate;
        spawnRate--;
    }
}
```

## Bolt travel

The last three spells use a function that we'll add to the end of spell.js called

`boltTravel`

.

**spell.js**

```js
spells = {
```

```js
...
```

```js
};
```

```js
function boltTravel(direction, effect, damage){
    let newTile = player.tile;
    while(true){
        let testTile = newTile.getNeighbor(direction[0], direction[1]);
        if(testTile.passable){
            newTile = testTile;
            if(newTile.monster){
                newTile.monster.hit(damage);
            }
            newTile.setEffect(effect);
        }else{
            break;
        }
    }
}
```

This function is sort of like

`DASH`

earlier. We pass in a specified

`direction`

, an

`effect`

sprite, and some

`damage`

number. Starting from the player tile, we move in that direction until we hit a wall. We draw an effect for each tile passed through and for each monster, we damage it.



We can do a lot with

`boltTravel`

.

## Spell 13: BOLT

**spell.js**

```js
    BOLT: function(){
        boltTravel(player.lastMove, 15 + Math.abs(player.lastMove[1]), 4);
    }
```

First, a simple test of our new function with the aptly named

`BOLT`

. Like

`DASH`

earlier, this spell operates in the direction of the player's last move. It's somewhat hard to pull off, so we make it do 4 damage!



The effect expression might look a little bit weird, but all we're trying to accomplish is to return either 15 or 16 (15 is the location of our horizontal bolt sprite, 16 holds the vertical version). A horizontal last move will have 0 in

`player.lastMove[1]`

, but a vertical move will have either -1 or +1. Taking the absolute value gives us 1 in either case.

## Spell 14: CROSS

**spell.js**

```js
    CROSS: function(){
        let directions = [
            [0, -1],
            [0, 1],
            [-1, 0],
            [1, 0]
        ];
        for(let k=0;k<directions.length;k++){
            boltTravel(directions[k], 15 + Math.abs(directions[k][1]), 2);
        }
    }
```

`CROSS`

also calls

`boltTravel`

but in the 4 cardinal directions. We define these

`directions`

in an array literal and iterate over it. We deal 2 damage and we use the same trick as last time to distinguish between horizontal and vertical bolts.

## Spell 15: EX

**spell.js**

```js
    EX: function(){
        let directions = [
            [-1, -1],
            [-1, 1],
            [1, -1],
            [1, 1]
        ];
        for(let k=0;k<directions.length;k++){
            boltTravel(directions[k], 14, 3);
        }
    }
```

`EX`

is pretty much the same as

`CROSS`

, but in diagonal directions. We deal 3 damage and pass a single sprite that works for any direction.

## The End

And with that, our spells are done. The whole thing is done. You've made a complete game in less than a thousand lines of code with no frameworks. Great job!

![casting](https://nluqo.github.io/broughlike-tutorial/screens/casting.gif)

So...

what's next?
