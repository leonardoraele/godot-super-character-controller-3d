# Missing states
// TODO Implement the states described below

These are special states bc the user must implement their own logic for when (if ever) to transition to them.
No other built-in state will ever transition to them.

## StaticState
This state prevents the player from controlling the character and, optionally, turns gravity off. This can be used to
represent the character being dead, being hit-stunned, or performing some action that disallows it to move, like
attacking, picking an item, opening a door a chest, operating a lever, etc.

This state can only be activated or deactivated by other scripts.

## FloatingState
This state can be used to simulate flying, swimming, zero gravity, mairio-like fence climbing, Ori-like burrowing, and
other similar mechanics that allow the character to move in all directions.

This state can only be activated or deactivated by other scripts.

## LedgeGrabbingState
If the character touches the wall while they are at a certain height difference from a floor on top of that wall, they
grab the edge of the floor and climb to the top.

## LadderClimbingState
Character climbs a vertical ladder or rope, up or down.

This state can only be activated or deactivated by other scripts.

## GlidingState
Character can slow down the fall.

The settings determine whether it is triggered by holding the jump input action or by pressing a defined input action
during the fall. Settings also determine whether the state ends when the input action is released or not. (if not, then
it ends when jump or dash input actions is pressed)
