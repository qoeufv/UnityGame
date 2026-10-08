# Asset Register

This register records the origin, license, ownership and production status of game assets. Update it when an asset enters the repository or changes its legal or technical status.

## Status values

- **Reference:** not shipped; visual or design reference only.
- **Prototype:** usable for internal development, replacement expected.
- **Approved:** reviewed for quality, performance and rights.
- **Deprecated:** should not be used in new content.

## Current register

| Asset ID | Asset | Type | Source | License/rights | Status | Location | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ENV-AZW-001 | Research laboratory | 3D building | Meshy AI generation | Creator account rights must be archived | Prototype | `Assets/Art/Planets/AzureWorld/Imported/` | Used on Azure World |
| ENV-AZW-002 | Residential habitat | 3D building | Meshy AI generation | Creator account rights must be archived | Prototype | `Assets/Art/Planets/AzureWorld/Imported/` | Fragmentation requires review |
| ENV-AZW-003 | Communication tower | 3D structure | Meshy AI generation | Creator account rights must be archived | Prototype | `Assets/Art/Planets/AzureWorld/Imported/` | Azure World landmark |
| ENV-AZW-004 | Landing platform | 3D structure | Meshy AI generation | Creator account rights must be archived | Prototype | `Assets/Art/Planets/AzureWorld/Imported/` | Interaction target |
| ENV-SG-001 | Stargate | 3D structure | User-provided generated asset | Confirm generator terms and archive source | Prototype | Project art folders | Used in flight prototype |

## Required fields for new assets

- Stable asset ID.
- Human-readable name and asset type.
- Author, marketplace, generator or internal owner.
- License, receipt or generator terms reference.
- Repository path and scenes/prefabs using it.
- Production status and replacement risk.
- Generation prompt and reference images when applicable.

