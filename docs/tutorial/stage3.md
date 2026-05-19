# Broughlike tutorial - Stage 3

Source: https://nluqo.github.io/broughlike-tutorial/stage3.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Map Generation](stage2.html)

# Stage 3 - Monsters

In this section, we're going to make a

`Monster`

class just like we did with the

`Tile`

class.



Earlier, we used just two variables (

`x`

&

`y`

) to represent position, but

`Tile`

already has that. So when we want to move a monster around, we'll simply pass it a tile.

**monster.js**

```js
class Monster{
	constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
	}

	draw(){
        drawSprite(this.sprite, this.tile.x, this.tile.y);
	}
```

```js
}
```

Each monster has its own

`sprite`

and starting

`hp`

. In addition, the monster will immediately move to its starting

`tile`

.



OK, let's start working on that movement code. Before moving we first check a

`tryMove`

function. Why is this needed?



Because in a broughlike the player and other monsters may often "try" to move into tiles where they can't fit! This might mean bouncing off a wall or it might turn into bump combat, a staple of classic roguelikes.

**monster.js**

```js
class Monster{
	constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
	}

	draw(){
        drawSprite(this.sprite, this.tile.x, this.tile.y);
	}
```

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
            }
            return true;
        }
    }

    move(tile){
        if(this.tile){
            this.tile.monster = null;
        }
        this.tile = tile;
        tile.monster = this;
    }
```

```js
}
```

For now, we'll check the neighboring tile (the one we're trying to move into) and only allow a

`move`

if the tile is passable and has no monster in it. We'll

`return true;`

to indicate the move was successful - either we could move or attack (we'll do that part later).



All the

`move`

method has to do is update a bunch of references: which

`monster`

is on which

`tile`

and which

`tile`

is holding which

`monster`

.



OK then. Which monster are we going to make first? The player!



"Beware that, when fighting monsters, you yourself do not become a monster... "

-Nietzsche



When I first started writing roguelikes I naturally coded the player as a separate thing from the monsters. It seemed counterintuitive to make them the same kind of thing, but actually they share so much behavior. And also it's a common roguelike mechanic that monsters and players behave in

similar ways

.

**monster.js**

```js
class Monster{
	constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
	}

	draw(){
        drawSprite(this.sprite, this.tile.x, this.tile.y);
	}
```

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
            }
            return true;
        }
    }

    move(tile){
        if(this.tile){
            this.tile.monster = null;
        }
        this.tile = tile;
        tile.monster = this;
    }
```

```js
}
```

```js
class Player extends Monster{
    constructor(tile){
        super(tile, 0, 3);
        this.isPlayer = true;
    }
}
```

This should look similar to how we implemented our

`Floor`

and

`Wall`

classes. We're passing a tile that we live on, a sprite index of

`0`

, and a maximum HP of

`3`

.



I'm setting an extra flag called

`isPlayer`

to dintinguish from other monsters. Some people might say this is a little kludgy, but I find this kind of flag very easy to use.



Ok, now let's put that player class to use and rip out all our

`x`

&

`y`

references.

**index.html**

```js
<script>
    tileSize = 64;
    numTiles = 9;
    uiWidth = 4;
```

```js
    x = y = 0;
```

```js
    spritesheet = new Image();
    spritesheet.src = 'spritesheet.png';

    document.querySelector("html").onkeypress = function(e){
```

```js
        if(e.key=="w") y--;
        if(e.key=="s") y++;
        if(e.key=="a") x--;
        if(e.key=="d") x++;
```

```js
        if(e.key=="w") player.tryMove(0, -1);
        if(e.key=="s") player.tryMove(0, 1);
        if(e.key=="a") player.tryMove(-1, 0);
        if(e.key=="d") player.tryMove(1, 0);
```

```js
    };

    setInterval(draw, 15);

    setupCanvas();

    generateLevel();
```

```js
    startingTile = randomPassableTile();
    x = startingTile.x;
    y = startingTile.y;
```

```js
    player = new Player(randomPassableTile());
```

**game.js**

