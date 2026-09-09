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
