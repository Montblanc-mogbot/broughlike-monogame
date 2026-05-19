# Broughlike tutorial - Stage 0

Source: https://nluqo.github.io/broughlike-tutorial/stage1.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Introduction](stage0.html)

# Stage 1 - Drawing to the screen

You should already have everything you need installed: a browser, a text editor, and an image editor. The apps that I'm going to use and that I recommend are Google Chrome, Sublime Text, and

GIMP

.

## Getting started

First open your text editor and create a file called

index.html

, type in

`Hello World!`

, and save the file.



Open the file in your chosen browser. Be aware that this isn't valid HTML but the browser will render it just fine anyway.

![helloworld](https://nluqo.github.io/broughlike-tutorial/screens/helloworld.png)



Let's replace our "Hello World!" text with the next 6 lines, which are all you need to draw to the screen:

**index.html**

```js
<canvas></canvas>
<script>
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");
    ctx.fillRect(0,0,20,20,20);
<\/script>
```

The

`canvas`

is an HTML tag on which we can draw shapes, images, and text. Our

`script`

tag is where our JavaScript code goes. Here, we're grabbing a reference to the

`canvas`

tag, getting the 2D context which is what we'll use to draw, and then drawing a rectangle.



Three lines might seem excessive for a single draw operation, but the first two lines are boilerplate. We'll only need to write them once for the whole project.



Our little black rectangle might not look like much yet, but that's our player character! Sadly the player doesn't move. We just need a little bit more code to handle the player's current position (I'll be highlighting removed lines with red and added lines with green).

**index.html**

```js
<script>
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");
```

```js
    ctx.fillRect(0,0,20,20,20);
```

```js
    x = y = 0;

    document.querySelector("html").onkeypress = function(e){
        if(e.key=="w") y--;
        if(e.key=="s") y++;
        if(e.key=="a") x--;
        if(e.key=="d") x++;
    };

    function draw(){
        ctx.fillRect (x*20,y*20,20,20);
    }

    setInterval(draw, 15);
```

```js
<\/script>
```

We're managing our player's position with

`x`

and

`y`

coordinates (counted in grid tiles since our game will be played on a grid).



Then the

`onkeypress`

event will fire whenever the user presses a key (specifically lowercase "wasd" keys). We'll move the player horizontally or vertically by changing

`x`

or

`y`

as appropriate.



Finally, the

`fillRect`

call is updated to use our new coordinate variables, moved into a function, and triggered with

`setInterval`

to make sure the screen is drawn every 15 milliseconds (more than 60 frames per second).

