# Super Character 3D Controller (WIP)

⚠ This project is still a work in progress and is missing a lot of critical features and documentation. It's not ready to be used yet.

## Installation

In the project's `addons` folder, do:
```
git submodule add <this_repository_clone_uri> SuperCharacter3D
```

<!--
In its core, the SuperCharacter3DController is just a script that inherits CharacterBody3D and implements a simple state
machine. It could be coded in a few hours. The real meat of this package is the rich collection of flexible and reusable
pre-implemented states, plus its acompanying presets and debugging tool.

By moving all the complexity of the character controller to the state nodes, it becomes super easy to extend the
character controls with your own state implementations, since you will be working with the familiar
Godot.CharacterBody3D class.

For example, lets say you want to implement a new state where the character spins in place while the player holds a
button, and then releases all the built up energy to propell it forward. First, you will create a new state node. Then,
you will check, on _Process when the player presses the spin button, and you transition to your custom state. At this
point, the control of the character is delegated to your state. That means, you are in charge of updating its Velocity
and calling MoveAndSlide every physics frame, just like you normally would with your own custom character controllers
that you would make by extending CharacterBody3D. The SuperPlatformer3D code will not interfere with the motion of your
character while your custom state is active. If you don't call MoveAndSlide (or manipulate the character in any other
way), it won't do anything. At this point, what you might want to check every frame to see if the player releases the
spin button, and then transition the character to another state, which will be in charge of moving the character while
they are being propelled forward. You could implement your own state, but you could also reuse SuperPlatformer3D's
Propell state. This state is a flexible state that is capable of moving the character forward at higher speed than they
normally can. Just instantiate a PropellSettings object with the settings you want, then transition to PropellState.
Done! Here's a mockup code:

```gdscript
extends Node3D

func _enter(transition):
	get_parent().character.velocity = Vector2.zero

func _process(delta):
	if Input.is_action_just_pressed("character_spin"):
		# Remember to name your node "spin_node", or change this line to the name of your custom state node.
		get_parent().transition("spin_state")

func _process_state_active(delta):
	if Input.is_action_just_released("character_spin"):
		var propell_settings = PropellSettings.new()
		propell_settings.initial_speed_boost = 50
		get_parent().transition("propell_state", propell_settings)

func _physics_process_state_active(delta):
	# Reuses fall state to make the character fall while in this state, but discard horizontal velocity
	get_parent().character.velocity = Vector3.new(0, get_parent().get_node("fall").get_vertical_velocity(delta), 0)
```

## Features
- A bunch of movement options and abilities pre-implemented, inclusing: walk, strafe, turn, jump, jump cancling
	(variable jump height), joyotte jump, midair jump (double jump, triple jump), glide, fly, ledge grab, ledge strafe,
		sequence jump (bunny hop), ground pound, crouch, crawl, crouch jump, slide, dash, air dash, dive, grappling hook,
	sprint, super jump, ground pound, wall climb, wall slide, wall run, ladder/pole climb, rope swing, rail slide,
	and more.
- Highly customizable, with more than X configurable parameters, neatly subdivided into categories, and extensivelly
	documented (with gif examples) to adjust the controller to your game needs.
- Highly extensible: implement new states that can capture state transitions and reuse logic from existing states.
- Adaptable to any game genre: 3rd-person platformers, first-person shooters, 3rd-person shooters, adventure games, etc.
- Presets: Comes with X configuration presets, to serve as examples as well as starting points.
- Resources: Save different control & ability configurations for your character as resources in your project and switch
	between them at runtime to unlock abilities for the player.
- Assist options implemented: coyotte jump, input buffering.
- Live demo?

### Future Features

#### Future MotionStateControllers
- [ ] Virtual2DMovementController <!-- Uses a Path3D as a basis for constraining the character movement in the XZ plane to simulate a 2D game in 3D world, like Mega Man X8 and Metroid Dread. The initial draft/proposal for implementation is: 1) flattening the Y position of all points in the Path3D; then 2) instead of applying velocity to the character in 3D space, calculate the velocity of the character as if it was a 2D character controller, then moving the character in the Path3D by the amount the character should move in the horizontal (XZ) axis every frame, while preserving the Y position --!>
<!--
-->
