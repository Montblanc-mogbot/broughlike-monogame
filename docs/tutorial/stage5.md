# Broughlike tutorial - Stage 5

Source: https://nluqo.github.io/broughlike-tutorial/stage5.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Monsters - Part 2](stage4.html)

# Stage 5: Game Lifecycle: drawing text, title screen, failure condition, and moving between levels

Currently, when monsters are killed they are immediately deleted from the game. That's great, but there is no code to handle

player

death. Let's tackle that now.

## Game state

We'll add the concept of a

`gameState`

so we can model the game as a

finite state machine

with four states.



Our four states:

- **loading**: waiting for the assets to load

- **title**: on the title screen

- **running**: playing the game actively

- **dead**: the moment after the player has died, but before returning to the title screen

"Finite state machine" sounds super complicated. It's not.

![state-machine](https://nluqo.github.io/broughlike-tutorial/screens/state-machine.png)

Just think about a title screen. That's a state. Then when the

actual game

is running, that's a state. Clearly different things are displayed on the title screen versus the game proper. And pressing a specific button in-game and pressing it on the title screen do different things. So that's all we're trying to accomplish.



To make this work, we need to only do two things.

- When needed, switch states simply by setting`gameState`to a different state name

- Use conditionals checking the value of`gameState`to wrap behavior that should only occur in specific states

**index.html**

```js
<script> 
    tileSize = 64;
    numTiles = 11;
    uiWidth = 4;
    level = 1;
    maxHp = 6;

    spritesheet = new Image();
    spritesheet.src = 'spritesheet.png';
```

```js
    spritesheet.onload = showTitle;
                             
    gameState = "loading";  

    startingHp = 3; 
    numLevels = 6;
```

```js
    document.querySelector("html").onkeypress = function(e){
```

```js
        if(gameState == "title"){                              
            startGame();                
        }else if(gameState == "dead"){                             
            showTitle();                                        
        }else if(gameState == "running"){
```

```js
            if(e.key=="w") player.tryMove(0,-1);
            if(e.key=="s") player.tryMove(0,1);
            if(e.key=="a") player.tryMove(-1, 0);
            if(e.key=="d") player.tryMove(1, 0);
```

```js
        }
```

```js
    };

    setInterval(draw, 20);

    setupCanvas();
```

```js
    generateLevel();                

    let player = new Player(randomPassableTile());
```

```js
  
<\/script>
```

When our spritesheet image loads, we want to switch to showing the title screen which is accomplished by assigning a function to

`spritesheet.onload`

(we'll write that

`showTitle`

function below next). But what's our first state? Unsurprisingly, we're initializing

`gameState`

to "loading" because that's the first thing that happens.



While we're here, we're adding some variables related to the game lifecycle:

`startingHp`

and

`numLevels`

.



Within the

`onkeypress`

handler, you can see where we're starting to add transitions between game states when buttons are pressed and also restricting when gameplay actions can take place (only when "running").



Lastly, we're moving out some code related to setting the

`player`

location and generating a level. If we're going to have multiple levels, it no longer makes sense to do these things only once.



There are several small changes to add to game.js:

**game.js**

```js
function draw(){
```

```js
    if(gameState == "running" || gameState == "dead"){
```

```js
        ctx.clearRect(0,0,canvas.width,canvas.height);

        for(let i=0;i<numTiles;i++){
            for(let j=0;j<numTiles;j++){
                getTile(i,j).draw();
            }
        }


        for(let i=0;i<monsters.length;i++){
            monsters[i].draw();
        }

        player.draw();
```

```js
    }
```

```js
}
```

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
    if(player.dead){    
        gameState = "dead";
    }
```

```js
}
```

```js
function showTitle(){                                          
    ctx.fillStyle = 'rgba(0,0,0,.75)';
    ctx.fillRect(0,0,canvas.width, canvas.height);

    gameState = "title";
}

function startGame(){                                           
    level = 1;
    startLevel(startingHp);

    gameState = "running";
}

