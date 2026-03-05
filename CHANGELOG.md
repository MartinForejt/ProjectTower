# Changelog

## [0.3.0] - 2026-03-06

### Added
- **UI Scaling**: All HUD/menus use `GUI.matrix` scaling based on 1080p reference resolution
- **Pause Menu**: ESC key toggles pause, with Resume, Settings, Main Menu, Exit options
- **Settings Panel**: Master Volume and SFX Volume sliders (in both pause menu and main menu)
- **Enemy Types**: Warrior, Scout (fast/weak), Tank (slow/tough), Archer (ranged attack)
- **Walking Animation**: Enemy limbs (arms/legs) animate with sinusoidal rotation
- **Target Spreading**: Turrets score targets using `distance + (targetedBy * 8)` to distribute fire

### Changed
- **Wall System**: Reworked to single wall ring entity (1000g), shared HP pool, shield at level 5, must rebuy if destroyed
- **Mine System**: Now a single pre-placed upgradable building (no longer placeable)
- **Main Menu**: Simplified to New Game, Settings, Exit with gold accent styling
- **GameHUD**: Larger panels, wall HP/shield bars in top bar, turret upgrade button

### Fixed
- Volume controls now affect all sound playback via `SoundManager.MasterVolume/SFXVolume`

## [0.2.0] - 2026-03-05

### Added
- **Chunked Voxel Terrain**: 8x7 chunks, 50x14x50 voxels each at vs=0.4, with MeshColliders
- Flat playable area around tower, rolling hills at edges
- Biome-based terrain coloring (gravel, moss, grass, dirt, scorched, flowers, forest)
- `TerrainSystem.DamageAt()` for explosion-driven terrain destruction
- **Smoke & Fire Effects**: Rocket/plasma explosions leave lingering fire glow and rising smoke columns
- **Physics Blood/Gore**: Enemy deaths spawn physics-driven blood chunks and droplets (replace flat red circles)
- **Dynamic Lighting**: `DynamicLight.cs` utility for temp/persistent point lights on projectiles, muzzle flashes, explosions
- **Animated Water Shader**: `Custom/AnimatedWater` with wave displacement, Fresnel, additional light response
- **Bloom & Vignette**: PS1PostProcess adds URP Volume with Bloom and Vignette

### Changed
- Terrain resolution ~6x increase (from vs=1.0 monolithic to vs=0.4 chunked)
- Sun with soft shadows added to atmospheric lights
- Explosion black scorch circles replaced with fire/smoke VFX

## [0.1.0] - 2026-03-05

### Added
- Project baseline: Unity 6 (6000.3.9f1) with URP
- **Main Menu** scene with New Game, Load Game, Settings, Exit buttons
- **Game Scene** with runtime-bootstrapped tower defense environment
- **Core systems**: GameManager (state machine), EconomyManager (coins), WaveManager (infinite wave scaling), BuildingSystem (place defenses/structures)
- **Tower**: central tower with health + shield (both upgradable), shield regeneration
- **Defenses**: Gun, Crossbow, Rocket Launcher, Plasma Gun — all with targeting, firing, and upgrade system
- **Projectile** system for ranged attacks
- **Enemies**: procedurally generated with difficulty scaling, boss enemies every 5 waves with magic shields, fireball attacks, and minion spawning
- **EnemySpawner**: spawns enemies in a ~160-degree arc below the tower
- **Buildings**: Mines (coin generation), Walls (with slots for troops/turrets), Moat (slows/damages enemies, upgradable with spikes)
- **HUD**: top bar (coins, wave info, tower HP/shield), build panel, tower upgrades, pause menu, game over screen
- **Camera**: angled perspective view with zoom, looking down at tower
- **PS1 post-process** baseline (low-res render target with point filtering)
- Setup phase with infinite time before starting waves
- Wave completion coin bonuses
