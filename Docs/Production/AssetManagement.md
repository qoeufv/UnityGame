# Asset, License, and Large-File Management

## Asset record

Every external or generated asset should have a small record containing:

| Field | Required information |
| --- | --- |
| Asset ID/name | Stable name and in-game use |
| Source | Creator, marketplace, tool, or generation service |
| License | License name, version, and permitted use |
| Attribution | Required credit text and URL |
| Original files | Source model, texture, prompt, or archive location |
| Import date | Date and responsible person |
| Modifications | Retopology, texture edits, scale/orientation changes |
| Restrictions | Redistribution, training, commercial, or platform limits |

Keep the record next to the feature brief or in a future `Docs/Production/Assets/` catalog. Do not accept an asset into a release when its license or provenance is unknown.

## Unity import rules

- Preserve source files outside Unity when the provider requires it; keep a reproducible reference in the asset record.
- Commit `.meta` files with imported assets so GUID references remain stable.
- Record scale, axis, material/shader, texture map, collider, and LOD decisions for models used in gameplay.
- Avoid placing temporary generated variants in production folders. Use an explicit `Temp`, `Review`, or external staging location.
- Remove unused duplicate imports before release; duplicate assets increase build size and make ownership unclear.

## Large files

Do not commit large binaries casually. Before adding a model, texture, video, archive, or generated package:

1. Check the repository hosting limit and the project’s `.gitignore`.
2. Prefer compressed, platform-appropriate formats and sensible texture resolution.
3. If the file is required in source control, use Git LFS or the project’s approved artifact storage; document that decision.
4. Never rewrite shared history to remove a large file without the owner’s explicit decision.
5. Keep standalone build archives in GitHub Releases, not in normal source commits.

## Generated AI assets

Record the generation service, input reference ownership, prompt or generation settings, output date, and any commercial-use terms. Review generated assets for accidental resemblance to third-party protected work, missing textures, non-manifold geometry, unsafe colliders, and inappropriate scale before integration.

## Review checklist

Before merge, the reviewer confirms that the asset is legally usable, technically importable, visually approved, referenced by the intended scene or prefab, and included in the correct build. Unused assets and uncertain-license assets stay out of the release candidate.