function startLevel(playerHp){                          
    generateLevel();

    player = new Player(randomPassableTile());
    player.hp = playerHp;
}
```

In

`draw`

, we restrict our draw operations to when the game is running OR when the player just died.



In

`tick`

, we change to the "dead" state when we detect that the player has died.



In

`showTitle`

, we draw a semi-transparent black background, which will function as our title screen for now. Then we change to the "title" state.



In

`startGame`

, we jump to the first floor, call

`startLevel`

, and we change to the "running" state.



In

`startLevel`

, we move over the code that we took out of index.html. Also

`player`

gets initalized with

`hp`

equal to the value passed in

`playerHp`

. This value starts as 3, but in a little bit we'll use this function to persist player HP across levels.



Now load up the game. You'll see the "title screen", a black box for now. When you hit a key, the game will start running. And if you were to die, everything would freeze until you pressed another key... leading back to the title screen, this time overlaid semitransparently over the game. That's the whole state machine right there working!

## Spawning more monsters

It's not easy to die in this game. No monsters spawn after the first two. Let's make it harder.

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
    if(player.dead){    
        gameState = "dead";
    }
```

```js
    spawnCounter--;
    if(spawnCounter <= 0){  
        spawnMonster();
        spawnCounter = spawnRate;
        spawnRate--;
    }
```

```js
}
```

```js
...
```

```js
function startLevel(playerHp){
```

```js
    spawnRate = 15;              
    spawnCounter = spawnRate;
```

```js
    generateLevel();

    player = new Player(randomPassableTile());
    player.hp = playerHp;
}
```

We're using two new global variables.

- `spawnCounter`counts down until each spawn and then resets

- `spawnRate`is how often monsters will get spawned and provides the initial value for`spawnCounter`every time it is reset

After every spawn,

`spawnRate`

is decremented so that monsters come out even faster.

Tweaking the initial value of

`spawnRate`

will have a massive impact on the game's design (try a value of 1 for some real fun).



Try that out and see how you like the spawn behavior.

## Teleport counter

One more rough edge to smooth out. Monsters can spawn right next to us and immediately start attacking. It's also very weird to see monsters spawning in the middle of the map with no explanation.



One convenient explanation is that these monsters are being "teleported" into the map. While each monster is teleporting, which will take one extra turn, they will be unable to do anything. Think of this as equivalent to Magic's "summoning sickness".



To visually demonstrate this, we need a teleporting sprite. Let's draw that before getting to the code.

## Drawing the teleport sprite

There's not much to this one. We draw a spiral, add swirly limbs, and outline it.Image: https://nluqo.github.io/broughlike-tutorial/art/teleport.png

To integrate this teleporting thing, we'll use a counter just like we did with

`spawnCounter`

. This should look familiar. You can do a

hell of a lot of work

with only boolean flags for statuses and integer counters for longer lasting effects.

**monster.js**

```js
class Monster{
    constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
```

```js
        this.teleportCounter = 2;
```

```js
    }                                                                           

    heal(damage){
        this.hp = Math.min(maxHp, this.hp+damage);
    }

    update(){
```

```js
        this.teleportCounter--;
```

```js
        if(this.stunned){
```

```js
        if(this.stunned || this.teleportCounter > 0){
```

```js
            this.stunned = false;                                               
            return;
        }
        this.doStuff();
    }
```

```js
...
```

```js
    draw(){
```

```js
        if(this.teleportCounter > 0){                                        
            drawSprite(10, this.tile.x, this.tile.y);                     
        }else{
```

```js
            drawSprite(this.sprite, this.tile.x, this.tile.y);               
            this.drawHp();
```

```js
        }
```

```js
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
```

```js
        this.teleportCounter = 0;
```

```js
}
```

The monsters are teleporting in and spawning nicely... eventually leading to an unwillable scenario. Let's give the player an

exit

from this situation.

## Drawing the exit sprite

I tried drawing something resembling a portal.Image: https://nluqo.github.io/broughlike-tutorial/art/exit.png

Our

`Exit`

tile is a pretty basic object, but it does need to do something (i.e. go to the next level) whenever the player moves onto it. It's within the monster code that we determine when we're stepping on something, but after that point it's best to delegate to the tiles themselves. Each tile will know exactly what to do when stepped on.

**tile.js**

```js
class Floor extends Tile{
    constructor(x,y){
        super(x,y, 2, true);
    };
```

```js
    stepOn(monster){                                                           
        //TODO: complete
    }
```

```js
}

class Wall extends Tile{
   constructor(x, y){
       super(x, y, 3, false);
   }
}
```

```js
class Exit extends Tile{
    constructor(x, y){
        super(x, y, 11, true);
    }

    stepOn(monster){
        if(monster.isPlayer){
            if(level == numLevels){
                showTitle();
            }else{
                level++;
                startLevel(Math.min(maxHp, player.hp+1));
            }
        }
    }
}
```

Notice that we need an empty

`stepOn`

method for

`Floor`

too because monsters will be stepping on them as well. It won't do anything for now but it's nice to leave a little reminder comment to finish up the behavior later. We'll never need this method for a

