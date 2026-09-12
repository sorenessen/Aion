# Regional Surface Evaluation

## Purpose

Validate a real categorical land-cover pipeline for Est without coupling
authoritative simulation state to a particular source dataset or renderer.
This is a rendering and data-ownership evaluation, not a spatial simulation
model or final visual design.

## Source

- Product: USGS Annual NLCD Land Cover 2025
- Collection: 1.2
- Version: 1.2, June 2026
- MRLC order: `5a3e2c72-778a-4f3b-9c57-be4e7b1fef82`
- Requested bounds: longitude -123.3 to -121.3, latitude 46.6 to 47.4
- Source raster: 5705 x 4230, one band, uint8, 30 meters
- CRS: Albers Equal Area, WGS84
- NoData: 250
- Expected source classes: 11, 12, 21, 22, 23, 24, 31, 41, 42, 43, 52, 71, 81, 82, 90, 95

The source archive SHA256 is:

`bf7f3ba86b462a8a3b89116286be1c9f1d60ecbca1eb1119cddff3d27f71b727`

Raw source data and full generated rasters are intentionally excluded
from Git. The local source manifest and generated manifest preserve
additional metadata and provenance.

## Conversion

The canonical category definition is
`src/Est.Web/src/surface/surface-categories.json`.

Est categories are Unknown, OpenWater, Developed, Barren, Forest,
Shrubland, Grassland, Agriculture, Wetland, and SnowIce. Source-specific
NLCD class mappings are kept separate from renderer-specific treatment.

The converter uses NumPy and Rasterio with pinned dependencies in
`scripts/requirements-geospatial.txt`. It preserves categorical values
through nearest-neighbor resampling and does not infer land cover from
satellite RGB or elevation.

From the repository root, after creating and activating a Python virtual
environment and installing the pinned requirements:

```sh
python scripts/convert-nlcd.py \
  data/source/nlcd/extracted/Annual_NLCD_LndCov_2025_CU_C1V2_5a3e2c72-778a-4f3b-9c57-be4e7b1fef82.tiff
```

The default output directory is `data/generated/nlcd/`. The converter
produces `surface-categories.tif`, `surface-preview-native.png`,
`surface-preview-geographic.png`, and `surface-manifest.json`.

The geographic preview is 1800 x 1053, EPSG:4326, with bounds:

- West: -123.61633988971845
- South: 46.23644679019992
- East: -121.00788955795646
- North: 47.76234468508979

The browser evaluation uses a small copy of the geographic PNG and a
reduced manifest under `src/Est.Web/public/evaluation/nlcd-2025/`.
The full generated manifest remains local.

## Browser validation

The Cesium evaluation is `src/Est.Web/cesium.html`. Its Est Surface Study
mode loads the geographic preview as a single-tile imagery layer over
Cesium World Terrain. The other three modes remain available for
comparison: Baseline, Terrain Study, and USGS Land Cover.

On September 8, 2026, browser inspection confirmed coherent geographic
alignment around Puget Sound, Tacoma/Olympia, and Mount Rainier. Close
inspection showed the surface following terrain relief, with coherent
coastline and snow/ice placement. The web production build passed.

The current preview is intentionally diagnostic. At close range it becomes
blocky, and its rectangular coverage boundary is visible. The dark-blue
surface outside coverage is not an Est land-cover classification. These
limitations should not be confused with source-data misalignment.

## Next evaluation

- Preserve the working baseline and source-to-category conversion.
- Evaluate full-resolution regional delivery and level of detail.
- Improve coverage fallback and regional transitions.
- Develop natural visual treatment from Est-owned categories.
- Evaluate water, forests, developed areas, mountains, snow, close-range
  quality, and performance.
- Keep renderer selection open and avoid premature geospatial infrastructure.

No regional spatial state has been added to Est.Simulation. The current
planetary environment remains aggregate authoritative state.

## Full-resolution TMS evaluation checkpoint

On September 9, 2026, the regional surface study gained a fifth Cesium
mode, Est Surface TMS. It preserves the original single-image study as a
comparison control and serves a static geographic TMS pyramid generated
from Est-owned categorical raster data.

