# Broughlike tutorial - Stage 0

Source: https://nluqo.github.io/broughlike-tutorial/stage6.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Game Lifecycle](stage5.html)

# Stage 6 - Treasure & Score

To give our game some replayability, we'll add a high score mechanic. The player will pick up a treasure to gain a point, but doing so will spawn another monster. Each level will have 3 treasures. This system will be supported by a

`score`

variable that resets to

`0`

before every game.

## Drawing the treasure sprite

You have plenty of options for representing treasure. Gems, jewelry, piles of gold, treasure chests, whatever.For gold, you typically want to use a yellowish-orange base with very high contrast. For gems, pick bright colors and angular highlights/shadows.Image: https://nluqo.github.io/broughlike-tutorial/art/treasure.png

## Generating treasure

To achieve our treasure mechanic, we need very little: mainly a boolean flag to each tile denoting it has treasure on it. If a tile has treasure, the treasure sprite will be drawn on top.

**map.js**

```js
function generateLevel(){
    tryTo('generate map', function(){
        return generateTiles() == randomPassableTile().getConnectedTiles().length;
    });

    generateMonsters();
```

```js
    for(let i=0;i<3;i++){                                         
        randomPassableTile().treasure = true;                            
    }
```

```js
}
```

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
```

```js
        if(this.treasure){                      
            drawSprite(12, this.x, this.y);                                             
        }
```

```js
    }
```

```js
}
```

With that, our treasure is in the game and being drawn. You can test it out yourself to see.

## Keeping score

Let's make the treasure meaningful. We need to initialize our

`score`

variable every time we start a new game. Then when a treasure gets picked up (by the player stepping on a tile that has one), the

`score`

is increased, the

`treasure`

flag is reset which effectively deletes the treasure, and we spawn a monster.

**game.js**

```js
function startGame(){                                           
    level = 1;
```

```js
    score = 0;
```

```js
    startLevel(startingHp);

    gameState = "running";
}
```

**tile.js**

```js
class Floor extends Tile{
    constructor(x,y){
        super(x,y, 2, true);
    };

    stepOn(monster){
```

```js
        //TODO: complete
```

```js
        if(monster.isPlayer && this.treasure){   
            score++;                        
            this.treasure = false;
            spawnMonster();
        }
```

```js
    }
```

And now that we wrote our

`drawText`

function last time, showing our current score is very easy. The calls to draw the current level and current score only differ by text and Y position, which we're manually hardcoding.

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

        drawText("Level: "+level, 30, false, 40, "violet");
```

```js
        drawText("Score: "+score, 30, false, 70, "violet");
```

```js
    }
```

```js
}
```

## High scores

If we have a score, we certainly need a high score board. The following additions add a score array to a browser storage mechanism called

`localStorage`

, which we'll then retrieve and display on the title screen. The cool thing is, despite being super simple to use (i.e. a dumping ground where we can throw any string variables we want),

`localStorage`

will preserve our score data across page refreshes and browser launches.



Since everything you put in

`localStorage`

needs to be a string and we would prefer to put objects in there, we're converting back and forth from

JSON

. If you don't know JSON, not to worry! It's a data format that looks much like JavaScript and all you need to utilize it is two built in functions.



First let's try to grab the scores, whether there's some there or not.

**game.js**

```js
function getScores(){
    if(localStorage["scores"]){
        return JSON.parse(localStorage["scores"]);
    }else{
        return [];
    }
}
```

If we've not yet saved anything to localStorage, we simply return an empty array. But if we have, we take what's there,

`parse`

it as JSON, and return the result.



Now let's write the function to add a score.

**game.js**

```js
function addScore(score, won){
    let scores = getScores();
    let scoreObject = {score: score, run: 1, totalScore: score, active: won};
    let lastScore = scores.pop();

    if(lastScore){
        if(lastScore.active){
            scoreObject.run = lastScore.run+1;
            scoreObject.totalScore += lastScore.totalScore;
        }else{
            scores.push(lastScore);
        }
    }
    scores.push(scoreObject);

    localStorage["scores"] = JSON.stringify(scores);
}
```

In this game you'll be able to continue a high score if you won the last game. This lets you attempt win streaks, a common thing to find in broughlikes.



Our

`addScore`

function takes two variables: a numeric score and a flag telling us if we won the game or died.



Here's the breakdown of what we're doing:

- retrieving our scores

- creating a new`scoreObject`to be added onto the list later

- doing a`pop`to get the`lastScore`

- if that score is active, we'll add our current run score to it. otherwise just put it back with`push`

- put our new score back on the list with`push`

- `stringify`all our scores and put them back into`localStorage`

We'll call this function in two cases: losing and winning.

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

