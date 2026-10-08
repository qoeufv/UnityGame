# Asset and Scene Pipeline

**Status:** Current conventions and production migration rules

## 1. Scene conventions

The current scene assets are:

- `Assets/Scenes/SpaceFlightPrototype.unity`
- `Assets/Scenes/Planet_AzureWorld.unity`
- `Assets/Scenes/Planet_PaleMoon.unity`

Each scene is a bootstrap entry point. Runtime composition is created by a root object with `SpaceFlightPrototype` or `PlanetSurfacePrototype`. `Tools/Open Space Flight Prototype` and `Tools/Setup Planet Exploration Prototypes` are the supported editor setup paths.

### Rules

1. Keep bootstrap scenes small and deterministic.
2. Register every loadable scene in the active Build Profile through the setup tool.
3. Do not depend on an object that exists only because a previous scene was open in the editor.
4. Keep authored content in prefabs or scene chunks once it needs designer iteration, lighting review, or persistent placement.
5. Treat procedural generation as composition code; it should accept data and produce named runtime roots that can be inspected and destroyed.

## 2. Asset folders and ownership

| Folder | Contents | Owner |
| --- | --- | --- |
| `Assets/Art/Stargate/` | Stargate FBX, textures, prefab and materials. | Imported external asset plus setup tool. |
| `Assets/Art/Planets/AzureWorld/Imported/` | Azure World building FBX files and generated texture maps. | Planet art kit. |
| `Assets/Resources/Planets/` | `PlanetData` ScriptableObjects loaded by runtime fallback. | Design data. |
| `Assets/Scenes/` | Bootstrap scenes included in the build. | Technical/design integration. |
| `Assets/Scripts/Flight/` | Runtime flight and surface systems. | Gameplay/technical. |
| `Assets/Editor/` | Setup, validation and content binding utilities. | Technical pipeline. |

## 3. Imported model requirements

Before an asset is bound to a planet:

- Export a single object or a deliberately named hierarchy with a known forward/up convention.
- Apply transforms in the DCC tool; avoid relying on a runtime quarter-turn correction as a permanent fix.
- Use real-world scale where possible. Record the intended height and footprint in the asset handoff.
- Place the model origin at the intended placement pivot, ideally at ground contact and the center of the footprint.
- Use readable names: `Azure_ResearchLab`, `Azure_Habitat`, `Azure_CommunicationTower`, `Azure_LandingPad`.
- Provide a textured preview, FBX/glTF source export, base color, normal, metallic and roughness maps when available.
- Keep the first game-ready kit within a measured triangle and material budget; record exceptions in the asset handoff.

The current editor setup searches the Azure World imported folder for model and texture tokens and binds them to the surface prototype. This is a useful bootstrap workflow, but token-based discovery should later be replaced by explicit `PlanetContentDefinition` references or addressable asset labels.

## 4. Materials and textures

Use the project's active render pipeline shader and preserve physically meaningful color spaces:

- Base color: sRGB texture.
- Normal: imported as a normal map.
- Metallic/roughness: linear data textures; pack channels only when the shader contract is documented.
- Emission: reserved for readable beacons, scan markers and navigation accents.

Avoid generating a new material instance per frame or per scan marker. Runtime-created materials should be owned by the generating system and destroyed with that system; authored materials should be shared and immutable at runtime.

## 5. Prefab and collider policy

An imported visual prefab should separate:

- Visual meshes and renderers.
- Gameplay colliders, if the player must physically collide with the asset.
- Interaction anchors such as scan origin, door, landing pad or terminal.
- Optional LOD groups and VFX attachment points.

The current slice removes building colliders at runtime and keeps the island terrain as the principal collision surface. This is deliberate for traversal testing, not a general rule. A production building should supply a simplified collision mesh or compound primitive colliders and should never use high-resolution render meshes for all gameplay collision.

## 6. Generated content ownership

Every runtime-generated root should:

1. Have a stable, descriptive name.
2. Be parented under a mode root (`Flight Runtime` or `Surface Runtime`).
3. Own the meshes/materials it creates and clean them up in `OnDestroy`.
4. Avoid leaking temporary colliders, materials or meshes across scene loads.
5. Expose seed and content inputs once generation becomes procedural and needs reproducibility.

The next terrain pass should move the island mesh, ocean, shoreline and decoration parameters into a `PlanetSurfaceDefinition` asset. This keeps the same runtime composition idea while allowing artists and designers to tune the result without editing code.

## 7. Build and validation gates

Minimum gate for a content change:

1. Unity imports the asset without errors.
2. The setup tool binds the intended model and maps.
3. The relevant scene is in the active Build Profile.
4. Play Mode can load from the flight scene and return from the surface scene.
5. The asset has the intended scale, orientation, materials and collision behavior.
6. No new compile errors or runtime exceptions appear in the Editor log.

For large binary assets, check repository limits before committing. If the project starts carrying many production-sized FBX/textures, use Git LFS or a versioned external asset store instead of silently committing oversized files.
