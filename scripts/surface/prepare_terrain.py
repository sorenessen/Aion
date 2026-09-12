#!/usr/bin/env python3

"""Prepare terrain-derived presentation inputs for Est.

The source DEM may use its native coordinate system and resolution.
This tool normalizes elevation onto Est's existing regional metric
working grid, then derives slope from that aligned elevation field.

The generated elevation and slope rasters are presentation inputs.
They are not authoritative Est simulation semantics.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
import rasterio
from rasterio.enums import Resampling
from rasterio.warp import reproject


ROOT = Path(__file__).resolve().parents[2]

DEFAULT_REFERENCE = (
    ROOT
    / "data"
    / "generated"
    / "nlcd"
    / "surface-categories.tif"
)

DEFAULT_SOURCE_DIRECTORY = (
    ROOT
    / "data"
    / "source"
    / "elevation"
    / "3dep"
)

DEFAULT_OUTPUT_DIRECTORY = (
    ROOT
    / "data"
    / "generated"
    / "terrain"
)

ELEVATION_FILENAME = "aligned-elevation.tif"
SLOPE_FILENAME = "slope-degrees.tif"
MANIFEST_FILENAME = "terrain-manifest.json"

OUTPUT_NODATA = -999999.0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Align source DEMs to the Est regional metric grid "
            "and derive slope."
        )
    )

    parser.add_argument(
        "--reference",
        type=Path,
        default=DEFAULT_REFERENCE,
        help=(
            "Raster whose CRS, transform, dimensions, and bounds "
            "define the Est working grid."
        ),
    )

    parser.add_argument(
        "--source-directory",
        type=Path,
        default=DEFAULT_SOURCE_DIRECTORY,
        help="Directory containing source DEM GeoTIFFs.",
    )

    parser.add_argument(
        "--output-directory",
        type=Path,
        default=DEFAULT_OUTPUT_DIRECTORY,
        help="Directory for generated terrain presentation inputs.",
    )

    return parser.parse_args()


def source_paths(directory: Path) -> list[Path]:
    paths = sorted(directory.glob("USGS_13_*.tif"))

    if not paths:
        raise RuntimeError(
            f"No 3DEP GeoTIFFs found in {directory}"
        )

    return paths


def reference_geometry(
    path: Path,
) -> tuple[
    rasterio.crs.CRS,
    rasterio.Affine,
    int,
    int,
    rasterio.coords.BoundingBox,
    tuple[float, float],
]:
    with rasterio.open(path) as reference:
        if reference.crs is None:
            raise RuntimeError(
                f"Reference raster has no CRS: {path}"
            )

        if not reference.crs.is_projected:
            raise RuntimeError(
                "Terrain slope requires a projected metric "
                "working grid."
            )

        if reference.crs.linear_units != "metre":
            raise RuntimeError(
                "Terrain slope currently requires reference "
                "grid units in metres."
            )

        x_resolution = abs(reference.res[0])
        y_resolution = abs(reference.res[1])

        if x_resolution <= 0 or y_resolution <= 0:
            raise RuntimeError(
                "Reference raster has invalid resolution."
            )

        return (
            reference.crs,
            reference.transform,
            reference.width,
            reference.height,
            reference.bounds,
            (x_resolution, y_resolution),
        )


def align_elevation(
    sources: list[Path],
    destination_crs: rasterio.crs.CRS,
    destination_transform: rasterio.Affine,
    width: int,
    height: int,
) -> tuple[np.ndarray, np.ndarray]:
    elevation = np.full(
        (height, width),
        np.nan,
        dtype=np.float32,
    )

    coverage = np.zeros(
        (height, width),
        dtype=bool,
    )

    for path in sources:
        print(f"Reprojecting {path.name}")

        with rasterio.open(path) as source:
            if source.count != 1:
                raise RuntimeError(
                    f"{path}: expected one elevation band, "
                    f"got {source.count}"
                )

            if source.crs is None:
                raise RuntimeError(
                    f"{path}: source raster has no CRS"
                )

            temporary = np.full(
                (height, width),
                OUTPUT_NODATA,
                dtype=np.float32,
            )

            reproject(
                source=rasterio.band(source, 1),
                destination=temporary,
                src_transform=source.transform,
                src_crs=source.crs,
                src_nodata=source.nodata,
                dst_transform=destination_transform,
                dst_crs=destination_crs,
                dst_nodata=OUTPUT_NODATA,
                resampling=Resampling.bilinear,
            )

            valid = (
                np.isfinite(temporary)
                & (temporary != OUTPUT_NODATA)
            )

            write_mask = valid & ~coverage

            elevation[write_mask] = temporary[write_mask]
            coverage[write_mask] = True

    return elevation, coverage


def derive_slope(
    elevation: np.ndarray,
    x_resolution_metres: float,
    y_resolution_metres: float,
) -> np.ndarray:
    if not np.all(np.isfinite(elevation)):
        raise RuntimeError(
            "Cannot derive slope from an elevation raster "
            "containing missing values."
        )

    dz_dy, dz_dx = np.gradient(
        elevation.astype(np.float64),
        y_resolution_metres,
        x_resolution_metres,
    )

    rise_run = np.hypot(dz_dx, dz_dy)

    slope = np.degrees(
        np.arctan(rise_run)
    ).astype(np.float32)

    return slope


def write_float_raster(
    path: Path,
    values: np.ndarray,
    crs: rasterio.crs.CRS,
    transform: rasterio.Affine,
) -> None:
    profile = {
        "driver": "GTiff",
        "width": values.shape[1],
        "height": values.shape[0],
        "count": 1,
        "dtype": "float32",
        "crs": crs,
        "transform": transform,
        "nodata": OUTPUT_NODATA,
        "compress": "LZW",
        "tiled": True,
        "blockxsize": 512,
        "blockysize": 512,
    }

    with rasterio.open(
        path,
        "w",
        **profile,
    ) as destination:
        destination.write(
            values.astype(np.float32),
            1,
        )


def main() -> None:
    args = parse_args()

    reference = args.reference.resolve()
    source_directory = args.source_directory.resolve()
    output_directory = args.output_directory.resolve()

    sources = source_paths(source_directory)

    (
        target_crs,
        target_transform,
        width,
        height,
        bounds,
        resolution,
    ) = reference_geometry(reference)

    print("=== EST TERRAIN PREPARATION ===")
    print("reference:", reference)
    print("source directory:", source_directory)
    print("source files:", len(sources))
    print("target CRS:", target_crs)
    print("target dimensions:", f"{width} x {height}")
    print("target resolution:", resolution)
    print("target bounds:", bounds)

    elevation, coverage = align_elevation(
        sources=sources,
        destination_crs=target_crs,
        destination_transform=target_transform,
        width=width,
        height=height,
    )

    covered_pixels = int(np.count_nonzero(coverage))
    total_pixels = int(coverage.size)
    missing_pixels = total_pixels - covered_pixels

    print("\n=== COVERAGE ===")
    print("covered pixels:", covered_pixels)
    print("total pixels:", total_pixels)
    print("missing pixels:", missing_pixels)
    print(
        "coverage percent:",
        round(
            covered_pixels / total_pixels * 100.0,
            6,
        ),
    )

    if missing_pixels:
        raise RuntimeError(
            "Source DEM coverage does not completely cover "
            f"the Est working grid: {missing_pixels} pixels missing."
        )

    slope = derive_slope(
        elevation=elevation,
        x_resolution_metres=resolution[0],
        y_resolution_metres=resolution[1],
    )

    output_directory.mkdir(
        parents=True,
        exist_ok=True,
    )

    elevation_path = output_directory / ELEVATION_FILENAME
    slope_path = output_directory / SLOPE_FILENAME

    write_float_raster(
        elevation_path,
        elevation,
        target_crs,
        target_transform,
    )

    write_float_raster(
        slope_path,
        slope,
        target_crs,
        target_transform,
    )

    manifest = {
        "purpose": "Est terrain presentation inputs",
        "referenceGrid": str(reference.relative_to(ROOT)),
        "sourceType": "USGS 3DEP 1/3 arc-second DEM",
        "sources": [
            str(path.relative_to(ROOT))
            for path in sources
        ],
        "grid": {
            "crs": target_crs.to_wkt(),
            "width": width,
            "height": height,
            "resolutionMetres": {
                "x": resolution[0],
                "y": resolution[1],
            },
            "bounds": {
                "left": bounds.left,
                "bottom": bounds.bottom,
                "right": bounds.right,
                "top": bounds.top,
            },
        },
        "outputs": {
            "elevation": str(
                elevation_path.relative_to(ROOT)
            ),
            "slopeDegrees": str(
                slope_path.relative_to(ROOT)
            ),
        },
        "processing": {
            "elevationResampling": "bilinear",
            "slopeMethod": (
                "gradient magnitude on projected metric grid, "
                "reported as degrees"
            ),
        },
        "role": (
            "Presentation input only; not authoritative "
            "simulation semantics."
        ),
    }

    manifest_path = output_directory / MANIFEST_FILENAME

    manifest_path.write_text(
        json.dumps(manifest, indent=2) + "\n"
    )

    print("\n=== OUTPUT ===")
    print("elevation:", elevation_path)
    print("slope:", slope_path)
    print("manifest:", manifest_path)

    print("\n=== ELEVATION STATS ===")
    print("min metres:", float(np.min(elevation)))
    print("max metres:", float(np.max(elevation)))
    print("mean metres:", float(np.mean(elevation)))

    print("\n=== SLOPE STATS ===")
    print("min degrees:", float(np.min(slope)))
    print("max degrees:", float(np.max(slope)))
    print("mean degrees:", float(np.mean(slope)))
    print(
        "median degrees:",
        float(np.median(slope)),
    )


if __name__ == "__main__":
    main()
