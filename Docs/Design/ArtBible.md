# Art Bible

**Status:** Prototype art direction  
**Applies to:** Space, ships, planets, surface environments and UI

## Visual identity

Game Frontier uses grounded science fiction with readable silhouettes, large environmental scale and restrained technology. The world should feel unfamiliar without becoming visually noisy. Exploration locations combine natural planetary forms with compact human or alien infrastructure.

## Core visual principles

1. **Readable at a distance.** Planets, ships, landing pads and major structures need distinct silhouettes.
2. **One dominant identity per location.** A planet should have a clear color, climate and material story.
3. **Technology belongs to its environment.** Buildings need foundations, paths, access points and believable scale.
4. **Atmosphere supports navigation.** Lighting and fog should guide the player rather than hide objectives.
5. **Prototype assets remain replaceable.** Generated models must not determine permanent architecture or scene structure.

## Azure World direction

- Environment: dark ocean world with a compact research settlement.
- Palette: deep blue-green water, muted rock and soil, cool white structures, cyan navigation accents.
- Terrain: irregular island silhouette, uneven volcanic surface and a readable shoreline.
- Lighting: overcast or low-angle cool light with controlled highlights on technology.
- Mood: isolated, scientific and slightly mysterious.

## Scale rules

- Unity uses metres as the working unit.
- Player eye height should remain near 1.6 metres.
- Doors should visually support a human height near 1.8 metres.
- Habitable buildings must clearly exceed player height.
- Landing pads must provide visible clearance around the ship.
- Structures should be evaluated from the playable camera, not only in the Scene view.

## Model guidelines

- Keep the model upright with a predictable Y-up import result.
- Place the pivot at ground centre when possible.
- Use separate objects for animated or interactive parts.
- Avoid hidden geometry and fragmented image-to-3D debris.
- Use simple authored colliders; visual mesh bounds should not automatically become gameplay collision.
- Prototype target: approximately 3,000–20,000 triangles for a medium environmental prop, adjusted for screen size and reuse.

## Texture and material guidelines

- Use physically based base color, normal, metallic and roughness/smoothness maps.
- Prefer 1K textures for small props and 2K for major hero structures during production.
- Avoid baking dramatic lighting into base color textures.
- Reuse trim sheets and material families where practical.
- Record source and license information for every external or AI-generated asset.

## Space visual language

- Use darkness and sparse light to preserve depth.
- Separate navigable objects from background stars through scale, motion and HUD treatment.
- Avoid filling the scene with equally bright objects.
- Destination markers should remain readable without replacing visual navigation.

## UI visual language

- Use cool cyan for navigation and scanning.
- Use warm colors only for danger, damage or irreversible warnings.
- Keep text blocks short and aligned to safe screen margins.
- Maintain a consistent hierarchy: location, objective, progress, contextual action.

## Asset review checklist

- Correct orientation and real-world scale.
- Clean silhouette from gameplay distance.
- Coherent texture set and material assignment.
- No unintended holes, floating fragments or embedded background geometry.
- Correct pivot and ground contact.
- Collision matches gameplay intent.
- Source, generator, license and prompt/reference recorded.
- Performance budget appropriate for its expected count.