```js
function draw(){
    ctx.clearRect(0,0,canvas.width,canvas.height);

    for(let i=0;i<numTiles;i++){
        for(let j=0;j<numTiles;j++){
            getTile(i,j).draw();
        }
    }
```

```js
    drawSprite(0, x, y);
```

```js
    player.draw();
```

```js
}
```

Test out your game. The player moves around but can't go through walls. Awesome. Now let's switch gears and do some art.

## Drawing Monsters

I'd like you to draw the 5 monsters used in the game. As a reminder, the strategy I used was to create a basic shape and then in the next two steps draw shading and highlights.The tiny resolution and small color palette of each sprite makes this process fairly easy. Importantly, I didn't worry too much about how great these looked or if they made sense (they definitely don't). Rather my goal was simple sprites that all felt distinct from one another.First, the lowly Bird.Image: https://nluqo.github.io/broughlike-tutorial/art/bird.pngHere, I made a lizardy dude that I'm calling Snake for some reason.Image: https://nluqo.github.io/broughlike-tutorial/art/snake.pngSome blobby thing that's going to have lot of health thus called Tank.Image: https://nluqo.github.io/broughlike-tutorial/art/tank.pngSort of a big dinosaur head called Eater.Image: https://nluqo.github.io/broughlike-tutorial/art/eater.pngThe last monster is the Jester.Image: https://nluqo.github.io/broughlike-tutorial/art/jester.pngWhile we're here, let's draw an HP pip sprite.Image: https://nluqo.github.io/broughlike-tutorial/art/hp.png

With the hard part out of the way, let's code up the monsters. For now, each one will only differ by sprite and starting HP. More detail to follow.

**monster.js**

```js
class Player extends Monster{
    constructor(tile){
        super(tile, 0, 3);
        this.isPlayer = true;
    }
}
```

```js
class Bird extends Monster{
    constructor(tile){
        super(tile, 4, 3);
    }
}

class Snake extends Monster{
    constructor(tile){
        super(tile, 5, 1);
    }
}

class Tank extends Monster{
    constructor(tile){
        super(tile, 6, 2);
    }
}

class Eater extends Monster{
    constructor(tile){
        super(tile, 7, 1);
    }
}

class Jester extends Monster{
    constructor(tile){
        super(tile, 8, 2);
    }
}
```

We need to think about how we're going to

spawn

the monsters into the game. Right off the bat, I know we're going to want to scale the number of monsters based on the current map level, so first let's add a variable to keep track of that.

**index.html**

```js
<script>
    tileSize = 64;
    numTiles = 9;
    uiWidth = 4;
```

```js
    level = 1;
```

```js
    spritesheet = new Image();
    spritesheet.src = 'spritesheet.png';
```

Then two new functions: one to spawn a single monster and another to create an bunch of monsters by repeatedly calling the first.

**map.js**

```js
function randomPassableTile(){
    let tile;
    tryTo('get random passable tile', function(){
        let x = randomRange(0,numTiles-1);
        let y = randomRange(0,numTiles-1);
        tile = getTile(x, y);
        return tile.passable && !tile.monster;
    });
    return tile;
}
```

```js
function generateMonsters(){
    monsters = [];
    let numMonsters = level+1;
    for(let i=0;i<numMonsters;i++){
        spawnMonster();
    }
}

function spawnMonster(){
    let monsterType = shuffle([Bird, Snake, Tank, Eater, Jester])[0];
    let monster = new monsterType(randomPassableTile());
    monsters.push(monster);
}
```

In this code, we're going to make an array of

`monsters`

, and spawn some monsters into it. How many do we want to spawn? One more than the current level (2 on the first floor, 3 on the second floor, etc.)



In

`spawnMonster`

, we start with an array of

`Monster`

classes that we just coded. To grab a random one, we'll

`shuffle`

the array and grab the first element. Here again you see the use of the

`new`

keyword but this time combined with a variable instead of the literal name of a class. We start them each on a

`randomPassableTile`

like we did with the player. Then we add them to our

`monsters`

array.



Only two more things to get monsters into the game: actually triggering

`generateMonsters`

and then drawing them.

**map.js**

```js
function generateLevel(){
    tryTo('generate map', function(){
        return generateTiles() == randomPassableTile().getConnectedTiles().length;
    });
```

```js
    generateMonsters();
```

```js
}
```

**game.js**

```js
function draw(){
    ctx.clearRect(0,0,canvas.width,canvas.height);

    for(let i=0;i<numTiles;i++){
        for(let j=0;j<numTiles;j++){
            getTile(i,j).draw();
        }
    }
```

```js
    for(let i=0;i<monsters.length;i++){
        monsters[i].draw();
    }
```

```js
    player.draw();
}
```

Check it out. We've got monsters on the map. They aren't doing much besides blocking our path. We'll need some monster AI to make this more interesting.

## Monster movement

I often use a pathfinding algorithm called

A*

to handle monster movement. But take a look at the

pseudocode

. It's hard to get right, even if you've written it before. If you

did

want to use

A*

, I would strongly recommend a library like rot.js to handle it.



Instead we're going to take a shortcut and use "greedy" movement, which simply means trying to get closer on

every

turn even if it's not the ideal path in the long term. Monsters will try to move closer even when that gets them trapped. Trust me, this will still lead to interesting (but unique) gameplay.

**monster.js**

```js
class Monster{
    constructor(tile, sprite, hp){
        this.move(tile);
        this.sprite = sprite;
        this.hp = hp;
    }
```

```js
    update(){
        this.doStuff();
    }

    doStuff(){
       let neighbors = this.tile.getAdjacentPassableNeighbors();
       
       neighbors = neighbors.filter(t => !t.monster || t.monster.isPlayer);

       if(neighbors.length){
           neighbors.sort((a,b) => a.dist(player.tile) - b.dist(player.tile));
           let newTile = neighbors[0];
           this.tryMove(newTile.x - this.tile.x, newTile.y - this.tile.y);
       }
    }
```

```js
    draw(){
        drawSprite(this.sprite, this.tile.x, this.tile.y);
    }
```

Our movement code will go in a method called

`doStuff`

. Why not just do it all in

`update`

? We very often want to separate updates (things like status effect counters or health regeneration) from monster actions.



We start by getting a monster's passable, adjacent neighbors that are either empty and can be moved to or contain the player and can be attacked. Now we need to pick the

closest

tile to the player. We do that with a

`sort`

that is going to sort our

`neighbors`

by their distance to the player, picking the first one (which will be closest), and trying to move to it.



Here's the method for calculating distance, specifically

Manhattan distance

.

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
    //manhattan distance
    dist(other){
        return Math.abs(this.x-other.x)+Math.abs(this.y-other.y);
    }
```

```js
    getNeighbor(dx, dy){
        return getTile(this.x + dx, this.y + dy)
    }
```

When should we call

`update`

? In game dev, the code to update the world and the monsters in it is commonly called a "tick". So we need one of those.

**game.js**

```js
function draw(){
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
}
```

We iterate over

`monsters`

(importantly in

reverse

so they can be safely deleted), call

`update`

if each monster is alive, and if not delete them with

`splice`

.



The last piece of the puzzle is when to call

`tick`

and I bet you can guess. In a turn based roguelike, monsters move immediately after the player takes an action.

**monster.js**

```js
class Player extends Monster{
    constructor(tile){
        super(tile, 0, 3);
        this.isPlayer = true;
    }
```

```js
    tryMove(dx, dy){
        if(super.tryMove(dx,dy)){
            tick();
        }
    }
```

```js
}
```

We override the

`tryMove`

method in the

`Player`

class. The overriden method that we wrote earlier in

`Monster`

is still available for us to use by calling

`super.tryMove(dx, dy)`

. If that method returns true (meaning the player action was a success instead of, say, bumping into a wall), we can trigger

`tick`

and all the monsters will then move.



Try out your game and that's exactly what you should see.

![chasing](https://nluqo.github.io/broughlike-tutorial/screens/chasing.gif)

In the

next section

, we're going to get the monsters attacking and fill out the details in our 5 monster implementations.
