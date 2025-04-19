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
  - Layer as "Player" (create this layer)
  - Add Animator component if using animations
  - Attach the `NinjaController.cs` script

### 3. Enemy Types

#### Basic Enemy
- Create with Sprite Renderer and Collider2D
- Tag as "Enemy"
- Layer as "Enemy" (create this layer)
- No additional scripts needed

#### Patrolling Enemy
- Create with Sprite Renderer and Collider2D
- Add Rigidbody2D (Kinematic)
- Tag as "Enemy"
- Layer as "Enemy"
- Create two empty GameObjects as patrol points
- Attach `EnemyPatrol.cs` script and assign patrol points

#### Shooting Enemy
- Create with Sprite Renderer and Collider2D
- Tag as "Enemy"
- Layer as "Enemy"
- Create an empty GameObject as a child for the shoot point
- Create a bullet prefab (with Rigidbody2D, Collider2D as trigger, and `Bullet.cs` script)
- Tag the bullet as "Bullet"
- Attach `EnemyShooter.cs` and assign bullet prefab and shoot point

### 4. Trap Types

#### Gear Traps
- Create with Sprite Renderer and Collider2D (set as trigger)
- Tag as "Trap"
- Layer as "Hazard" (create this layer)
- Add Animator if using animations
- Attach `GearTrap.cs` script
- Configure movement parameters:
  - Set `moveDirection` to customize movement axis (e.g., Vector3.right for horizontal)
  - Enable/disable rotation with `shouldRotate`
  - Adjust `rotationSpeed` for visual spinning effect
  - Set movement distance, speed and delay time

#### Spike Traps
- Create with Sprite Renderer and BoxCollider2D
- Tag as "Hazard" (will be set automatically by script)
- Layer as "Hazard" (create this layer)
- Add Animator with "IsExtended" bool parameter if using animations
- Attach `SpikeTrap.cs` script
- Configure behavior type in the Inspector:
  - **TimedCycle**: Activates on a fixed timer
  - **PlayerProximity**: Activates when player is nearby
  - **OneTimeTriggered**: Activates once when triggered
  - **AlwaysActive**: Always dangerous (static spikes)

### 5. Goal Flag
- Create with Sprite Renderer and Collider2D (set as trigger)
- Add Animator if using animations
- Attach `FlagGoal.cs` script

## Required Tags
- "Player" - For the ninja
- "Enemy" - For all enemies
- "Trap" - For moving gear traps
- "Hazard" - For spike traps (set automatically by SpikeTrap script)
- "Bullet" - For enemy projectiles

## Required Layers
- "Player" - For the ninja character
- "Enemy" - For all enemy types
- "Platform" - For ground/walls
- "Hazard" - For deadly traps and spikes

## Layer Collision Matrix Setup
1. Go to Edit > Project Settings > Physics 2D
2. Configure the Layer Collision Matrix to ensure:
   - Player collides with Enemy, Platform, and Hazard
   - Enemy collides with Player and Platform
   - Hazard collides with Player

## Unity Settings
- For best results, set Time -> Fixed Timestep to 0.01 in Project Settings
- Ensure correct collision layers are set up using the Layer Collision Matrix

## Camera Setup
- Create a Camera GameObject that follows the level center
- Set the camera to Orthographic projection 