The pyramid uses EPSG:4326, 256-pixel tiles, south-origin TMS coordinates,
and levels 7 through 11. It contains 758 PNG tiles. Nearest-neighbor
reprojection preserves categorical values, and Unknown remains transparent.
The generator, validator, and geometry tests are under scripts/surface/
and tests/python/. The published evaluation tiles are under
src/Est.Web/public/evaluation/nlcd-2025/surface-tms/.

Validation completed before the checkpoint: 10 Python geometry tests
passed, the published pyramid validator checked all 758 tiles and all
10 canonical colors, and the web production build passed. The existing
large-chunk build warning remains non-blocking.

Browser inspection confirmed that the new mode loads and that close-range
lake and shoreline detail is substantially less pixelated than the original
preview. This is a promising visual result, not final acceptance. The
dataset's approximately 30-meter source resolution remains the detail
ceiling.

Subsequent browser evaluation confirmed:
- Renderer-only inspection daylight works without changing authoritative
  Est simulation state.
- Surface modes can be switched without moving the camera.
- No obvious tile-to-tile seams were observed in populated regional
  coverage.
- Unknown pixels remain transparent as designed.
- The finite regional coverage boundary is clearly visible when the
  surrounding globe has no Est-owned surface fallback. This is an
  evaluation-data limitation, not a TMS alignment failure. Do not hide it
  by inventing classifications or stretching regional data beyond its
  source coverage. A future surface stack should allow detailed regional
  semantics to fall back to coarser Est-owned planetary semantics.

Remaining evaluation work:
- Evaluate natural category materials and close-range visual quality,
  performance, and scale transitions.
- Harden and test publication rollback behavior before treating the
  generator as a general-purpose production pipeline.

No tile server, new backend service, or regional simulation state was
introduced. The original Baseline and other comparison modes remain
available.

## Natural material presentation checkpoint

On September 10, 2026, the regional TMS pipeline gained an Est-owned
presentation layer between semantic surface categories and generated
imagery. Semantic identity remains in `surface-categories.json`;
`surface-presentation.json` defines presentation-only material parameters,
and `scripts/surface/presentation.py` interprets them. Cesium continues to
consume generated imagery and does not own Est surface semantics.

Presentation version 2, `natural-material-study`, adds deterministic
geographic tonal variation to the base color of each known surface category.
The variation is calculated from longitude and latitude rather than tile-local
random state, so a geographic coordinate receives the same treatment
independently of the tile or render call that contains it. Unknown remains
fully transparent. Category IDs and boundaries are not blended or modified.

The published evaluation pyramid remains 758 tiles across levels 7 through
11. Validation now permits the many RGB values intentionally produced by
material variation while enforcing binary alpha, zero RGB for transparent
pixels, and agreement between the published manifest and the current Est
presentation definition.

Browser A/B inspection against the earlier flat Est Surface Study confirmed
that the presentation seam works without Cesium-specific material logic.
The difference is visible, especially across broad forest, barren, developed,
and snow/ice regions, but subtle tonal variation alone does not remove the
classified-raster appearance. Terrain relief contributes much of the useful
small-scale visual structure, while the approximately 30-meter source
classification remains visible at close range.

This result is useful even though it is not a final art direction. It
establishes that Est surface categories can describe presentation materials
rather than only fixed colors while preserving semantic ownership and
renderer independence. Further work should build category-specific material
structure on this seam rather than spending substantial effort tuning a
single generic noise treatment. Any added structure must remain
presentation-only and must not imply unsupported simulation facts.

Validation for this checkpoint includes 20 passing Python tests: 10
presentation/material tests and the existing 10 geographic TMS geometry
tests. The presentation tests cover deterministic rendering, geographic
coordinate stability across render calls, Unknown transparency, alpha
preservation, category-boundary preservation, material variation, exact
zero-variation rendering, semantic category coverage, and input-shape
validation.

## Category-specific material presentation checkpoint

On September 10, 2026, presentation version 3,
`category-material-study`, extended each surface material from one generic
variation amplitude to independent broad-, medium-, and fine-scale geographic
variation. The underlying noise components remain deterministic in geographic
space, while each category controls how strongly it responds at each scale.
Semantic category IDs, category boundaries, Unknown transparency, TMS geometry,
and Cesium ownership remain unchanged.

