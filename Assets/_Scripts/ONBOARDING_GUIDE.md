# UnitedFights Developer Onboarding Guide

Welcome to the **UnitedFights** development team! This document is designed to get you up to speed with the codebase, architecture, and core systems of the project.

## 1. Project Overview

**UnitedFights** is a **Roguelike Deckbuilder** with **Auto-Chess elements**.
- **Core Loop**: Players traverse a generated map, fighting turn-based battles, collecting cards/perks, and upgrading heroes.
- **Unique Mechanic**: **Hero Evolution**. Collecting 3 copies of the same hero automatically merges them into a more powerful version (Tier 2 -> Tier 3), similar to Auto-Chess games.
- **Meta-Game**: Features a backend integration (Firebase) for leaderboards, match history, and player progression.

---

## 2. Architecture & Core Systems

The project follows a **Manager-System-Action** architecture designed for modularity and complex interaction chains.

### A. The Manager Layer (Singletons)
These classes manage global state and persist across scenes.
- **`GameManager`**: The central brain. Tracks active heroes, the master deck, perks, and handles the **Evolution Logic**.
- **`MapSystem`**: Generates the roguelike map (nodes for Combat, Shop, Rest, Elite, Boss). Handles scene transitions based on node selection.
- **`ActionSystem`**: The execution engine. **Crucial**: Almost all gameplay logic goes through this system.
- **`FirebaseManager` & `ScoreManager`**: Handles authentication, cloud saves, and leaderboards.

### B. The Action System (`ActionSystem.cs`)
This is the most important system to understand. The game does not simply "call functions" to do things; it creates **Actions** (`GameAction`).
- **Flow**: When an action occurs (e.g., `DealDamage`), it passes through 3 phases:
    1. **Pre-Reactions**: Listeners can modify the action *before* it happens (e.g., "Block" reduces damage).
    2. **Perform**: The actual logic executes (e.g., Health is subtracted).
    3. **Post-Reactions**: Listeners trigger *after* the action (e.g., "OnHit" effects).
- **Why?**: This allows for complex relic/perk interactions (e.g., "Gain 1 Strength whenever you play an Attack").

### C. The Systems Layer (`Assets/_Scripts/System`)
Logic is broken down into specific systems rather than monolithic classes.
- **`DamageSystem`**: Calculates damage, applies armor reduction.
- **`PoisonSystem` / `BurnSystem`**: Manages status effects per turn.
- **`MapGenerator`**: Algorithms for creating the node path.
- **`CardSystem`**: Manages drawing, discarding, and playing cards.

---

## 3. Key Data Structures (ScriptableObjects)

The game is highly data-driven. Content is created by making new ScriptableObjects in the Project view.

### `HeroData`
Defines a playable character.
- **Stats**: Health, Cost.
- **Deck**: Starting cards (`List<CardData>`).
- **Perks**: Passive abilities (`List<PerkData>`).
- **Evolution**: `NextEvolution` field points to the upgradable version of this hero.

### `CardData` (Presumed)
Defines a card.
- **Cost**: Mana cost.
- **Effects**: Referenced `GameAction`s (e.g., `DealDamageGA`, `GainArmorGA`).

### `MapData`
Holds the current state of the run's map (Nodes, Connections, Current Position).

---

## 4. The Gameplay Loop

### Phase 1: Preparation (Menu/Map)
1. **`GameManager`** loads. User logs in via `AuthManager`.
2. **`MapSystem`** generates a map.
3. Player selects a node.
    - If **Combat**: Loads `unitedfights` scene.
    - If **Shop/Rest**: Opens respective UI overlay.

### Phase 2: Combat (`unitedfights` scene)
1. **`MatchSetupSystem`** initializes the board, spawning Heroes (`HeroInstance`) and Enemies.
2. **Turn Loop**:
    - **Player Turn**: Draw cards -> Play cards (trigger `Gameactions`) -> End Turn.
    - **Enemy Turn**: Enemies execute intent (Attack, Buff, Debuff).
3. **Win/Loss**:
    - **Win**: Reward screen -> Return to Map.
    - **Loss**: Game Over -> Score submitted to Firebase.

---

## 5. Backend Integration (Firebase)

We use Google Firebase for backend services.
- **Authentication**: Email/Password login.
- **Firestore (Database)**:
    - `users/`: Stores Elo, High Score, Tier (Bronze/Silver/Gold).
    - `matchHistory/`: Logs every match result.
    - `herostats/`: Tracks wins/losses per hero.

**`ScoreManager.cs`** is your main interface for database operations. It handles checking for user documents and creating them if they don't exist.

---

## 6. How to Contribute

### Adding a New Hero
1. Create a `HeroData` asset.
2. Assign sprite, health, and starting deck.
3. **Important**: If this hero evolves, create the Tier 2 and Tier 3 versions first, then link them in the `NextEvolution` field.

### Adding a New Game Mechanic
1. Create a new `GameAction` (e.g., `StunAction`).
2. Create a System to handle it if complex (e.g., `StunSystem`).
3. Add it to `ActionSystem`'s logic if it needs verifying order of operations.

### Debugging
- **Common Crash**: Firebase dependencies not found. Ensure the Google Services file is present.
- **Map Issues**: If map doesn't generate, check `MapGenerator` parameters.
- **Logic Bugs**: Use `Debug.Log` inside `ActionSystem.Flow` to trace the sequence of events.

---

## 7. Important Directories
- `Assets/_Scripts/GameActions`: Specific logic for card effects.
- `Assets/_Scripts/System`: Core game logic modules.
- `Assets/_Scripts/Manager`: Global state managers.
- `Assets/_Scripts/Data`: ScriptableObject definitions.
