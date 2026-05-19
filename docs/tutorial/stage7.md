# Broughlike tutorial - Stage 7

Source: https://nluqo.github.io/broughlike-tutorial/stage7.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Treasure & Score](stage6.html)

# Stage 7 - Animation, Screenshake, & Sounds

Turn based movement without animation is not only a bit boring, but it also makes deciphering gameplay difficult. Monsters jump all over the screen and it's impossible to tell exactly what's happening.



Luckily, smoothing turn based movement is a piece of cake. When a monster moves from one tile to another, we start out by drawing them immediately in the new tile BUT with an

`offsetX`

and

`offsetY`

that represents the gap between the old and new position. So for the very first instant after movement, the offsets make the monster appear to be at their old tile, even though their "official" position is at their new tile.



Each frame, we reduce the offsets and in doing so, the sprites slide into place. When the offsets reach 0, sprites appear precisely at the their actual position.

![smoothmovement](https://nluqo.github.io/broughlike-tutorial/screens/smoothmovement.jpg)

**monster.js**

```js
class Monster{
    constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
        this.teleportCounter = 2;
```

```js
        this.offsetX = 0;                                                   
        this.offsetY = 0;
```

```js
    }
```

```js
...
```

```js
    move(tile){
        if(this.tile){
            this.tile.monster = null;
```

```js
            this.offsetX = this.tile.x - tile.x;    
            this.offsetY = this.tile.y - tile.y;
```

```js
        }
        this.tile = tile;
        tile.monster = this;
        tile.stepOn(this);
    }
```

We initialize the offsets to 0 and then when moving, we calculate them as the difference between the old tile and the new tile position. Say you move to the right 1 tile. You've moved +1 in the X direction, so this code will set the offset to -1. That way you'll start out being drawn to the LEFT of your new tile position (the opposite direction you moved in).



Let's start adding in those offsets.

**monster.js**

```js
    getDisplayX(){                     
        return this.tile.x + this.offsetX;
    }

    getDisplayY(){                                                                  
        return this.tile.y + this.offsetY;
    }
```

```js
    draw(){
        if(this.teleportCounter > 0){
```

```js
            drawSprite(10, this.tile.x, this.tile.y);
```

```js
            drawSprite(10, this.getDisplayX(),  this.getDisplayY());
```

```js
        }else{
```

```js
            drawSprite(this.sprite, this.tile.x, this.tile.y);
```

```js
            drawSprite(this.sprite, this.getDisplayX(),  this.getDisplayY());
```

```js
            this.drawHp();                                 
        }
```

```js
    }

    drawHp(){
        for(let i=0;i<this.hp;i++){
            drawSprite(
                9,
```

```js
                this.tile.x + (i%3)*(5/16),
                this.tile.y - Math.floor(i/3)*(5/16)
```

```js
                this.getDisplayX() + (i%3)*(5/16),   
                this.getDisplayY() - Math.floor(i/3)*(5/16)
```

```js
            );                                              
        }
    }
```

We're adding two new functions called

`getDisplayX`

and

`getDisplayY`

which will return a monster's

apparent

position. I say "apparent" because in a grid based game like this each monster is only ever positioned exactly on a specific tile for gameplay purposes. Once adding these wrapper functions, we just need to replace all the instances where the

`x`

and

`y`

tile coordinates were referenced directly.



Now here's the magic, the code that actually animates these offsets:

**monster.js**

```js
    draw(){
        if(this.teleportCounter > 0){
```

```js
            drawSprite(10, this.getDisplayX(),  this.getDisplayY());
```

```js
        }else{
```

```js
            drawSprite(this.sprite, this.getDisplayX(),  this.getDisplayY());
```

```js
            this.drawHp();                                 
        }
```

```js
        this.offsetX -= Math.sign(this.offsetX)*(1/8);     
        this.offsetY -= Math.sign(this.offsetY)*(1/8);
```

```js
    }
```



During each draw call, we're reducing the value of the offsets by one eigth of a tile, which is what produces the sliding animation.



`Math.sign()`

produces either -1, 0, or +1 if passed a negative, zero, or positive number respectively. This lets us move in the correct direction if the offset is still non-zero and completely stop altering the offset if it is zero.



Something interesting to note here is this actually won't work with a value like 0.1 instead of 0.125 (i.e. 1/8) because of floating point math. If you type

`0.1 + 0.1 + 0.1`

into the console instead of

`0.3`

you'll get

`0.30000000000000004`

. Floating point numbers can only be represented precisely if they are powers of two (e.g. 1/2, 1/4, 1/8). Obviously we could have written this to accomodate any kind of number with more code, but it's nice to do it all in only two lines.



## Bump attack

With that, basic animation between tiles is in place. And just two extra lines can add a bump attack animation.

**monster.js**

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
            }else{
                if(this.isPlayer != newTile.monster.isPlayer){
                    this.attackedThisTurn = true;
                    newTile.monster.stunned = true;
                    newTile.monster.hit(1);
```

```js
                    this.offsetX = (newTile.x - this.tile.x)/2;         
                    this.offsetY = (newTile.y - this.tile.y)/2;
```

```js
                }
            }
            return true;
        }
    }
