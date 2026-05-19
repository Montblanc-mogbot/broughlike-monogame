# Broughlike tutorial - Stage 2

Source: https://nluqo.github.io/broughlike-tutorial/stage2.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Drawing to the screen](stage1.html)

# Stage 2 - Map Generation

In this step, we want to generate a map to play on.



A map is often stored as a 2D array, which is nothing more than an array where every element is another array. Specifically, our map will be an array of columns and each column will be an array of tiles. You could do it with rows, whatever, same thing. Make a new class called

`Tile`

in tile.js.

**tile.js**

```js
class Tile{
	constructor(x, y, sprite, passable){
        this.x = x;
        this.y = y;
        this.sprite = sprite;
        this.passable = passable;
	}

	draw(){
        drawSprite(this.sprite, this.x, this.y);
	}
}
```

Each tile will have

`x`

and

`y`

coordinates, a

`sprite`

(simply the numeric position in the spritesheet), and a

`passable`

flag. For the most basic maps, all you need to know is how to distinguish between walls and floors and

`passable`

does that for us. You can also see that we're using

`drawSprite`

like we did for the player.



Let's extend

`Tile`

to make floors and walls.

**tile.js**

```js
class Tile{
    constructor(x, y, sprite, passable){
        this.x = x;
        this.y = y;
        this.sprite = sprite;
        this.passable = passable;
    }

    draw(){
        drawSprite(this.sprite, this.x, this.y);
    }
}
```

```js
class Floor extends Tile{
    constructor(x,y){
        super(x, y, 2, true);
    };
}

class Wall extends Tile{
    constructor(x, y){
        super(x, y, 3, false);
    }
}
```

We have the code for floor and wall tiles, but we need the sprites.

## Drawing Map Tiles

The floor uses dark colors and not too much contrast since we don't want it to compete with the foreground sprites. With tiles, keep in mind how they're going to repeat. Pixels on the left of our sprite will appear next to pixels on the right (likewise for bottom and top). An easy technique I'm using here is drawing a bunch of connecting lines and then breaking them up with a few pixels of the background color.Image: https://nluqo.github.io/broughlike-tutorial/art/floor.pngFor the wall tile, we'll start with the same background color from the floor tile. This allows us to have rounded walls that blend in seamlessly.Image: https://nluqo.github.io/broughlike-tutorial/art/wall.pngDon't forget to export your file after updating it!



In map.js, we're going to start making the map.

**index.html**

```js
    setInterval(draw, 15);

    setupCanvas();
```

```js
    generateLevel();
```

**map.js**

```js
function generateLevel(){
    generateTiles();
}

function generateTiles(){
    tiles = [];
    for(let i=0;i<numTiles;i++){
        tiles[i] = [];
        for(let j=0;j<numTiles;j++){
            if(Math.random() < 0.3){
                tiles[i][j] = new Wall(i,j);
            }else{
                tiles[i][j] = new Floor(i,j);
            }
        }
    }
}
```

This makes a 2D array called

`tiles`

and populates it with about 30% walls and 70% floors.



It does so by looping over the columns and then, for each column, looping over the rows and creating new wall/floor tiles in each spot as appropriate.