The category profiles produce measurably different spatial responses. A focused
frequency-response test confirmed that fine-scale treatment creates
substantially more local variation than broad-scale treatment over the same
geographic region. The full Python suite now contains 21 passing tests,
including this material-profile behavior.

The generated and published version 3 pyramid remains 758 tiles across levels
7-11 with the same semantic coverage and 18,081,876 opaque pixels. Browser A/B
inspection confirmed that category-specific spatial character is visible and
that the TMS remains sharper and more useful than the blurred single-image
study. No obvious tile-boundary artifact was observed.

The visual experiment also identified the dominant remaining limitation.
Category-specific tonal structure does not substantially change the fact that
the regional surface reads as a classified raster draped over terrain. At close
range, discrete source-cell and category-boundary geometry dominates perception
more than the internal material variation. Increasing generic procedural
variation would decorate those classified regions rather than address that
limitation and could make the result look artificially noisy.

Preserve version 3 as an architectural capability, but do not spend the next
iteration tuning broad, medium, and fine amplitudes. The next presentation
experiment should investigate how discrete semantic classifications can drive a
more continuous-looking physical surface without altering authoritative
category identity, inventing unsupported classifications, or moving Est
surface semantics into Cesium. Blurring categorical truth is not an acceptable
substitute for a presentation model.

## Source-class visual structure checkpoint

On September 11, 2026, the regional surface evaluation tested whether the
classified-raster appearance was primarily caused by Est collapsing the
original NLCD classes into its smaller semantic vocabulary.

Direct geometry comparison confirmed that the original Annual NLCD raster and
`surface-categories.tif` have identical dimensions, CRS, 30-meter resolution,
bounds, and affine transform. Est's semantic conversion therefore does not
introduce or coarsen the source-cell geometry. It changes categorical identity
only.

An adjacency analysis measured 4,807,796 class transitions between known
neighboring NLCD cells. Est's semantic categories retain 2,868,083 of those
transitions and intentionally collapse 1,939,713, or 40.35 percent. The largest
collapsed groups are distinctions within developed intensity and forest type,
with smaller losses within wetlands and agriculture.

A disposable geographic diagnostic rendered all 16 known NLCD source classes
with distinct presentation colors. Restoring those source distinctions visibly
adds meaningful regional structure, especially within developed areas, forest,
agriculture, and wetlands. It does not, however, resolve the fundamental visual
limitation. The result still reads as categorical land-cover imagery, and the
native 30-meter classified-cell geometry remains visually dominant.

This changes the next presentation direction. Do not spend the next iteration
smoothing Est semantic boundaries or further tuning procedural noise merely to
hide categorical geometry. NLCD remains valuable authoritative input for Est
surface semantics and may also contribute non-authoritative information to
presentation, but direct category colorization should not be treated as the
intended final visual-surface strategy.

Preserve the current semantic pipeline and the version 3 presentation/TMS work
as proven capabilities and evaluation controls. The next architectural
investigation should define a visual-surface input and composition seam that is
explicitly separate from authoritative simulation semantics. Presentation may
legitimately use more source information than the simulation vocabulary and may
combine multiple visual inputs, provided those inputs never become simulation
truth and Cesium remains a presentation consumer rather than the owner of Est
surface semantics.

The guiding distinction is now: simulation semantics answer what is at a
location; presentation determines how that location should look. Those concerns
are related but are not required to use the same data product.

## Continuous visual surface checkpoint

On September 11, 2026, the regional surface evaluation added a continuous
visual-surface path using Sentinel-2 RGB imagery as a presentation-only input.
The semantic category raster continues to determine Est surface identity and
known-versus-Unknown coverage. It does not colorize the imagery, blend category
materials into it, or otherwise define the visual appearance of known pixels.

The experiment deliberately excluded slope response, category tinting, and
procedural material variation. Inside the known semantic footprint, the
prepared RGB imagery is used directly as the visual basis; Unknown remains
transparent. This isolates the architectural question of whether a continuous
visual source can replace categorical land-cover geometry as the dominant
rendered surface while preserving Est semantic ownership.