![snake](https://nluqo.github.io/broughlike-tutorial/screens/snake.png)

It works but... there's no code to clear the character aftering rendering though. Let's do that at the beginning of every draw call.

**index.html**

```js
function draw(){
```

```js
    ctx.clearRect(0,0,1000,1000);
```

```js
    ctx.fillRect (x*20,y*20,20,20);
}
```

In 20 lines of code, we have a moving player. We're 90% of the way to having a functioning game. 😉



One problem is the canvas is not aligned properly, it's very small, and we don't know where it ends. Let's add an outline and center the canvas. To do this, we're going to add a

`style`

tag to the top of our file and put CSS in it. If we had a lot of CSS, it would be better to put it in a separate file but the small amount we need fits well in the document.

**index.html**

```js
<style>
    canvas{
        outline: 1px solid white;
    }

    body{
        background-color: indigo;
        text-align: center;
        margin-top: 50px;
    }
<\/style>
```

```js
<canvas></canvas>
<script>
    canvas = document.querySelector("canvas");
```



A couple more lines at the very top to make this truly valid HTML. The

`doctype`

tells the browser we're an HTML page, duh. And a

`title`

is required for... reasons.

**index.html**

```js
<!DOCTYPE html>
<title>AWESOME BROUGHLIKE</title>
```

By the way, you might be used to

`html`

/

`head`

/

`body`

tags but they're all optional! Seriously. Include them if you'd like.

## Getting organized

Let's lay the groundwork for the rest of the project by creating a couple folders and empty files. Keep in mind, this is only to help us stay organized. There's no requirement to have a specific directory layout. If we keep all of our code in one file, it'll get really hard to dig through. But it's certainly not impossible. I know a major roguelike project that consists of a single 80,000 line python file.



Create a folder called

js

where all our script files will go and one called

sounds

where our sounds will go.



Go ahead and create six empty JavaScript files in the new

js

folder:

- game.js

- map.js

- tile.js

- monster.js

- util.js

- spell.js

Now we'll include those JavaScript files using

`script`

tags.

**index.html**

```js
<canvas></canvas>
```

```js
<script src="js/game.js"><\/script>
<script src="js/map.js"><\/script>
<script src="js/tile.js"><\/script>
<script src="js/monster.js"><\/script>
<script src="js/util.js"><\/script>
<script src="js/spell.js"><\/script>
```

```js
<script>
    canvas = document.querySelector("canvas");
```



I'll be defining everything in these files globally and without using any sort of namespacing or modules. This makes the code shorter, but potentially harder to track down. One easy alternative is defining the entire file as an object like this:

**game.js**

```js
game = {
    someFunction: function(){
        ...
    },
    anotherFunction: function(){
        ...
    },
    andAnother: function(){
        ...
    }
}
```

Then when you read

`game.someFunction()`

you'll know exactly where to find it (in the file game.js). For now, I'm going to stick to global functions.



Let's

move

our

`canvas`

and

`ctx`

definitions to a new function in game.js and call that new function from index.html.

**index.html**

```js
<script>
```

```js
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");
```

```js
    x = y = 0;

    document.querySelector("html").onkeypress = function(e){
        if(e.key=="w") y--;
        if(e.key=="s") y++;
        if(e.key=="a") x--;
        if(e.key=="d") x++;
    };

    function draw(){
        ctx.fillRect (x*20,y*20,20,20);
    }

    setInterval(draw, 15);
```

```js
    setupCanvas();
```

```js
<\/script>
```

**game.js**

```js
function setupCanvas(){
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");
}
```

If you test the game, nothing should have changed. But we have taken the first step towards organizing our code.



We want to dynamically size the canvas based on some constants. This will make it really easy to tweak the design of the game later, making the grid size tiny or huge instantly.

**game.js**

```js
function setupCanvas(){
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");
```

```js
    canvas.width = tileSize*(numTiles+uiWidth);
    canvas.height = tileSize*numTiles;
    canvas.style.width = canvas.width + 'px';
    canvas.style.height = canvas.height + 'px';
```

```js
}
```

Run the code.



![tileSizeUndefined](https://nluqo.github.io/broughlike-tutorial/screens/tileSizeUndefined.png)

You should get an error. Uh oh. We forgot to define a few variables. Let's do that now.

**index.html**

```js
<script>
```

```js
    tileSize = 64;
    numTiles = 9;
    uiWidth = 4;
```

```js
    x = y = 0;
```

These 3 variables tell us respectively: how big a tile should be in on screen pixels, how wide/tall our map will be measured in tiles, and how much space we should reserve for the game UI.



That should fix the error we saw. Now we can also utilize some of these variables in the draw call.



**index.html**

```js
    function draw(){
```

```js
        ctx.clearRect(0,0,1000,1000);
        ctx.fillRect (x*20,y*20,20,20);
```

```js
        ctx.clearRect(0,0,canvas.width,canvas.height);
        ctx.fillRect (x*tileSize,y*tileSize,tileSize,tileSize);
```

```js
    }
```

You'll notice that the "player" rectangle gets bigger and when you move to the bottom/edge of the screen, it lines up nicely.

## Drawing sprites

Finally, let's replace our little rectangle with a real sprite. You could make the whole game in rectangles if you wanted (it worked for Mike Bithell), but I'm not talented enough for that.Open up your favorite image editor. I'm going to use[GIMP](https://www.gimp.org/).Here are some setup tips specific to GIMP:Select**File > New**and create a new file that's**512 by 16 pixels**. Make sure your starting background is transparent (under**Advanced Options**choose**Fill with: Transparency**). Normally, a spritesheet is arranged in big square but it's a little simpler to make it a long line of sprites.A visible grid will help us distinguish our sprites. Select**Image > Configure Grid**, and configure the spacing to be**16 pixels**in both directions. Then select**View > Show Grid**.Configure your**Eraser Tool**to use**Hard edge**. Otherwise, you won't fully erase things.Select the**Pencil Tool**and make sure your**Brush**setting is configured to be**Size: 1**.One extra, optional tip: sprites can be hard to see on the checkerboard pattern of transparent layers, so try adding an extra background layer filled with a darkish grey color.With the**Pencil Tool**in hand, you're ready. We're going to use a simple 3 step process to draw our sprites:Fill in a basic shape with 1 or 2 colorsAdd some shadingFinish details and highlightsI'm going to show you each of my 3 steps, but only as a hint. Feel free to really make this art your own.Draw your character sprite in the first 16 by 16 pixel tile.Image: https://nluqo.github.io/broughlike-tutorial/art/player.pngDraw your a corpse sprite in the next spot over. I cheated a bit by rotating the a copy of the first sprite 90 degrees.Image: https://nluqo.github.io/broughlike-tutorial/art/player-corpse.pngThe last step after drawing each sprite is exporting your spritesheet by selecting**File > Export**and exporting to a file name**spritesheet.png**in your game's directory.As a sanity check, here's what your spritesheet should look like at this point.Image: https://nluqo.github.io/broughlike-tutorial/screens/first-draw.png



Let's use the sprite we just drew. This code will load the PNG file.

**index.html**

```js
<script>
    tileSize = 64;
    numTiles = 9;
    uiWidth = 4;

    x = y = 0;
```

```js
    spritesheet = new Image();
    spritesheet.src = 'spritesheet.png';
```



We're going to draw a sprite from that spritesheet now instead of drawing a rectangle. Let's try the simplest way to call

`drawImage`

with 3 arguments.

**index.html**

```js
    function draw(){
        ctx.clearRect(0,0,canvas.width,canvas.height);
```

```js
        ctx.fillRect (x*tileSize,y*tileSize,tileSize,tileSize);
```

```js
        ctx.drawImage(spritesheet, x*tileSize, y*tileSize);
```

```js
    }
```

![tinysprite](https://nluqo.github.io/broughlike-tutorial/screens/tinysprite.png)

It works, but there's a few problem. The sprites are too small for one. The bigger problem though is that this displays the

whole

spritesheet. We need to call

`drawImage`

with

9 arguments

.



It sounds bad, but it's pretty simple once you see the purpose. The first argument is the name of the image. The last four arguments are totally identical to the ones we used in

`drawRect`

. Those specify how to draw to the

destination

on screen. But we're using a spritesheet which consists of many sprites. We can't use the whole image, so we need 4 more arguments to specify how to pull from the

source

.



One image, 4 source variables, and 4 destination variables.



`ctx.drawImage(image, sx, sy, sWidth, sHeight, dx, dy, dWidth, dHeight);`



Since we're going to be making this same call repeatedly, let's move this code out to a new function called

`drawSprite`

and move/update our

`draw`

function too. And let's put each argument on a different line to make it clearer.

**index.html**

```js
    function draw(){
        ctx.clearRect(0,0,canvas.width,canvas.height);
        ctx.drawImage(spritesheet, x*tileSize, y*tileSize);
    }
```

**game.js**

```js
function setupCanvas(){
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");

    canvas.width = tileSize*(numTiles+uiWidth);
    canvas.height = tileSize*numTiles;
    canvas.style.width = canvas.width + 'px';
    canvas.style.height = canvas.height + 'px';
}
```

```js
function drawSprite(sprite, x, y){
    ctx.drawImage(
        spritesheet,
        sprite*16,
        0,
        16,
        16,
        x*tileSize,
        y*tileSize,
        tileSize,
        tileSize
    );
}

function draw(){
    ctx.clearRect(0,0,canvas.width,canvas.height);
    drawSprite(0, x, y);
}
```

![blurry](https://nluqo.github.io/broughlike-tutorial/screens/blurry.png)

Now one more hiccup. Our sprite looks like poop. The reason is the way browser scales images by default. It makes sene for photos but not for pixel art. For years, displaying scaled pixel art in browsers was basically impossible. Luckily we only need one line to correct it.

**game.js**

```js
function setupCanvas(){
    canvas = document.querySelector("canvas");
    ctx = canvas.getContext("2d");

    canvas.width = tileSize*(numTiles+uiWidth);
    canvas.height = tileSize*numTiles;
    canvas.style.width = canvas.width + 'px';
    canvas.style.height = canvas.height + 'px';
```

```js
    ctx.imageSmoothingEnabled = false;
```

```js
}
```

And we're done! If you're feeling a little overwhelmed by all these arcane references to interacting with the DOM (the way the browser connect HTML and JS together), I understand.



Take heart that we're pretty much past all of that. Our entry point into drawing stuff on the screen is to load images, grab a canvas element, get its context, and start calling draw operations on it. A lot of boilerplate right? But the good news is that we don't need any more hooks into the DOM and the rest of the code we write will be fairly self contained.

![stage1-complete](https://nluqo.github.io/broughlike-tutorial/screens/stage1-complete.png)



In the

next section

, we'll generate a map.
