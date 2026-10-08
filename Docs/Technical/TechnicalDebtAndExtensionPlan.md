# Technical Debt and Extension Plan

This file records known constraints so the team can distinguish deliberate prototype shortcuts from defects.

## 1. Current technical debt

| Priority | Debt | Impact | Recommended resolution |
| --- | --- | --- | --- |
| P0 | Flight-to-surface context uses static fields and scene names. | Fragile when multiple destinations, saves, or interrupted loads are added. | Introduce `GameSessionState` and a typed `SurfaceTransitionContext` keyed by planet ID. |
| P0 | Surface discovery is an integer `HashSet` local to the scene. | Progress is lost on scene reload and can break when target order changes. | Stable objective IDs plus a session/save progression service. |
| P1 | `SpaceFlightPrototype` hard-codes destination indices and thresholds. | Every new planet requires code edits and risks index mismatch. | Destination registry with `PlanetData` entries and travel rules. |
| P1 | Surface content is generated inside one large prototype class. | Terrain, props, markers, HUD and transitions are difficult to test independently. | Split into content builder, interaction registry, mission adapter and presentation controller. |
| P1 | Input is polled directly from `Keyboard.current` and `Mouse.current`. | Rebinding, gamepad, accessibility and automated input testing are difficult. | Unity Input Actions or an injected input interface. |
| P1 | Buildings have no physical colliders in the current slice. | Exploration is easy to test but lacks believable physical space and occlusion. | Add authored simplified colliders and interaction anchors per prefab. |
| P1 | Island, ocean and shoreline are low-poly runtime placeholders. | Visual quality and navigation scale are not production-ready. | Move to authored/seeded terrain chunks with material, LOD and water systems. |
| P2 | Materials and meshes are frequently created at runtime. | Increased memory churn and reduced editor visibility. | Share authored materials; centralize procedural asset ownership and pooling. |
| P2 | Runtime HUD is built from legacy `UnityEngine.UI.Text`. | Limited localization, styling and responsive layout. | Replace with a UI prefab system and localized string keys. |
| P2 | No standalone build/release pipeline is documented. | Testers without Unity cannot run the game reliably. | Add CI build profiles, artifact naming, and GitHub Releases checklist. |

## 2. Recommended production interfaces

These are proposed names and responsibilities, not current classes:

```text
IGameModeService
  Enter(mode, context)
  Exit(mode)

ITravelService
  CanTravel(destinationId, TravelContext)
  RequestLanding(destinationId)

IPlanetContentProvider
  Load(planetId, SurfaceLoadContext)
  Unload(planetId)

IInteractableRegistry
  Register(IInteractable)
  FindFocused(Ray, distance)

IMissionService
  ApplyEvent(GameplayEvent)
  GetProgress(missionId)

ISessionStateStore
  Read()
  Write(GameSessionState)
```

Keep these interfaces in a neutral `Core` or `Gameplay` layer. Flight and surface presentation may implement adapters, but neither mode should directly depend on a concrete save file, UI prefab, or combat scene.

## 3. Delivery order

1. Stabilize the current flight → land → scan → return loop and add a repeatable validation checklist.
2. Replace static return flags with a typed transition/session state.
3. Convert scan targets to stable interactables and mission events.
4. Extract surface generation into data-driven content definitions.
5. Add authored terrain, water and collider kits while retaining the procedural fallback.
6. Move destination selection to a data-driven registry and add a second planet through that path.
7. Add streaming and fly-by transition only after the two-mode state model is stable.
8. Add durable saves, release builds and automated build validation.

## 4. Non-goals for the current slice

The following should not be smuggled into the flight/surface prototype without a design decision:

- Seamless planet-scale traversal.
- Swimming, underwater gameplay or water physics.
- Combat rules inside the surface controller.
- Persistent economy or faction progression.
- Runtime procedural generation without a reproducible seed.
- High-fidelity lighting or final art materials.

Keeping these explicit preserves the small, testable landing slice while leaving clean extension points for the full game.