The published continuous-surface pyramid remains EPSG:4326 geographic TMS with
256-pixel tiles, south-origin Y coordinates, levels 7 through 11, and 758
tiles. Its metadata records the visual RGB input separately from the semantic
source. Cesium consumes the resulting RGBA tiles and does not interpret NLCD
classes or Est semantic categories.

Browser A/B evaluation validated the architecture hypothesis. The continuous
surface no longer reads primarily as discrete NLCD polygons. Snowfields, rock,
forest, ridges, drainage, roads, shoreline, developed areas, and industrial
structure remain visually continuous across semantic-category boundaries.
Semantic classification is therefore better treated as a description of the
world than as the direct paint used to render the world.

The experiment also exposed a separate resolution problem. Sentinel-2 RGB is
approximately 10-meter source imagery, but the current imagery-preparation
pipeline resamples it onto the approximately 30-meter semantic working grid
before TMS generation. Close-range buildings consequently lose useful source
detail and become soft or paint-like even though the continuous visual model
itself is working.

Do not address that limitation by increasing TMS zoom over the existing
30-meter aligned RGB product. That would only magnify already-discarded detail.
The next visual-surface iteration should decouple semantic and presentation
resolution: keep authoritative semantics on their appropriate grid, preserve
imagery near its useful source resolution, and sample both independently during
visual composition. Semantic categories should use categorical nearest-neighbor
sampling; continuous imagery should use an appropriate continuous resampler.
The visual TMS level ceiling should follow actual visual-source resolution
rather than the semantic raster resolution.

This checkpoint establishes the intended ownership direction without adding a
tile server, backend service, regional simulation state, or renderer-owned
surface semantics. The categorical and material-based modes remain valuable
evaluation controls and fallback capabilities.

## Visual-resolution decoupling checkpoint

On September 11-12, 2026, the continuous visual-surface pipeline was
decoupled from the semantic raster's approximately 30-meter working
resolution. Sentinel-2 RGB is now composed on a separate 10-meter visual grid
covering the same projected regional bounds, while Est surface semantics
remain on their existing 30-meter grid.

The 10-meter visual grid is 17,115 by 12,690 pixels. Semantic coverage is
projected onto it with nearest-neighbor sampling so categorical identity is
not blurred or reinterpreted. Continuous RGB imagery is sampled bilinearly.
This establishes that semantic/reference resolution and presentation
resolution are independent concerns and need not share one raster grid.

The imagery-preparation pipeline also gained a bounded repair for small,
fully enclosed presentation-coverage gaps. The evaluation region initially
contained 349 missing required visual pixels across 131 enclosed components;
the largest component contained 24 pixels. The repair accepts components no
larger than 32 pixels, limits total repair to 512 pixels, rejects components
that touch the visual-grid edge, requires a covered external boundary, and
fills inward deterministically from covered neighboring RGB values. All 349
required pixels were repaired. Uncovered pixels outside the required semantic
surface remain uncovered.

TMS generation was independently decoupled from semantic resolution. Semantic
source bounds continue to define the publication extent, but when a visual RGB
source is present its effective geographic resolution determines the maximum
detail level. The approximately 30-meter semantic source resolves through
level 11; the 10-meter visual source resolves through level 13.

Visual coverage is validated once as a pyramid preflight invariant rather than
by repeatedly reading and reprojecting the full visual dataset mask for every
tile. This matters at the 10-meter working size of more than 217 million
pixels and keeps per-tile rendering focused on RGB reprojection.

The resulting local experimental pyramid contains 11,188 PNG tiles across
levels 7 through 13 and occupies approximately 435 MB. Its tile counts and
manifest were independently verified. The 10-meter aligned RGB artifact is
approximately 517 MB. These are evaluation artifacts, not a decision to store
large high-resolution pyramids in normal Git history. The existing smaller
level-11 continuous-surface artifact remains the committed evaluation
checkpoint.

The full Python suite contains 41 passing tests at this checkpoint, and
`git diff --check` passes.

Architecturally, this experiment succeeded. Est can preserve semantic truth on
one grid, presentation information on another, derive rendering detail from
the appropriate source, validate cross-grid coverage, and continue to keep
Cesium outside semantic interpretation.