![2darray](https://nluqo.github.io/broughlike-tutorial/screens/2darray.png)

So our map is actually there and the way you can confirm this is by going into the dev tools and simply typing "tiles".

![tiles-console](https://nluqo.github.io/broughlike-tutorial/screens/tiles-console.png)



But let's draw it. All we need to do is iterate over every tile and call

`draw`

on it.



But first, I want to wrap access to the tile array in a function called

`getTile`

. There are a lot of reasons for doing this. For one it reads a little cleaner, but it also lets us pull off shenanigans like returning tiles

outside

the bounds of the array and which don't actually exist (which might happen if, for instance, the player casts a giant area of effect spell that extends past the outer dungeon walls).

**map.js**

```js
function generateLevel(){
    generateTiles();
}

function generateTiles(){
    tiles = [];
    for(let i=0;i<numTiles;i++){
        tiles[i] = [];
        for(let j=0;j<numTiles;j++){
            if(Math.random() < 0.3){
                tiles[i][j] = new Wall(i,j);
            }else{
                tiles[i][j] = new Floor(i,j);
            }
        }
    }
}
```

```js
function inBounds(x,y){
    return x>0 && y>0 && x<numTiles-1 && y<numTiles-1;
}
```

```js
function getTile(x, y){
    if(inBounds(x,y)){
        return tiles[x][y];
    }else{
        return new Wall(x,y);
    }
}
```



Now with that out of the way, we can get to drawing simply by looping over each tile and telling it to draw itself.

**game.js**

```js
function draw(){
    ctx.clearRect(0,0,canvas.width,canvas.height);
```

```js
    for(let i=0;i<numTiles;i++){
        for(let j=0;j<numTiles;j++){
            getTile(i,j).draw();
        }
    }
```

```js
    drawSprite(0, x, y);
}
```

Cool. We haven't added an outer wall though. Let's reuse the

`inBounds`

function to make one.

**map.js**

```js
function generateTiles(){
    tiles = [];
    for(let i=0;i<numTiles;i++){
        tiles[i] = [];
        for(let j=0;j<numTiles;j++){
```

```js
            if(Math.random() < 0.3){
```

```js
            if(Math.random() < 0.3 || !inBounds(i,j)){
```

```js
                tiles[i][j] = new Wall(i,j);
            }else{
                tiles[i][j] = new Floor(i,j);
            }
        }
    }
}
```

![walls-drawn](https://nluqo.github.io/broughlike-tutorial/screens/walls-drawn.png)



Only thing is the walls are not doing anything. We start on a wall and we're walking right through them!



To fix that first problem, we're simply going to teleport the player to a random floor tile when the game starts.



How do we find a random floor tile though? One really dumb, easy way to do so is repeatedly grab random tiles until we find a floor tile. This pattern works really well for a variety of map generation techniques: try to do something at random and keep doing it until we get it right. Add a timeout to make sure we don't get stuck in an infinite loop on accident. You don't want to lock up your browser after all.



We'll also need a simple utility function to generate random integers within a given range.

**util.js**

```js
function tryTo(description, callback){
    for(let timeout=1000;timeout>0;timeout--){
        if(callback()){
            return;
        }
    }
    throw 'Timeout while trying to '+description;
}
```

```js
function randomRange(min, max){
    return Math.floor(Math.random()*(max-min+1))+min;
}
```

**map.js**

```js
function getTile(x, y){
    if(inBounds(x,y)){
        return tiles[x][y];
    }else{
        return new Wall(x,y);
    }
}
```

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

Our

`tryTo`

function needs to be passed a callback that attempts something and returns

`true`

only if it worked. That's why we're passing an anonymous function that rolls some random coordinates, gets the corresponding tile, and returns true only if the following expression is true:

`tile.passable && !tile.monster`

(the tile is passable and has no monster).



Notice that nowhere else in our program have we defined

`tile.monster`

. Thanks to JavaScript's lenient property and type handling, this will work fine. A missing property will have a value of

`undefined`

and that will get coerced to

`false`

and we're good. Let's take

`randomPassableTile`

out for a spin by teleporting the player to random tile as mentioned earlier.

**index.html**

```js
    generateLevel();
```

```js
    startingTile = randomPassableTile();
    x = startingTile.x;
    y = startingTile.y;
```

Refresh several times and you'll see that the player spawns in random locations, none of which are walls.



Our map is generated and our player is spawned in the correct place. We're almost done.



## Banishing disconnected islands (a roguelike developer's greatest enemy)

One more big problem first and this one is a super common headache when procedurally generating the maps. With our extremely dumb way of making the levels, there are often areas of the map not connected to other areas. Disconnected islands. We need a way a check that

everything is connected

.



This part is a little tricky, so first let's some write some pseudocode:

```js
/*
    generate a level and make note of how many passable tiles we generated

    starting from a random passable tile, get all tiles connected to it

    if the total number of passsable tiles matches the number of connected tiles
        everything is connected!
    else 
        start over

*/
```

That's the basic idea. Now let's convert that into real code:

**map.js**

```js
function generateLevel(){
```

```js
    generateTiles();
```

```js
    tryTo('generate map', function(){
        return generateTiles() == randomPassableTile().getConnectedTiles().length;
    });
```

```js
}

function generateTiles(){
```

```js
    let passableTiles=0;
```

```js
    tiles = [];
    for(let i=0;i<numTiles;i++){
        tiles[i] = [];
        for(let j=0;j<numTiles;j++){
            if(Math.random() < 0.3 || !inBounds(i,j)){
                tiles[i][j] = new Wall(i,j);
            }else{
                tiles[i][j] = new Floor(i,j);
```

```js
                passableTiles++;
```

```js
            }
        }
    }
```

```js
    return passableTiles;
```

```js
}
```

They key here is

`getConnectedTiles`

. Imagine you are drawing a dungeon map on grid paper and you want to color in all the connected floor spaces. How would you do it?

![floodfill](https://nluqo.github.io/broughlike-tutorial/screens/floodfill.gif)

[diagram by André Karwath](https://en.wikipedia.org/wiki/Flood_fill#/media/File:Recursive_Flood_Fill_4_(aka).gif)



One way is pick a random floor tile on the grid, expand outwards repeatedly, and color connected floor tiles until you're done. You obviously would ignore wall tiles and you wouldn't recolor floor tiles once you drew them the first time. That's exactly the idea behind the

flood fill

algorithm and here's more pseudocode for a function that does it:

```js
/*
    create a list of tiles to check for connectedness
    create a list of connected tiles

    add a single random passable tile to both lists

    while there are more tiles to check...
        pick one
        get its neighbors
        filter out the walls
        filter out the tiles we've already found were connected
        add the filtered neighbors to a list of connected tiles and to the tiles that need to be checked

    return the list of connected tiles
*/
```

We'll convert that to JavaScript as well by breaking it down into several smaller functions:

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
    getNeighbor(dx, dy){
        return getTile(this.x + dx, this.y + dy)
    }

    getAdjacentNeighbors(){
        return shuffle([
            this.getNeighbor(0, -1),
            this.getNeighbor(0, 1),
            this.getNeighbor(-1, 0),
            this.getNeighbor(1, 0)
        ]);
    }

    getAdjacentPassableNeighbors(){
        return this.getAdjacentNeighbors().filter(t => t.passable);
    }

    getConnectedTiles(){
        let connectedTiles = [this];
        let frontier = [this];
        while(frontier.length){
            let neighbors = frontier.pop()
                                .getAdjacentPassableNeighbors()
                                .filter(t => !connectedTiles.includes(t));
            connectedTiles = connectedTiles.concat(neighbors);
            frontier = frontier.concat(neighbors);
        }
        return connectedTiles;
    }
```

```js
    draw(){
        drawSprite(this.sprite, this.x, this.y);
    }
}
```

OK, that seems like a lot so let's break it down.



`getNeighbor`

is pretty straightforward. It's a wrapper around

`getTile`

. Even though it's a simple wrapper, this method is useful when implementing all sorts of mechanics. Also note that it works for

more

than just adjacent neighbors.



`getAdjacentNeighbors`

returns the adjacent neighbors of a tile and shuffles them before returning.



`getAdjacentPassableNeighbors`

additionally filters out non-passable tiles (e.g. walls).

`filter`

is a nifty JavaScript array function where we pass in a callback and we return true for only the elements we want to keep.



`getConnectedTiles`

does exactly what the pseudocode said. It uses

`filter`

again and also

`concat`

, which adds one array to another.

**util.js**

```js
function randomRange(min, max){
    return Math.floor(Math.random()*(max-min+1))+min;
}
```

```js
function shuffle(arr){
    let temp, r;
    for (let i = 1; i < arr.length; i++) {
        r = randomRange(0,i);
        temp = arr[i];
        arr[i] = arr[r];
        arr[r] = temp;
    }
    return arr;
}
```

And finally

`shuffle`

is an implementation of

Fisher-Yates shuffle

. All it's doing is a random swap for each element in an array, leading to a perfectly shuffled array.



To reiterate: we keep trying to make levels until we find one that has no disconnected islands. The way we know there are no disconencted islands is when we've made only a single connected "island". If we have one big island of floor tiles, the number of tiles on that island should match the total number of floor tiles on the map. When we're able to satisfy that condition, we're happy with our level and we can stop.



So now let's check our work. Refresh a bunch of times and you'll never see a disconnected island again.

![stage2-complete](https://nluqo.github.io/broughlike-tutorial/screens/stage2-complete.png)

Just for fun, you might try out different values of

`numTiles`

. Here's what I got with a value of 24 (much higher and the timeout was usually reached):

![kicks](https://nluqo.github.io/broughlike-tutorial/screens/kicks.png)

In the

next section

, we'll create our monsters and make sure the player can't walk through walls.
