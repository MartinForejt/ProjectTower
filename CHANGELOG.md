# Changelog

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