    if(player.dead){
```

```js
        addScore(score, false);
```

```js
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

**tile.js**

```js
class Exit extends Tile{
    constructor(x, y){
        super(x, y, 11, true);
    }

    stepOn(monster){
        if(monster.isPlayer){
            if(level == numLevels){
```

```js
                addScore(score, true);
```

```js
                showTitle();
            }else{
                level++;
                startLevel(Math.min(maxHp, player.hp+1));
            }
        }
    }
}
```

Our high scores are now quietly sitting in

`localStorage`

. You can check for yourself by simply typing "localStorage" into the console or taking a peek at the Application tab in the dev tools.



Let's display them.

**game.js**

```js
function showTitle(){                                          
    ctx.fillStyle = 'rgba(0,0,0,.75)';
    ctx.fillRect(0,0,canvas.width, canvas.height);

    gameState = "title";

    drawText("SUPER", 40, true, canvas.height/2 - 110, "white");
    drawText("BROUGH BROS.", 70, true, canvas.height/2 - 50, "white");
```

```js
    drawScores();
```

```js
}
```

```js
...
```

```js
function drawScores(){
    let scores = getScores();
    if(scores.length){
        drawText(
            rightPad(["RUN","SCORE","TOTAL"]),
            18,
            true,
            canvas.height/2,
            "white"
        );

        let newestScore = scores.pop();
        scores.sort(function(a,b){
            return b.totalScore - a.totalScore;
        });
        scores.unshift(newestScore);

        for(let i=0;i<Math.min(10,scores.length);i++){
            let scoreText = rightPad([scores[i].run, scores[i].score, scores[i].totalScore]);
            drawText(
                scoreText,
                18,
                true,
                canvas.height/2 + 24+i*24,
                i == 0 ? "aqua" : "violet"
            );
        }
    }
}
```

Don't panic. You could draw the scores in MUCH less code if you prefer, but we're taking our time to carefully sort and align the scores here. And it's also artificially long because we've split

`drawText`

arguments onto multiple lines for clarity.



Let me break it down:

- We get the scores and then only continue if we have some

- We draw a header row (RUN SCORE TOTAL) in the very middle of the canvas. There's a new utility method`rightPad`which we'll cover in a moment

- Next we take the most recent score off the end, sort the scores in descending order, and put that most recent score back at the beginning. This way you always see the last score at the top and in a different color... accomplished a few lines down by`i == 0? "aqua": "violet"`.

- We loop over at most 10 scores and draw each one slightly lower on the screen with`canvas.height/2 + 24 + i*24`

And what about

`rightPad`

? We're adding this so the scores are spaced out in a table format.

**util.js**

```js
function rightPad(textArray){
    let finalText = "";
    textArray.forEach(text => {
        text+="";
        for(let i=text.length;i<10;i++){
            text+=" ";
        }
        finalText += text;
    });
    return finalText;
}
```

We iterate over an array of strings representing a row of data. We pad out each string with spaces until it is 10 characters long and add it to the last string. We return the combined string, which should be a perfectly spaced out row of score data.



![high-scores](https://nluqo.github.io/broughlike-tutorial/screens/high-scores.png)

The

next section

adds some nifty animation and screenshake.
