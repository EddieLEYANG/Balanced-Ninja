# Balance Ninja - Setup Instructions

## Game Overview
Balance Ninja is a 2D physics-based platformer where the player controls a ninja assassin by tilting the level itself using mouse drag controls. The objective is to eliminate all enemies in each level and reach the flag to progress.

## Setting Up a Level

### 1. Level Structure
- Create an empty GameObject named "Level"
- Add a Rigidbody2D component with:
  - Body Type: Dynamic
  - Gravity Scale: 0
  - Angular Drag: ~0.5 (adjust for feel)
  - Constraints: Freeze Position X, Y
- Attach the `LevelDragRotation.cs` script

### 2. Player Setup
- Create a player GameObject with:
  - Sprite Renderer with your ninja sprite
  - Rigidbody2D (Mass: 1, Linear Drag: 0.5)
  - Collider2D (Box or Circle) 
  - Tag as "Player"
  - Add Animator component if using animations
  - Attach the `NinjaController.cs` script

### 3. Enemy Types

#### Basic Enemy
- Create with Sprite Renderer and Collider2D
- Tag as "Enemy"
- No additional scripts needed

#### Patrolling Enemy
- Create with Sprite Renderer and Collider2D
- Add Rigidbody2D (Kinematic)
- Tag as "Enemy"
- Create two empty GameObjects as patrol points
- Attach `EnemyPatrol.cs` script and assign patrol points

#### Shooting Enemy
- Create with Sprite Renderer and Collider2D
- Tag as "Enemy"
- Create an empty GameObject as a child for the shoot point
- Create a bullet prefab (with Rigidbody2D, Collider2D as trigger, and `Bullet.cs` script)
- Tag the bullet as "Bullet"
- Attach `EnemyShooter.cs` and assign bullet prefab and shoot point

### 4. Traps
- Create with Sprite Renderer and Collider2D (set as trigger)
- Tag as "Trap"
- Add Animator if using animations
- Attach `Trap.cs` script for moving traps

### 5. Goal Flag
- Create with Sprite Renderer and Collider2D (set as trigger)
- Add Animator if using animations
- Attach `FlagGoal.cs` script

## Required Tags
- "Player" - For the ninja
- "Enemy" - For all enemies
- "Trap" - For spike traps
- "Bullet" - For enemy projectiles

## Unity Settings
- For best results, set Time -> Fixed Timestep to 0.01 in Project Settings
- Ensure all collision layers are set up to interact properly

## Camera Setup
- Create a Camera GameObject that follows the level center
- Set the camera to Orthographic projection 