`Wall`

because monsters don't step on those. And unlike in other languages we don't need this method on the base object in order to call it on the subclasses.



Now what happens when the player steps on the

`Exit`

? If we were already on the last level, we jump to the title screen. Otherwise, we start a new level with 1 extra HP.



Two more pieces to tie it all together: triggering

`stepOn`

and creating the

`Exit`

tile.

**monster.js**

```js
    move(tile){
        if(this.tile){
        this.tile.monster = null;
        }
        this.tile = tile;
        tile.monster = this;
```

```js
        tile.stepOn(this);
```

```js
    }
```

**game.js**

```js
function startLevel(playerHp){     
    spawnRate = 15;              
    spawnCounter = spawnRate;      

    generateLevel();

    player = new Player(randomPassableTile());
    player.hp = playerHp;
```

```js
    randomPassableTile().replace(Exit);
```

```js
}
```

With these small additions, the game has some real structure to it.

## Title time

The title screen is seriously lacking though. I don't want to do anything too crazy for the title screen, but at the very least it needs some text.



If you're trying to draw UI elements for a browser game, I would typically recommend trying it with HTML. You've got a lot of functionality out of the box with HTML & CSS, built up over decades. But for a few simple lines of text, the canvas will do just fine.



Drawing text on the canvas is not much different from drawing images. The important bit here is at the end:

`ctx.fillText`

. All we need to pass is what text to draw and where.

**game.js**

```js
function drawText(text, size, centered, textY, color){
    ctx.fillStyle = color;
    ctx.font = size + "px monospace";
    let textX;
    if(centered){
        textX = (canvas.width-ctx.measureText(text).width)/2;
    }else{
        textX = canvas.width-uiWidth*tileSize+25;
    }

    ctx.fillText(text, textX, textY);
}
```

The first two lines of our function set the font color and size. The string

`"px monospace"`

might look a little cryptic. We're actually writing a bit of CSS : we specify a font size in "px" units (pixels) and specify the type of font we want, which is "monospace".



We're going to leave the Y position up to the caller, but to make things easier this function will handle X. We're going to draw text in two places basically: justified centered on the title screen and on the far right (where the UI is) during play. The

`centered`

variable lets us toggle between these two.



So what does this line mean exactly?



`textX = (canvas.width-ctx.measureText(text).width)/2;`



When you want to center

any

element, whether on the canvas or in HTML, the calculation is the same. For X: half the container width minus half the content width. For Y: half the container height minus half the content height. The reason is obvious when you see it:



![centering](https://nluqo.github.io/broughlike-tutorial/screens/centering.png)



This is a good example of where HTML & CSS would be a lot easier to use! For one there are many easy ways to center things in CSS (there weren't for about 20 years but now there certainly are). And for two getting the width or height of an HTML element is very easy. None of this

`measureText`

nonsense. Did you know there isn't even a way to measure the height? Ugh!



Anyway, enough ranting... let's draw some text.

**game.js**

```js
function draw(){
    if(gameState == "running" || gameState == "dead"){
        ctx.clearRect(0,0,canvas.width,canvas.height);

        for(let i=0;i<numTiles;i++){
            for(let j=0;j<numTiles;j++){
                getTile(i,j).draw();
            }
        }


        for(let i=0;i<monsters.length;i++){
            monsters[i].draw();
        }

        player.draw();
```

```js
        drawText("Level: "+level, 30, false, 40, "violet");
```

```js
    }
```

```js
}
```

```js
...
```

```js
function showTitle(){                                          
    ctx.fillStyle = 'rgba(0,0,0,.75)';
    ctx.fillRect(0,0,canvas.width, canvas.height);

    gameState = "title";
```

```js
    drawText("SUPER", 40, true, canvas.height/2 - 110, "white");
    drawText("BROUGH BROS.", 70, true, canvas.height/2 - 50, "white");
```

```js
}
```

To review the arguments to our

`drawText`

function

- some text

- a font size

- whether the text is centered

- the Y position

- the color

Whew! That's a mouthful, but trust me when I say it's preventing us from duplicating a lot of code.



We're drawing the level number on the UI during play and drawing the name of the game on the title screen. I chose a silly name. I'm sure you can do better.



Try it out. I think it's looking good. At this point, the game can be considered a fully playable and complete game. It's just not very interesting yet. We'll get there.

![title-screen](https://nluqo.github.io/broughlike-tutorial/screens/title-screen.png)

In the

next section

, we'll be adding treasure and score mechanics.