```

When bump attacking, monsters are not really moving between tiles. Instead we're setting their offset so that the moment after attack they'll appear partially

in

the tile they're attacking. That's why we divided by 2. We want the monster to look like they've jumped halfway into their opponent's tile to give them a wallop.

## Screenshake

Screenshake uses a similar concept to offset animation: draw things in their proper place and then just tack on some additional offsets. And also reduce the value of those offsets every frame until they're 0 (the screenshake should quickly fade out).



In the case of screenshake, the offsets are random and they will apply to

everything

on screen. This lets us do our screenshake offset in a single place:

`drawSprite`

.



We'll start out declaring a

`shakeAmount`

variable for the magnitude of the shake and two component variables

`shakeX`

and

`shakeY`

. All are assigned 0 because of course we don't want to start out shaking.

**index.html**

```js
    gameState = "loading";  

    startingHp = 3; 
    numLevels = 6;
```

```js
    shakeAmount = 0;       
    shakeX = 0;                 
    shakeY = 0;
```

After each monster hit, we add some shake.

**monster.js**

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
            }else{
                if(this.isPlayer != newTile.monster.isPlayer){
                    this.attackedThisTurn = true;
                    newTile.monster.stunned = true;
                    newTile.monster.hit(1);
```

```js
                    shakeAmount = 5;
```

```js
                    this.offsetX = (newTile.x - this.tile.x)/2;         
                   this.offsetY = (newTile.y - this.tile.y)/2;         
                }
            }
            return true;
        }
    }
```



The part that actually affects what you see is here in

`drawSprite`

. Remember that the 6th and 7th arguments to

`drawImage`

are for the destination X & Y coordinates (where on screen to draw). Adding

`shakeX`

and

`shakeY`

does the job.



Then we'll add a new

`screenshake`

method that will handle splitting out

`shakeAmount`

into X & Y components and damping the amount.

**game.js**

```js
function drawSprite(sprite, x, y){
    ctx.drawImage(
        spritesheet,
        sprite*16,
        0,
        16,
        16,
```

```js
        x*tileSize,
        y*tileSize,
```

```js
        x*tileSize + shakeX,
        y*tileSize + shakeY,
```

```js
        tileSize,
        tileSize
    );
}

function draw(){
    if(gameState == "running" || gameState == "dead"){
        ctx.clearRect(0,0,canvas.width,canvas.height);
```

```js
        screenshake();
```

```js
...
```

```js
function screenshake(){
    if(shakeAmount){
        shakeAmount--;
    }
    let shakeAngle = Math.random()*Math.PI*2;
    shakeX = Math.round(Math.cos(shakeAngle)*shakeAmount);
    shakeY = Math.round(Math.sin(shakeAngle)*shakeAmount);
}
```

The

`shakeAmount`

is reduced by 1 every frame unless it's already 0. Notice this is yet another example here of type coercion. 0 gets coerced into

`false`

and any other number gets coerced into

`true`

.



The next part uses some basic

trigonometry

. A quick refresher in case you forgot your trig classes: if you provide an angle,

cosine

will tell you big the X component is and

sine

will tell you how big the Y component is. Our JavaScript trig functions deal in

radians

instead of degrees, which is why we'll be referencing

`Math.PI*2`

