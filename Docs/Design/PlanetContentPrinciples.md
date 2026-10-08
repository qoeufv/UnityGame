# Planet Content Principles

## Planet contract

Every explorable planet should define, at minimum:

- A clear name and environmental identity.
- A visual palette and lighting mood.
- A reason the player is visiting.
- A safe arrival or landing site.
- At least three readable landmarks or points of interest.
- One local survey, observation or interaction objective.
- A visible route back to the ship and orbit.
- A short description that sets the player's expectations before landing.

The existing `PlanetData` asset is the starting data contract for display name, environment, description, colors, civilization level, landing availability and points of interest. A planet-specific scene or builder may add content, but it should not remove the common player-facing contract without a deliberate design decision.

## World identity matrix

New planets should differ along several dimensions rather than only changing hue:

| Dimension | Examples |
| --- | --- |
| Physical form | Ocean island, crater basin, canyon shelf, ring habitat, ice cavern |
| Environmental force | Tides, storms, volcanic heat, low gravity, dust, signal interference |
| Human presence | Research outpost, abandoned colony, active settlement, orbital debris |
| Player activity | Survey, navigation, rescue, signal tracing, ecology observation |
| Emotional tone | Calm, isolated, mysterious, hostile, awe-inspiring |

At least one physical form, one environmental force and one player activity should change between neighboring destinations. This prevents a collection of visually different but mechanically identical maps.

## Landing site rules

The landing site is the player's first spatial anchor. It must:

- Be recognizable from the surface camera.
- Have enough open space for disembarkation and movement.
- Connect visually to the surrounding terrain.
- Keep the ship visible or provide a clear directional marker.
- Avoid forcing the player through invisible collision volumes.

## Points of interest

Points of interest should have a strong silhouette, a reason to approach and a result after interaction. A scan result can reveal a fact, update a survey record, expose a signal or unlock a future destination. Decorative props should support composition but should not be mistaken for required objectives.

## Expansion rule

Do not add a planet only to increase the destination count. Add it when it introduces a new combination of environment, question and activity that the current worlds cannot provide. A new planet should be prototyped with blockout geometry first, then receive art assets after the movement and objective are accepted.

