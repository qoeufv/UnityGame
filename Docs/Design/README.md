# Frontier Design Documentation

This directory contains the product and game-design baseline for Frontier. It is intended to keep design decisions reviewable as the project grows and to give art, engineering, level design and production work a shared reference.

## Document map

| Document | Purpose |
| --- | --- |
| [Product Vision](ProductVision.md) | Product direction, player promise and design principles |
| [Core Experience](CoreExperience.md) | The exploration loop from orbit to planet surface and back |
| [Scope and Roadmap](ScopeAndRoadmap.md) | Current slice, explicit non-goals and sequencing principles |
| [Planet Content Principles](PlanetContentPrinciples.md) | Rules for adding planets, environments, points of interest and future civilizations |

## How to use these documents

- Product and design changes should be checked against the vision and core loop before implementation.
- A new feature should state which part of the loop it improves and whether it belongs to the current scope.
- A new planet should follow the content principles and document any deliberate exception.
- Weekly reports record delivery and validation; these documents record the intended product baseline.

## Current baseline

The active product slice is a desktop third-person space-flight prototype with a first-person landing and exploration slice on Azure World. The current implementation uses runtime-generated environment and surface blockout geometry so movement, camera, landing and scanning can be evaluated before final art production.

