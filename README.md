# Telebot

**A 2D puzzle platformer about discovering what you can do — taught entirely without words.**

You wake up as a robot in a scrapyard, and find you can teleport. Everything else, you figure out by playing.

 [⬇ Download build](https://om0rl.itch.io/telebot) · [🎬 Gameplay video](https://www.bilibili.com/video/BV1Lh7P6JEBJ)

![Teleport bullet](TODO_teleport.gif)

---

## Overview

Telebot is a side-scrolling 2D puzzle platformer built in Unity. The core mechanic is the **teleport bullet**: fire it, and it travels in a straight line until it hits something — then you swap places with it. Every puzzle in the game is built on the interaction between that bullet and the objects it hits.

The game currently runs as a linear sequence of rooms (roughly 20 minutes of play). The underlying systems are built for a metroidvania structure, which is the intended direction.

| | |
|---|---|
| **Engine** | Unity (URP) |
| **Language** | C# |
| **Scope** | Solo project — design, programming, shaders, sprites |
| **Play time** | ~20 minutes |

---

## Controls

Movement, jump, fire, interact, drag. The title screen lists the keys — that is the only text instruction in the game.

---

## Design Principles

The game has **no written tutorial**. Every mechanic has to be taught through level structure alone. Three principles drive that:

### 1. Introduce one identity at a time

When a new component appears, the level only demonstrates its **most prominent behavior** — nothing else.

A key wall is a solid wall that opens with a key. A jump switch is a solid block that toggles whenever the player jumps. That is all the player learns at first.

Interactions with other components and the subtler properties of each object are deliberately withheld. This gives the player a clean, confident mental model, and leaves room for that model to be extended later.

### 2. Learning through impossible situations

1. The player attempts the room the obvious way.
2. The attempt fails — the room appears unsolvable.
3. To progress, the player has to re-examine components they already understand and find a property they had not considered.

The failure is the teaching moment. The player is not told that a mechanic has hidden depth; they are placed in a situation where discovering it is the only way forward.

### 3. Learning through accident

1. The player is playing normally, not looking for anything.
2. Something unexpected happens.
3. The surprise itself carries the new information.

This covers mechanics that would feel arbitrary if gated behind a puzzle. Instead of requiring the discovery, the level is arranged so it is likely to happen on its own.

---

## Architecture

A hybrid grid system layered over Unity's Tilemap, driven by a static event system, with an interface-based extension model for gameplay objects.

### Grid system

Unity's Tilemap is efficient for static level geometry but cannot represent objects that need per-instance state and complex interactions. A pure grid implementation would have meant giving up Tilemap's tooling and physics integration.

Telebot uses both. **`GridManager`** is a singleton that maintains a unified view of the level: static tiles from Tilemap, plus a registry of **`GridObject`** instances for anything that needs richer behavior. Callers can query what occupies any grid position without caring which system owns it. This means player movement can stay physics-driven and continuous, while puzzle logic resolves on discrete grid coordinates.

Levels are divided into **`Zone`**s, each carrying its own obstacle tilemap, damage tilemap, and area-mask tilemap, along with its own list of grid objects.

### Event system

**`GameEvent`** is a static class holding the game's global events (C# `event`). Events that only concern one subsystem stay private to that subsystem. This keeps cross-system communication explicit and prevents modules from holding direct references to each other.

### Extension interfaces

New gameplay objects are added by implementing interfaces rather than modifying core systems:

| Interface | Purpose |
|---|---|
| `IInteract` | Object responds to the player's interact key |
| `IBulletInteract` | Object defines its own response when the teleport bullet hits it |
| `ICanDrag` | Object can be dragged by the player within the current room |

A new mechanic only needs to implement the relevant interface to be picked up by the interact, teleport, and drag systems — the core logic does not change. Six interactive component types are implemented so far.

### Core systems

| System | Responsibility |
|---|---|
| `GridManager` | Hybrid Tilemap / GridObject grid, zone and object registry |
| `GameEvent` | Global event broadcast |
| `RoomManager` | Camera control, room transitions, room-bound utilities (rooms are fixed size) |
| `InputController` | Player state, which inputs are legal in each state, state transitions (4 states) |
| `TeleportBulletSystem` | Aim mode entry/exit, bullet firing; `OnHitInfo` carries hit data to listeners |
| `DragSystem` | Drag mode entry/exit for `ICanDrag` objects |
| `PlayerGridObj` | Player's grid representation and the list of ground objects beneath it |
| `PlayerRespawnController` | Death and respawn to the last registered flag |
| `InventorySystem` | Storage for bullet modifiers |
| `ThemeManager` / `ThemeController` | Swaps global volume and palette material via `ThemeData` ScriptableObjects |

---

## Shader work

**Palette swap shader (Shader Graph).** Sprites are authored in grayscale using four preset shades. The shader separates pixels by brightness and remaps each band to an arbitrary color, so a full palette change happens at runtime without drawing a second copy of any sprite. Paired with `ThemeData` ScriptableObjects, an entire area's color scheme can be swapped in one call — sprites, UI, and tilemaps together.

---

## Roadmap

- **Audio.** Currently missing, and several mechanics would benefit from a sound cue.
- **Metroidvania structure.** Rooms that stay unsolvable until the player learns something elsewhere, backtracking, and secret rooms found through environmental cues.
- **Real levels.** Most existing rooms teach a mechanic. The systems support far more than the levels currently ask of them.
- **Areas 2–4.** A number of implemented mechanics are unused; the framework is built to extend.
- **Narrative.** Storyboard cutscenes to frame the setting.

---

## Known issues

The Unity version used for this build has an upstream bug that can throw a Job System error when particle systems are active, causing an occasional crash during long sessions. Particle usage is currently avoided as a workaround; an engine version upgrade is planned.

---

## Credits

Base 2D movement uses a script from the DigitalWorld package.

Design references: **Öoo** (the primary influence), *Stephen's Sausage Roll*, *The Witness*, *Outer Wilds*, *Can of Wormholes*.

---

*Solo project by Yashuajia. Feedback welcome.*