## Close-range raster conclusion

Browser A/B evaluation of the 10-meter result established a different limit:
preserving all available Sentinel-2 detail does not make that imagery suitable
as Est's close-range urban representation.

At regional scale the continuous visual surface remains substantially more
natural than direct categorical land-cover rendering. At close urban scale,
however, individual buildings and site features become broad or blurry color
shapes. The Washington State Capitol area provided the clearest comparison:
the baseline can resolve recognizable buildings, roads, parking areas, paths,
trees, roof structure, and surrounding site organization that the 10-meter
Sentinel surface cannot.

This is no longer a semantic-grid or TMS-level problem. It is an information
limit in the visual source at the requested viewing scale.

Do not continue this line of investigation by generating level 14 or 15 tiles
from the same imagery, sharpening or interpolating the source, adding more
generic procedural noise, tinting imagery from semantic categories, tuning
scalar slope darkening, smoothing semantic boundaries, or acquiring additional
Sentinel dates in an attempt to manufacture street-level detail.

The work remains useful. Continuous imagery is a viable regional presentation
layer, and the independent-resolution composition architecture should be
preserved. The failed hypothesis is narrower: one raster surface should not be
expected to provide Est's useful representation at every camera distance.

## Multi-scale presentation pivot

The next presentation investigation will evaluate different representations
at different spatial and camera scales while preserving one authoritative Est
world state.

The provisional model is:

- planetary scale: terrain, atmosphere, and coarse/global surface appearance
- regional scale: terrain, imagery/materials, land-cover-informed appearance,
  and broad vegetation or water treatment
- local/city scale: buildings, roads, bridges, vegetation, water features,
  and other justified geometry
- street/immediate scale: higher-detail geometry, materials, and local
  presentation detail where evidence and product requirements justify them

These are evaluation categories, not committed LOD boundaries or a final
rendering architecture. The renderer may change how a feature is represented
as the camera approaches it, but representation must not redefine simulation
truth.

The guiding ownership rule remains:

> Simulation determines what exists and what state it is in. Presentation
> determines how that state should be represented at the current scale.

The next experiment is intentionally narrow: the Washington State Capitol
campus in Olympia. Keep World Terrain and baseline imagery, acquire real
building footprints for a small surrounding area, convert them through an
Est-owned renderer-neutral preparation boundary, and let the Cesium adapter
render the resulting building geometry.

The first question is not whether Est can build a complete city system. It is
whether a recognizable real building can transition from being primarily part
of regional imagery at altitude to readable geometry during close descent
without an unacceptable visual discontinuity.

Do not begin the spike with roads, vegetation, props, facade generation, or a
general planetary geometry system. Those become justified follow-ups only if
the building experiment proves the central multi-scale hypothesis.

The regional surface work is therefore not abandoned. It has established the
presentation/data boundaries needed for a larger system and remains useful at
the scales where its source information is appropriate. The next evaluation
moves to local geometry because the browser evidence shows that additional
raster refinement is now solving the wrong problem.

## Local-geometry checkpoint

On September 12, 2026, the Washington State Capitol local-geometry experiment
validated the first multi-scale presentation hypothesis.

A small OpenStreetMap extract around the Capitol was converted through an
Est-owned preparation boundary into a renderer-neutral GeoJSON
FeatureCollection. Source-specific OSM building semantics are normalized
before reaching Cesium. The initial artifact contains 871 building features.
Each feature has an Est-defined identifier and normalized height in metres;
the Cesium adapter consumes those prepared properties rather than interpreting
`building`, `building:levels`, or other OSM tags.

The first renderer intentionally does very little. It places the prepared
building footprints over Cesium World Terrain and baseline World Imagery and
extrudes them to their normalized heights. The Washington State Capitol itself
is represented from its real mapped footprint with an 18-metre height derived
from six mapped levels. No dome, facade, roof system, procedural architecture,
road geometry, vegetation, props, or handcrafted Capitol detail was added.