radians (the number of radians in a circle) instead of say 360 degrees.



We select a random

`shakeAngle`

and then use

`Math.cos`

and

`Math.sin`

to find the X & Y components. We multiply by the

`shakeAmount`

and finally

`round`

off to ensure there is no sub-pixel nonsense going on (that can look really bad).



Our screenshake is pretty subtle, but for fun try setting the

`shakeAmount`

on hit to a ridiculously high amount like 50 or higher and see what happens.

## Sounds

To really polish things up, we need sound!



We're going to make 5 sounds, from scratch:

- **hit1.wav**: when the player hits a monster

- **hit2:wav**: when a monster hits the player

- **treasure.wav**: when the player picks up a treasure

- **newLevel.wav**: when the player exits a level

- **spell.wav**: when a player casts a spell

To make our sounds, we're going to use a tool called Bfxr.Image: https://nluqo.github.io/broughlike-tutorial/screens/bfxr.PNGNow this might look an overwhelming monstrousity (audio software always is). But I promise it's the easiest game development tool I've ever used and creating very nice sounds can be done in a couple button clicks.First navigate to[bfxr.net](http://bfxr.net). You might have to fiddle around with your Flash settings to get it to launch.Really all you need to do is click one of the buttons in the upper left. The rest of the UI is for tweaking those sounds further, but that's optional. Our first sound "hit1.wav" can be generated with the*Hit/Hurt*button. We can continue to click that button to generate new sounds until we get one we like. You'll also find the buttons*Powerup*and*Jump*useful for the other types of sounds we want to generate.If you want to try out some customization options, the easiest one to start would be*Mutation*which mutates your current sound randomly. Selecting different synths is also pretty straightforward. I find*Sin*to be the softest option and it's great for soothing electronic noises like boops and beeps.*White*is great for static and rough sounding noises like explosions.After you're done, simply select*Export Wav*, name the file according to our naming scheme shown earlier, and save it into our project under a "sounds" folder.Generate the 5 sounds and then we'll get back into coding.

To play audio in the browser we use the

`Audio`

API. We'll create a new

`Audio`

object passing the audio URL and then call

`play`

on it.

**index.html**

```js
    setInterval(draw, 15);

    setupCanvas();
```

```js
    initSounds();
```

```js
<\/script>
```

**game.js**

```js
function initSounds(){          
    sounds = {
        hit1: new Audio('sounds/hit1.wav'),
        hit2: new Audio('sounds/hit2.wav'),
        treasure: new Audio('sounds/treasure.wav'),
        newLevel: new Audio('sounds/newLevel.wav'),
        spell: new Audio('sounds/spell.wav'),
    };
}

function playSound(soundName){                       
    sounds[soundName].currentTime = 0;  
    sounds[soundName].play();
}
```

We immediately call

`initSounds`

to load our 5 sounds and store them in a global object called

`sounds`

. To play a sound, we call

`playSound`

with the desired sound name. The only oddity is

`currentTime`

. Without resetting this to 0, trying to play a sound that's already in the progress of playing (like hitting a bunch of enemies quickly) sounds terrible.



Now let's call

`playSound`

in 4 places (the "spell" sound will be covered in the next section):

**monster.js**

```js
    hit(damage){
        this.hp -= damage;
        if(this.hp <= 0){
            this.die();
        }
```

```js
        if(this.isPlayer){                                                     
            playSound("hit1");                                              
        }else{                                                       
            playSound("hit2");                                              
        }
```

```js
    }
```

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
            playSound("treasure");
```

```js
            this.treasure = false;
            spawnMonster();
        }
    }
}
```

```js
...
```

```js
class Exit extends Tile{
    constructor(x, y){
        super(x, y, 11, true);
    }

    stepOn(monster){
        if(monster.isPlayer){
```

```js
            playSound("newLevel");
```

```js
            if(level == numLevels){
                addScore(score, true);
                showTitle();
            }else{
                level++;
                startLevel(Math.min(maxHp, player.hp+1));
            }
        }
    }
}
```

Now the game is really starting to look and sound like something nice!

![animation23](https://nluqo.github.io/broughlike-tutorial/screens/animation23.gif)

For our last addition, we'll add spells in the

next section

.
