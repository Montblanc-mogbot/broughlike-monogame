# Broughlike tutorial - Stage 4

Source: https://nluqo.github.io/broughlike-tutorial/stage4.html

[JavaScript Broughlike Tutorial](index.html)[Previously: Monsters](stage3.html)

# Stage 4 - Monsters Part 2

Since the monsters will be attacking the player and vice versa, let's first draw the HP for each.

**monster.js**

```js
    draw(){
        drawSprite(this.sprite, this.tile.x, this.tile.y);
```

```js
        this.drawHp();
```

```js
    }
```

```js
    drawHp(){
        for(let i=0; i<this.hp; i++){
            drawSprite(
                9,
                this.tile.x + (i%3)*(5/16),
                this.tile.y - Math.floor(i/3)*(5/16)
            );
        }
    }
```

Monster HP now displays nicely.



The idea here is to draw one HP pip sprite for each unit of HP the monster has. We can't just draw each pip in the same spot; you would only see the top one in that case. We've got a bit of funky math to layout all the pips:



- **`5/16`**- Since`drawSprite`operates on a sprite index we normally pass in whole numbers representing 16 pixel sprites. However, we can instead work in individual pixels by using fractions. So`5/16`means 5 pixels within a 16 pixel sprite.

- **`i%3`**- this resets to 0 every 3 pips.

- **`Math.floor(i/3)`**- this increases by one every 3 pips.

The result means pips are drawn first left to right offset by 5 pixels each and then stacked vertically offset by 5 pixels for each row.

![hp](https://nluqo.github.io/broughlike-tutorial/screens/hp.png)

## Attacking

Now onto attacking. We can let the monsters attack the player and vice versa with a small addition to

`tryMove`

.

**monster.js**

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
```

```js
            }else{
                if(this.isPlayer != newTile.monster.isPlayer){
                    newTile.monster.hit(1);
                }
```

```js
            }
            return true;
        }
    }
```

```js
    hit(damage){
        this.hp -= damage;
        if(this.hp <= 0){
            this.die();
        }
    }

    die(){
        this.dead = true;
        this.tile.monster = null;
        this.sprite = 1;
    }
```

We're comparing

`isPlayer`

flags to make sure monsters don't attack each other.



When an attack is successful, that triggers

`hit`

, applying damage to the target monster's HP and if they run out of HP, they

`die`

. When dying, the monster sprite is set to index

`1`

(our player corpse). This will only apply to the player since we earlier wrote code to delete monsters as soon as they are

`dead`

.



Cool beans! Attacking is in the game. If you let the monsters kill you, you'll notice that you're still able to move around the map as a corpse. We'll tackle that later.



Monsters are working as expected, but with identical behavior. Here's the plan for making each one unique:

- **Bird:**our basic monster with no special behavior

- **Snake:**moves twice (yes, basically copied from 868-HACK's Virus)

- **Tank:**moves every other turn

- **Eater:**destroys walls and heals by doing so

- **Jester:**moves randomly

## Snake

Since

`Bird`

is already done, let's start with

`Snake`

. Make sure to test out each monster after updating their code. While testing this code, it may be easier to temporarily modify the

`spawnMonster`

code to only generate a specific kind of monster (left as an exercise for the reader). Or you can just refresh a bunch of times.

**monster.js**

```js
class Snake extends Monster{
    constructor(tile){
        super(tile, 5, 1);
    }
```

```js
    doStuff(){
        this.attackedThisTurn = false;
        super.doStuff();

        if(!this.attackedThisTurn){
            super.doStuff();
        }
    }
```

```js
}
```

Rather simple. The Snake can move twice, move and attack, but not attack twice (that's overpowered!). We need one tie-in within tryMove to set

`attackedThisTurn`

to true upon attacking.

**monster.js**

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
            }else{
                if(this.isPlayer != newTile.monster.isPlayer){
```

```js
                    this.attackedThisTurn = true;
```

```js
                    newTile.monster.hit(1);
                }
            }
            return true;
        }
    }
```

## Tank

While working on the Tank, we'll introduce a

`stunned`

flag. When a monster is stunned, they'll be unable to react until the next turn.



We'll be able to use this flag in multiple ways: to stun monsters whenever they are hit by the player or hit by certain spells and to pause the action of monsters like the Tank.

