# Tower Defense / RTS Hybrid (INCOMPLETE)

A Unity 6 prototype combining tower-defense and real-time strategy gameplay. Features a multi-tier pathfinding system, ORCA local avoidance, a DOTS/ECS parallel movement pipeline, and dynamic grid-based world representation.

<img width="475" height="315" alt="Screenshot 2026-04-03 at 7 10 18 PM" src="https://github.com/user-attachments/assets/995f0d39-e196-46d2-b28a-5cee5f8c848c" />

---

> **Status:** While core systems are playable, the game is incomplete; several subsystems are in active migration or partial implementation. As such, this project currently serves only as a demo of these core mechanics.

---

## Gameplay

1. Start from the main menu and enter the game scene
2. Place buildings on a snapped grid subject to territory, buildability, and resource checks
3. Select and command squads — troops spread to avoid clumping and automatically switch to shared flow-field routing for larger groups
4. Defend against periodic enemy waves while managing your economy

---

## Technical Highlights

### Three-tier navigation (`PathfindingManager.cs`)
Movement strategy is selected dynamically based on context:

| Tier | When used | Implementation |
|------|-----------|----------------|
| Direct steering | Short-range moves | Straight vector to target |
| A\* waypointing | Long-range moves | Grid-based `GridPathfinder.cs` |
| Shared flow fields | Group movement | `FlowfieldBuilder.cs` with shared caching |

### Flow field caching and lifecycle
Flow fields are reused across nearby destinations rather than recomputed per-unit. The cache tracks reference counts per field, evicts by age, and invalidates on terrain change — making group movement practical at RTS unit counts.

### Regional flow field generation with warm start (`FlowfieldBuilder.cs`)
Flow fields can be generated over sub-regions of the world and optionally seeded from a previous field for faster convergence. This reduces per-frame computation cost when the destination changes slightly.

### ORCA local avoidance from scratch (`ORCASolver.cs`, `ORCAManager.cs`)
Full Optimal Reciprocal Collision Avoidance implementation including half-plane constraint construction and linear programming. Spatial hashing in `ORCAManager.cs` limits neighbour queries to nearby agents. Runs as a separate system alongside pathfinding.

### DOTS/ECS parallel movement pipeline (`TroopMovementSystem.cs`, `ORCASystem.cs`)
An experimental ECS movement stack schedules movement and ORCA jobs in parallel using Unity's Job System with Burst compilation. Maintained alongside the classic MonoBehaviour stack for comparison and gradual migration.

### Classic ↔ ECS bridge (`GridECSBridge.cs`, `FlowFieldBridgeSystem.cs`)
Grid walk costs are synchronised into ECS-native arrays through `GridECSBridge`, while computed flow directions are fed back to ECS entities through `FlowFieldBridgeSystem`. Keeps both pipelines consistent without duplicating world state.

### Sectorized world (`GridManager.cs`, `SectorManager.cs`)
The grid is partitioned into sectors and connected for higher-level spatial reasoning. Lays groundwork for hierarchical pathfinding (sector-level A* + intra-sector flow fields).

### Dynamic buildability and nav invalidation (`GridManager.cs`)
When a structure is placed or removed, one pipeline updates occupancy, walk costs, sector aggregates, ECS sync, and flow-field cache invalidation — keeping navigation correctness coupled to the economy system automatically.

---

## Architecture

```
Assets/Scripts/
  │
  ├── Managers/
  │   ├── BaseManager.cs            Global singleton base
  │   ├── GridManager.cs            Grid state, buildability, nav invalidation
  │   ├── SectorManager.cs          Sector partitioning and connectivity
  │   ├── PathfindingManager.cs     Three-tier nav strategy, flow-field cache
  │   └── FactionManager.cs         Faction tracking
  │
  ├── Pathfinding/
  │   ├── GridPathfinder.cs         A* on grid
  │   ├── FlowfieldBuilder.cs       Regional flow field generation, warm start
  │   ├── ORCASolver.cs             ORCA half-plane LP solver
  │   └── ORCAManager.cs            Spatial hashing, ORCA integration
  │
  ├── DOTS/
  │   ├── TroopMovementSystem.cs    ECS parallel movement jobs
  │   ├── ORCASystem.cs             ECS ORCA jobs
  │   ├── GridECSBridge.cs          Grid cost → ECS native arrays
  │   └── FlowFieldBridgeSystem.cs  Flow directions → ECS entities
  │
  ├── Troop/
  │   ├── TroopAI.cs                Controller / delegator
  │   ├── TroopStateMachine.cs      State logic
  │   ├── TroopTargetSelector.cs    Target selection (in progress)
  │   ├── TroopMovement.cs          Movement execution
  │   ├── TroopManagment.cs         Squad command, spread, flow-field handoff
  │   └── TroopCombatSystem.cs      Troop combat specialisation
  │
  ├── Combat/
  │   ├── CombatSystem.cs           Health, damage, status effects
  │   └── BuildingCombatSystem.cs   Building combat specialisation
  │
  ├── Build/
  │   ├── BuildMode.cs              Placement, snapping, territory/cost checks
  │   └── BuildMenu.cs              UI controller
  │
  ├── Wave System/
  │   ├── WaveScript.cs             Wave scheduling
  │   └── EnemySpawnScript.cs       Enemy instantiation
  │
  └── Economy/
      ├── ResourcePool.cs           Global resource state
      ├── ResourceBuilding.cs       Building income
      └── WoodResourceNode.cs       Resource node
```

---

## Dependencies

| Package | Use |
|---------|-----|
| Entities (DOTS) | ECS movement pipeline |
| Input System | Player input |
| Cinemachine | Camera control |
| AI Navigation | (supplementary nav mesh) |
| URP | Rendering pipeline |

---

## Scenes

| Scene | Purpose |
|-------|---------|
| Start Menu | Main menu |
| GameScene | Core gameplay |
| TestScene | Pathfinding mode comparison harness |

---
