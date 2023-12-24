# Missing states
// TODO Implement the states described below

These are special states bc the user must implement their own logic for when (if ever) to transition to them.
No other built-in state will ever transition to them.

## DeadState
This state prevents the player from controlling the character and, optionally, turns the character into a physics body.

## Hit
This state prevents the player from controlling the character for a brief duration and, optionally, moves the character
slightly.

## Attacking
This state limits or reduces the player's control over the character for a short duration and, optionally, perform short
movements.

## AirAttacking
Same as Attacking, but the character is in the air.

## Floating
This state can be used to simulate flying, swimming, zero gravity, Ori-like burrowing, or any other similar mechanic
that allows the character to move in all directions.

## Ledge Grabbing
If the character touches the wall while they are at a certain height difference from a floor on top of that wall, they
grab the edge of the floor and climb to the top.

## Ladder Climbing
Character climbs a vertical ladder up or down

## Gliding
Character can slow down the fall

## Interacting
The character is interacting with the world. They might be opening a door, grabbing an item on the floor, operating a
lever, or anything else that requires them to remain still for a short time.