**monster.js**

```js
    tryMove(dx, dy){
        let newTile = this.tile.getNeighbor(dx,dy);
        if(newTile.passable){
            if(!newTile.monster){
                this.move(newTile);
            }else{
                if(this.isPlayer != newTile.monster.isPlayer){
```

```js
                    this.attackedThisTurn = true;
```

```js
                    newTile.monster.stunned = true;
```

```js
                    newTile.monster.hit(1);
                }
            }
            return true;
        }
    }
```

When monsters are attacked, they get

`stunned`

, making it easier for the player to take on tough monsters.

**monster.js**

```js
    update(){
```

```js
        if(this.stunned){
            this.stunned = false;
            return;
        }
```

```js
        this.doStuff();
    }
```

If the

`stunned`

flag is true, we reset it to false and do a

`return`

which exits the function and prevents the monster from doing anything until next turn.

**monster.js**

```js
class Tank extends Monster{
    constructor(tile){
        super(tile, 6, 2);
    }
```

```js
    update(){
        let startedStunned = this.stunned;
        super.update();
        if(!startedStunned){
            this.stunned = true;
        }
    }
```

```js
}
```

Here, the

`Tank`

monster stuns itself if it wasn't already

`stunned`

at the beginning of the turn. Effectively, this results in action only every other turn.

## Eater

Then comes the

`Eater`

. Before doing normal monster behavior, this guy is going to check for any nearby walls and eat them for health! Each wall will grant half a health point (our

`drawHp`

method only draws whole points though).

**monster.js**

```js
class Eater extends Monster{
    constructor(tile){
        super(tile, 7, 1);
    }
```

```js
    doStuff(){
        let neighbors = this.tile.getAdjacentNeighbors().filter(t => !t.passable && inBounds(t.x,t.y));
        if(neighbors.length){
            neighbors[0].replace(Floor);
            this.heal(0.5);
        }else{
            super.doStuff();
        }
    }
```

```js
}
```

First, we need to get all the nearby walls using

`getAdjacentNeighbors`

and only include tiles that are not

`passable`

(indicating a wall) and are also

`inBounds`

(so the outer wall doesn't get destroyed).



If walls are found, we're going to call two new methods. The

`replace`

method is replacing a

`Wall`

tile with a

`Floor`

tile. The

`heal`

method adds half a hitpoint to the monster. If no walls are found, we'll simply do the normal monster behavior.



Now let's implement those methods.

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
    heal(damage){
        this.hp = Math.min(maxHp, this.hp+damage);
    }
```

This method

`heal`

is a one-liner. Add some amount of healing "damage" without going over some global

`maxHp`

, which we'll need to define next. We don't want our monsters to gain infinite health!

**index.html**

```js
<script>
    tileSize = 64;
    numTiles = 9;
    uiWidth = 4;
    level = 1;
```

```js
    maxHp = 6;
```

Next up is

`replace`

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
    replace(newTileType){
        tiles[this.x][this.y] = new newTileType(this.x, this.y);
        return tiles[this.x][this.y];
    }
```

You can use

`replace`

any time one tile type changes into another type. Here it's a wall replacing a floor, but imagine if a water tile replaced a floor!



One thing that's not coded here is to copy over monsters and items present on the old tile to the new tile. Keep that in mind for future additions.

## Jester

The last monster is the

`Jester`

and it's able to move randomly simply by trying to move to the first neighbor returned by the (pre-shuffled)

`getAdjacentPassableNeighbors`

.

**tile.js**

```js
class Jester extends Monster{
    constructor(tile){
        super(tile, 8, 2);
    }
```

```js
    doStuff(){
        let neighbors = this.tile.getAdjacentPassableNeighbors();
        if(neighbors.length){
            this.tryMove(neighbors[0].x - this.tile.x, neighbors[0].y - this.tile.y);
        }
    }
```

```js
}
```

With those enemy behaviors in place, our little broughlike is starting to feel like... a game. 😍

![behavior](https://nluqo.github.io/broughlike-tutorial/screens/behavior.gif)

In the

next section

, we'll turn this thing into a proper game with a title screen, multiple levels, and victory and failure conditions.
