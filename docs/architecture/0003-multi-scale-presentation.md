# Multi-Scale Presentation

## Status

Accepted

## Context

Est must eventually present worlds across very different viewing scales, from
planetary and regional views to cities, buildings, and potentially ground-level
experiences.

The First Globe evaluation initially explored whether one Est-owned raster
surface could remain useful throughout that descent. Categorical land-cover
rendering exposed source-cell and classification boundaries. Continuous
Sentinel-2 imagery removed those categorical visual artifacts and established
a clean separation between semantic truth and visual appearance.

The visual pipeline was then decoupled from the semantic grid so approximately
10-meter Sentinel imagery could retain its source detail rather than being
downsampled onto the approximately 30-meter semantic grid. That architecture
worked, and TMS detail was successfully extended according to visual-source
resolution.

Close-range evaluation nevertheless showed that 10-meter imagery lacks the
information needed to represent recognizable urban structures at building and
street scales. More raster zoom levels cannot recover geometry and detail that
the source does not contain.

Forcing one representation to serve every scale would therefore either produce
poor close-range results or encourage presentation workarounds that manufacture
detail without improving Est's authoritative world model.

## Decision

Est presentation may use different representations of the same authoritative
world feature at different spatial or camera scales.

Planetary and regional presentation may use terrain, imagery, materials, and
other broad surface representations.

Local presentation may progressively introduce geometry such as buildings,
roads, bridges, vegetation, water features, and other spatial structures when
those representations are justified by available data and viewing scale.

Street or immediate presentation may use still richer geometry and materials
when product requirements justify that detail.

These presentation representations do not own or redefine simulation truth.

Simulation and authoritative world state determine what exists and what state
it is in. Presentation determines how that state is represented for a
particular view.

Renderer adapters may contain bounded knowledge required to display prepared
assets, but source-specific semantic interpretation and authoritative feature
meaning should remain outside the renderer when practical.

No universal LOD system, planetary geometry store, streaming architecture, or
complete local-scene model is introduced by this decision. Those mechanisms
will be selected only when focused experiments establish their actual
requirements.

## Rationale

A planet-scale application has fundamentally different presentation costs and
information requirements at different viewing distances.

Continuous regional imagery is useful where individual buildings and local
features do not need to be geometrically legible. At close range, recognizable
structures require information that coarse imagery cannot supply.

Allowing representation to vary by scale preserves the successful
simulation/presentation separation while avoiding the false requirement that
one raster, mesh, material system, or data product solve every visual scale.

This also preserves future flexibility. Real Earth data may drive local
geometry where it exists, while simulated or procedural worlds may eventually
produce equivalent renderer-neutral features from Est state. The presentation
system can consume those representations without making a particular Earth
dataset part of the simulation architecture.

The design follows Est's architecture principles:

> Future possibility is a design constraint, not a requirement to build the
> future now.

and:

> Treat architectural principles as strong defaults, not purity requirements.

The initial implementation remained deliberately small: prove or disprove the
local-geometry hypothesis on one recognizable urban area before generalizing
the architecture.

That focused experiment has now succeeded. OpenStreetMap building footprints
around the Washington State Capitol were normalized through an Est-owned
preparation step and rendered by the Cesium adapter as terrain-relative
extruded geometry over baseline imagery and World Terrain. The experiment
produced 871 individually addressable building features without exposing OSM
building semantics directly to Cesium.

The result materially improved structural legibility during descent. At
regional and campus scales, mapped buildings align with the underlying world
and remain persistent spatial objects as the camera approaches them instead of
degrading into enlarged raster pixels.

The initial geometry is intentionally visually simple. Flat roofs, approximate
heights, uniform materials, and missing architectural detail are not failures
of this experiment. The success criterion is that local features can become
persistent, independently addressable geometry capable of receiving richer
presentation and, where appropriate in future architecture, state derived from
the authoritative Est world.

## Consequences

Positive:

- Semantic and simulation truth remain independent of rendering technique.
- Regional imagery can remain useful without being forced into street-level
  responsibilities.
- Local geometry can add real structural information instead of manufacturing
  detail through raster processing.
- Presentation can become richer during camera descent without requiring the
  authoritative world to change identity.
- Earth-specific source data can be normalized before reaching renderer
  adapters.
- Future simulated or procedural worlds can potentially provide equivalent
  presentation features through the same ownership boundary.
- Expensive local detail does not need to dictate the representation used for
  an entire planet.

Tradeoffs:

- Est will eventually need rules for selecting and transitioning among
  representations.
- Multiple presentation products may exist for the same geographic feature.
- Local geometry introduces data preparation, rendering, performance, and
  possibly streaming requirements that regional raster presentation does not.
- Visual continuity between representations becomes an explicit quality
  concern.
- The first local-scene artifact is deliberately narrow and evaluation-focused;
  a durable general local-scene contract should be defined only as additional
  feature classes and simulation requirements establish what information it
  actually needs.

## Deferred Questions

- Which camera distances or screen-space measures should trigger
  representation changes.
- Whether transitions use discrete LOD, blending, streaming, hierarchical
  spatial partitions, or another mechanism.
- What the durable renderer-neutral local-scene contract should contain.
- How buildings, roads, vegetation, water, and other feature classes should be
  represented in authoritative Est state.
- Which real-world datasets should contribute to local presentation.
- How procedural and simulated worlds should generate equivalent local
  representations.
- How much geometry should be resident, generated, cached, or streamed at each
  scale.
- How terrain, imagery, geometry, and materials should visually blend during
  transitions.
- Whether Cesium remains the long-term renderer once Est's local-scene
  requirements are better understood.

These questions are deliberately deferred until focused local-geometry
experiments provide evidence for their requirements.