Browser evaluation strongly validated the representation change. From broader
city and campus views, mapped structures become clearly legible spatial
objects aligned with the underlying imagery. During close descent they remain
geometry with position, footprint, and volume rather than becoming enlarged
raster pixels. The experiment therefore changes the close-range problem from
"find enough raster detail to look like a building" to "provide the geometric
and material information needed to represent the building."

Visual blandness is explicitly not a failure criterion for this checkpoint.
Uniform light-colored extrusions, flat roofs, approximate heights, and missing
architectural detail are expected limitations of the deliberately minimal
proof. The important result is that local structures are now independently
addressable presentation objects that can be enriched later without requiring
the regional surface representation to carry street-level information.

This result also preserves the value of the earlier terrain and regional
surface work. Est-owned terrain, procedural/material surface treatment,
satellite or aerial imagery, or combinations of those sources may continue to
provide planetary and regional appearance. Local mapped geometry can resolve
structures, roads, vegetation, and other features as closer viewing scales
justify them. No single surface source is required to solve every scale.

The emerging capability stack is therefore:

1. terrain and elevation establish the land form;
2. regional surface presentation establishes broad appearance;
3. mapped or generated spatial features establish local structure;
4. local geometry provides persistent addressable objects;
5. authoritative Est state can eventually drive relevant object state and
   behavior without transferring simulation ownership to the renderer;
6. presentation can progressively enrich those objects with materials,
   architectural detail, effects, and other scale-appropriate representations.

This checkpoint does not establish final LOD distances, streaming strategy,
planetary geometry storage, a general feature schema, or Cesium as the
permanent local renderer. Those remain evidence-driven follow-up decisions.

The next local-scene work should continue to prioritize capability over
cosmetic polish. Uniform building materials are sufficient while Est proves
which additional spatial structures and state-bearing boundaries are required.

## Longmire cross-environment local-geometry checkpoint

On September 12, 2026, the local-geometry evaluation moved from the Washington
State Capitol urban/civic environment to Longmire in Mount Rainier National
Park.

The purpose was not to improve the appearance of the neutral building
extrusions. It was to test whether the same renderer-neutral local-scene
boundary remained useful in a terrain-rich environment where regional
landscape presentation is visually dominant and mapped structures are sparse.

The Longmire evaluation reused the existing Est local-scene preparation model:
OpenStreetMap building ways are normalized before reaching Cesium, building
height is represented through Est-defined `heightMeters`, and Cesium consumes
the prepared geographic geometry rather than interpreting raw OSM building
semantics.

Browser inspection provided strong positive evidence across several camera
conditions. From broad mountainous views, the structures remain spatially
coherent within the valley rather than behaving like raster detail. During
oblique descent they remain terrain-relative three-dimensional objects against
steep surrounding relief. At close range their footprint and volume remain
legible. Top-down comparison with the underlying imagery also showed strong
geographic agreement between prepared geometry and visible building locations.

The result is significant because Olympia and Longmire exercise the same
presentation boundary in materially different environments. Olympia
demonstrated dense urban/civic structure. Longmire demonstrates sparse local
structure embedded in a terrain-dominant mountain landscape.

This validates the composition model:

`terrain -> regional surface/imagery -> mapped spatial features -> local geometry`

The layers have distinct responsibilities. Terrain describes land form.
Regional surface treatment or imagery provides broad visual appearance.
Prepared local geometry provides persistent, independently addressable
structures. None of those presentation representations becomes authoritative
simulation state merely by being rendered.

The comparison also clarifies the role of imagery. High-quality imagery can be
extremely useful as regional or local appearance and as a visual alignment
reference without being required to carry structural responsibility for
buildings. A building can visually coincide with imagery while remaining a
separate Est-prepared geometric object that can later receive materials,
richer architectural representation, effects, or state derived from the
authoritative world.

The neutral extrusions remain intentionally crude. Approximate heights, simple
roof forms, uniform materials, missing facade detail, and incomplete source
metadata are not failures of this checkpoint. The experiment validates spatial
composition and ownership boundaries, not final presentation quality.

Do not infer from this checkpoint that Est has solved automatic LOD selection,
geometry streaming, planetary local-feature storage, detailed building
generation, material generation, vegetation geometry, road geometry, or
simulation-driven building behavior. Those remain evidence-driven follow-up
work.
