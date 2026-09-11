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
