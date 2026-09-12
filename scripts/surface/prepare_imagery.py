#!/usr/bin/env python3

"""Prepare continuous imagery presentation input for Est.

Sentinel-2 visual imagery is normalized onto an Est-owned regional
visual grid derived from the semantic reference CRS and bounds. The visual
grid preserves source-appropriate presentation detail independently of the
semantic raster resolution. Source acquisition and mosaicking details remain
inside this preparation boundary so downstream presentation code can consume
a generic RGB field without knowing about Sentinel, MGRS, STAC, or
source-scene geometry.

The generated imagery is a presentation input. It is not authoritative
Est simulation semantics.
"""

from __future__ import annotations

import argparse
import json
from dataclasses import dataclass
from pathlib import Path

import numpy as np
import rasterio
from rasterio.enums import Resampling
from rasterio.transform import from_origin
from rasterio.warp import reproject
from rasterio.vrt import WarpedVRT


ROOT = Path(__file__).resolve().parents[2]

DEFAULT_REFERENCE = (
    ROOT
    / "data"
    / "generated"
    / "nlcd"
    / "surface-categories.tif"
)

DEFAULT_OUTPUT_DIRECTORY = (
    ROOT
    / "data"
    / "generated"
    / "imagery"
)

IMAGERY_FILENAME = "aligned-visual-rgb.tif"
MANIFEST_FILENAME = "imagery-manifest.json"

MAX_ENCLOSED_GAP_COMPONENT_PIXELS = 32
MAX_ENCLOSED_GAP_TOTAL_PIXELS = 512
VISUAL_RESOLUTION_METRES = 10.0


@dataclass(frozen=True)
class Observation:
    priority: int
    date: str
    tile: str
    item_id: str
    cloud_cover_percent: float
    href: str
    role: str


OBSERVATIONS = (
    Observation(
        priority=10,
        date="2026-07-13",
        tile="MGRS-10TDS",
        item_id="S2B_10TDS_20260713_0_L2A",
        cloud_cover_percent=0.002651,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/DS/2026/7/"
            "S2B_10TDS_20260713_0_L2A/TCI.tif"
        ),
        role="primary",
    ),
    Observation(
        priority=10,
        date="2026-07-13",
        tile="MGRS-10TDT",
        item_id="S2B_10TDT_20260713_0_L2A",
        cloud_cover_percent=0.1306,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/DT/2026/7/"
            "S2B_10TDT_20260713_0_L2A/TCI.tif"
        ),
        role="primary",
    ),
    Observation(
        priority=10,
        date="2026-07-13",
        tile="MGRS-10TES",
        item_id="S2B_10TES_20260713_0_L2A",
        cloud_cover_percent=0.000177,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/ES/2026/7/"
            "S2B_10TES_20260713_0_L2A/TCI.tif"
        ),
        role="primary",
    ),
    Observation(
        priority=10,
        date="2026-07-13",
        tile="MGRS-10TET",
        item_id="S2B_10TET_20260713_0_L2A",
        cloud_cover_percent=0.000899,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/ET/2026/7/"
            "S2B_10TET_20260713_0_L2A/TCI.tif"
        ),
        role="primary",
    ),
    Observation(
        priority=10,
        date="2026-07-13",
        tile="MGRS-10TFS",
        item_id="S2B_10TFS_20260713_0_L2A",
        cloud_cover_percent=0.774683,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/FS/2026/7/"
            "S2B_10TFS_20260713_0_L2A/TCI.tif"
        ),
        role="primary",
    ),
    Observation(
        priority=10,
        date="2026-07-13",
        tile="MGRS-10TFT",
        item_id="S2B_10TFT_20260713_0_L2A",
        cloud_cover_percent=0.0,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/FT/2026/7/"
            "S2B_10TFT_20260713_0_L2A/TCI.tif"
        ),
        role="primary",
    ),
    Observation(
        priority=20,
        date="2026-07-30",
        tile="MGRS-10TFT",
        item_id="S2B_10TFT_20260730_0_L2A",
        cloud_cover_percent=0.979048,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/FT/2026/7/"
            "S2B_10TFT_20260730_0_L2A/TCI.tif"
        ),
        role="fallback",
    ),
    Observation(
        priority=21,
        date="2026-07-30",
        tile="MGRS-10TFS",
        item_id="S2B_10TFS_20260730_0_L2A",
        cloud_cover_percent=0.064675,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/FS/2026/7/"
            "S2B_10TFS_20260730_0_L2A/TCI.tif"
        ),
        role="fallback",
    ),
    Observation(
        priority=22,
        date="2026-07-30",
        tile="MGRS-10TES",
        item_id="S2B_10TES_20260730_0_L2A",
        cloud_cover_percent=2.088069,
        href=(
            "https://sentinel-cogs.s3.us-west-2.amazonaws.com/"
            "sentinel-s2-l2a-cogs/10/T/ES/2026/7/"
            "S2B_10TES_20260730_0_L2A/TCI.tif"
        ),
        role="fallback",
    ),
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Align continuous visual imagery to the Est regional "
            "metric working grid."
        )
    )

    parser.add_argument(
        "--reference",
        type=Path,
        default=DEFAULT_REFERENCE,
        help=(
            "Raster whose CRS, transform, dimensions, and bounds "
            "define the Est working grid and semantic footprint."
        ),
    )

    parser.add_argument(
        "--output-directory",
        type=Path,
        default=DEFAULT_OUTPUT_DIRECTORY,
        help="Directory for generated imagery presentation input.",
    )

    return parser.parse_args()


def read_reference(
    path: Path,
) -> tuple[
    rasterio.crs.CRS,
    rasterio.Affine,
    int,
    int,
    rasterio.coords.BoundingBox,
    tuple[float, float],
    np.ndarray,
]:
    with rasterio.open(path) as reference:
        if reference.crs is None:
            raise RuntimeError(
                f"Reference raster has no CRS: {path}"
            )

        if not reference.crs.is_projected:
            raise RuntimeError(
                "Imagery preparation requires Est's projected "
                "working grid."
            )

        if reference.crs.linear_units != "metre":
            raise RuntimeError(
                "Imagery preparation currently requires reference "
                "grid units in metres."
            )

        semantic = reference.read(1)
        semantic_surface = semantic != 0

        if not np.any(semantic_surface):
            raise RuntimeError(
                "Reference raster contains no semantic surface."
            )

        return (
            reference.crs,
            reference.transform,
            reference.width,
            reference.height,
            reference.bounds,
            (
                abs(reference.res[0]),
                abs(reference.res[1]),
            ),
            semantic_surface,
        )



def create_visual_grid(
    bounds: rasterio.coords.BoundingBox,
    resolution_metres: float = VISUAL_RESOLUTION_METRES,
) -> tuple[rasterio.Affine, int, int, tuple[float, float]]:
    if resolution_metres <= 0:
        raise ValueError("Visual resolution must be greater than zero.")

    span_x = bounds.right - bounds.left
    span_y = bounds.top - bounds.bottom

    width_float = span_x / resolution_metres
    height_float = span_y / resolution_metres

    width = int(round(width_float))
    height = int(round(height_float))

    tolerance = 1e-9

    if (
        abs(width_float - width) > tolerance
        or abs(height_float - height) > tolerance
    ):
        raise RuntimeError(
            "Reference bounds are not evenly divisible by the requested "
            f"{resolution_metres:g} metre visual resolution."
        )

    transform = from_origin(
        bounds.left,
        bounds.top,
        resolution_metres,
        resolution_metres,
    )

    return (
        transform,
        width,
        height,
        (resolution_metres, resolution_metres),
    )


def project_required_surface(
    semantic_surface: np.ndarray,
    source_crs: rasterio.crs.CRS,
    source_transform: rasterio.Affine,
    destination_transform: rasterio.Affine,
    width: int,
    height: int,
) -> np.ndarray:
    destination = np.zeros(
        (height, width),
        dtype=np.uint8,
    )

    reproject(
        source=semantic_surface.astype(np.uint8),
        destination=destination,
        src_transform=source_transform,
        src_crs=source_crs,
        src_nodata=0,
        dst_transform=destination_transform,
        dst_crs=source_crs,
        dst_nodata=0,
        resampling=Resampling.nearest,
    )

    return destination != 0

def read_aligned_observation(
    observation: Observation,
    destination_crs: rasterio.crs.CRS,
    destination_transform: rasterio.Affine,
    width: int,
    height: int,
) -> tuple[np.ndarray, np.ndarray]:
    with rasterio.open(observation.href) as source:
        if source.crs is None:
            raise RuntimeError(
                f"{observation.item_id}: source raster has no CRS"
            )

        if source.count != 3:
            raise RuntimeError(
                f"{observation.item_id}: expected three RGB bands, "
                f"got {source.count}"
            )

        with WarpedVRT(
            source,
            crs=destination_crs,
            transform=destination_transform,
            width=width,
            height=height,
            resampling=Resampling.bilinear,
        ) as vrt:
            values = vrt.read(
                indexes=(1, 2, 3),
                out_dtype="uint8",
            )

            valid = np.ones(
                (height, width),
                dtype=bool,
            )

            for band_index in (1, 2, 3):
                valid &= (
                    vrt.read_masks(band_index) > 0
                )

    return values, valid


def compose_imagery(
    observations: tuple[Observation, ...],
    destination_crs: rasterio.crs.CRS,
    destination_transform: rasterio.Affine,
    width: int,
    height: int,
) -> tuple[
    np.ndarray,
    np.ndarray,
    list[dict[str, object]],
]:
    imagery = np.zeros(
        (3, height, width),
        dtype=np.uint8,
    )

    coverage = np.zeros(
        (height, width),
        dtype=bool,
    )

    contribution_records: list[dict[str, object]] = []

    ordered = sorted(
        observations,
        key=lambda item: (
            item.priority,
            item.tile,
            item.item_id,
        ),
    )

    for observation in ordered:
        print(
            f"Reading {observation.date} "
            f"{observation.tile} "
            f"({observation.role})"
        )

        values, valid = read_aligned_observation(
            observation=observation,
            destination_crs=destination_crs,
            destination_transform=destination_transform,
            width=width,
            height=height,
        )

        write_mask = valid & ~coverage

        valid_pixels = int(
            np.count_nonzero(valid)
        )
        contributed_pixels = int(
            np.count_nonzero(write_mask)
        )

        if contributed_pixels:
            imagery[:, write_mask] = values[:, write_mask]
            coverage[write_mask] = True

        contribution_records.append(
            {
                "priority": observation.priority,
                "date": observation.date,
                "tile": observation.tile,
                "itemId": observation.item_id,
                "cloudCoverPercent": (
                    observation.cloud_cover_percent
                ),
                "role": observation.role,
                "source": observation.href,
                "validPixels": valid_pixels,
                "contributedPixels": contributed_pixels,
            }
        )

        print("  valid pixels:", valid_pixels)
        print("  contributed pixels:", contributed_pixels)

    return imagery, coverage, contribution_records


def repair_enclosed_gaps(
    imagery: np.ndarray,
    coverage: np.ndarray,
    required_surface: np.ndarray,
) -> list[dict[str, int]]:
    missing = required_surface & ~coverage

    rows, cols = np.where(missing)

    if len(rows) == 0:
        return []

    if len(rows) > MAX_ENCLOSED_GAP_TOTAL_PIXELS:
        raise RuntimeError(
            "Imagery coverage contains too many missing required "
            f"surface pixels for bounded enclosed-gap repair: "
            f"{len(rows)} missing, limit "
            f"{MAX_ENCLOSED_GAP_TOTAL_PIXELS}."
        )

    missing_pixels = set(
        zip(
            rows.tolist(),
            cols.tolist(),
        )
    )

    visited: set[tuple[int, int]] = set()
    components: list[list[tuple[int, int]]] = []

    neighbor_offsets = (
        (-1, -1),
        (-1, 0),
        (-1, 1),
        (0, -1),
        (0, 1),
        (1, -1),
        (1, 0),
        (1, 1),
    )

    for start in sorted(missing_pixels):
        if start in visited:
            continue

        stack = [start]
        visited.add(start)
        component: list[tuple[int, int]] = []

        while stack:
            row, col = stack.pop()
            component.append((row, col))

            for row_offset, col_offset in neighbor_offsets:
                candidate = (
                    row + row_offset,
                    col + col_offset,
                )

                if (
                    candidate in missing_pixels
                    and candidate not in visited
                ):
                    visited.add(candidate)
                    stack.append(candidate)

        components.append(component)

    for component in components:
        if (
            len(component)
            > MAX_ENCLOSED_GAP_COMPONENT_PIXELS
        ):
            raise RuntimeError(
                "Missing imagery component exceeds the bounded "
                "enclosed-gap repair limit: "
                f"{len(component)} pixels, limit "
                f"{MAX_ENCLOSED_GAP_COMPONENT_PIXELS}."
            )

        component_pixels = set(component)
        boundary_pixels: set[tuple[int, int]] = set()

        for row, col in component:
            for row_offset, col_offset in neighbor_offsets:
                neighbor_row = row + row_offset
                neighbor_col = col + col_offset

                if (
                    neighbor_row < 0
                    or neighbor_col < 0
                    or neighbor_row >= coverage.shape[0]
                    or neighbor_col >= coverage.shape[1]
                ):
                    raise RuntimeError(
                        "Missing imagery component touches the "
                        "visual-grid edge."
                    )

                candidate = (
                    neighbor_row,
                    neighbor_col,
                )

                if candidate not in component_pixels:
                    boundary_pixels.add(candidate)

        if any(
            not coverage[row, col]
            for row, col in boundary_pixels
        ):
            raise RuntimeError(
                "Missing imagery component is not fully enclosed "
                "by covered imagery."
            )

    repairs: list[dict[str, int]] = []

    for component in components:
        remaining = set(component)

        while remaining:
            wave: list[
                tuple[
                    int,
                    int,
                    np.ndarray,
                ]
            ] = []

            for row, col in sorted(remaining):
                neighbors = []

                for row_offset, col_offset in neighbor_offsets:
                    neighbor_row = row + row_offset
                    neighbor_col = col + col_offset

                    if coverage[
                        neighbor_row,
                        neighbor_col,
                    ]:
                        neighbors.append(
                            imagery[
                                :,
                                neighbor_row,
                                neighbor_col,
                            ].astype(np.float64)
                        )

                if not neighbors:
                    continue

                replacement = np.rint(
                    np.mean(
                        np.stack(
                            neighbors,
                            axis=0,
                        ),
                        axis=0,
                    )
                ).clip(
                    0,
                    255,
                ).astype(np.uint8)

                wave.append(
                    (
                        row,
                        col,
                        replacement,
                    )
                )

            if not wave:
                raise RuntimeError(
                    "Enclosed imagery gap could not be repaired "
                    "from its covered boundary."
                )

            for row, col, replacement in wave:
                imagery[:, row, col] = replacement
                coverage[row, col] = True
                remaining.remove((row, col))

                repairs.append(
                    {
                        "row": int(row),
                        "col": int(col),
                    }
                )

    return repairs


def write_rgb_raster(
    path: Path,
    imagery: np.ndarray,
    coverage: np.ndarray,
    crs: rasterio.crs.CRS,
    transform: rasterio.Affine,
) -> None:
    profile = {
        "driver": "GTiff",
        "width": imagery.shape[2],
        "height": imagery.shape[1],
        "count": 3,
        "dtype": "uint8",
        "crs": crs,
        "transform": transform,
        "compress": "LZW",
        "photometric": "RGB",
        "tiled": True,
        "blockxsize": 512,
        "blockysize": 512,
    }

    output = imagery.copy()
    output[:, ~coverage] = 0

    with rasterio.Env(GDAL_TIFF_INTERNAL_MASK=True):
        with rasterio.open(
            path,
            "w",
            **profile,
        ) as destination:
            destination.write(output)
            destination.write_mask(
                coverage.astype(np.uint8) * 255
            )


def main() -> None:
    args = parse_args()

    reference = args.reference.resolve()
    output_directory = args.output_directory.resolve()

    (
        target_crs,
        semantic_transform,
        semantic_width,
        semantic_height,
        bounds,
        semantic_resolution,
        semantic_surface,
    ) = read_reference(reference)

    (
        target_transform,
        width,
        height,
        resolution,
    ) = create_visual_grid(bounds)

    required_surface = project_required_surface(
        semantic_surface=semantic_surface,
        source_crs=target_crs,
        source_transform=semantic_transform,
        destination_transform=target_transform,
        width=width,
        height=height,
    )

    required_surface_pixels = int(
        np.count_nonzero(required_surface)
    )

    print("=== EST IMAGERY PREPARATION ===")
    print("reference:", reference)
    print("source type: Sentinel-2 Level-2A TCI")
    print("observations:", len(OBSERVATIONS))
    print("target CRS:", target_crs)
    print(
        "semantic grid:",
        f"{semantic_width} x {semantic_height}",
        semantic_resolution,
    )
    print("visual dimensions:", f"{width} x {height}")
    print("visual resolution:", resolution)
    print("target bounds:", bounds)
    print(
        "required visual-grid surface pixels:",
        required_surface_pixels,
    )

    imagery, coverage, contributions = compose_imagery(
        observations=OBSERVATIONS,
        destination_crs=target_crs,
        destination_transform=target_transform,
        width=width,
        height=height,
    )

    covered_before_repair = int(
        np.count_nonzero(
            coverage & required_surface
        )
    )

    missing_before_repair = (
        required_surface_pixels - covered_before_repair
    )

    print("\n=== COVERAGE BEFORE REPAIR ===")
    print(
        "covered required surface pixels:",
        covered_before_repair,
    )
    print(
        "missing required surface pixels:",
        missing_before_repair,
    )
    print(
        "coverage percent:",
        round(
            covered_before_repair
            / required_surface_pixels
            * 100.0,
            6,
        ),
    )

    repairs = repair_enclosed_gaps(
        imagery=imagery,
        coverage=coverage,
        required_surface=required_surface,
    )

    covered_after_repair = int(
        np.count_nonzero(
            coverage & required_surface
        )
    )

    missing_after_repair = (
        required_surface_pixels - covered_after_repair
    )

    print("\n=== CONTROLLED REPAIR ===")
    print("repaired pixels:", len(repairs))

    for repair in repairs:
        print(
            "repaired row/col:",
            repair["row"],
            repair["col"],
        )

    print("\n=== FINAL COVERAGE ===")
    print(
        "covered required surface pixels:",
        covered_after_repair,
    )
    print(
        "missing required surface pixels:",
        missing_after_repair,
    )

    if missing_after_repair:
        raise RuntimeError(
            "Imagery preparation did not completely cover "
            "the required Est surface after controlled repair."
        )

    output_directory.mkdir(
        parents=True,
        exist_ok=True,
    )

    imagery_path = (
        output_directory
        / IMAGERY_FILENAME
    )

    write_rgb_raster(
        path=imagery_path,
        imagery=imagery,
        coverage=coverage,
        crs=target_crs,
        transform=target_transform,
    )

    manifest = {
        "purpose": "Est continuous visual presentation input",
        "semanticReference": str(
            reference.relative_to(ROOT)
        ),
        "sourceType": "Sentinel-2 Level-2A true-color imagery",
        "observations": contributions,
        "semanticGrid": {
            "width": semantic_width,
            "height": semantic_height,
            "resolutionMetres": {
                "x": semantic_resolution[0],
                "y": semantic_resolution[1],
            },
        },
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
            "visualRgb": str(
                imagery_path.relative_to(ROOT)
            ),
        },
        "processing": {
            "sourceResampling": "bilinear",
            "semanticFootprintResampling": "nearest",
            "composition": (
                "ordered first-valid observations; "
                "2026-07-13 primary imagery wins and selected "
                "2026-07-30 observations fill uncovered pixels"
            ),
            "coverageBeforeRepair": {
                "requiredSurfacePixels": (
                    required_surface_pixels
                ),
                "coveredRequiredSurfacePixels": (
                    covered_before_repair
                ),
                "missingRequiredSurfacePixels": (
                    missing_before_repair
                ),
            },
            "enclosedGapRepair": {
                "maximumComponentPixels": (
                    MAX_ENCLOSED_GAP_COMPONENT_PIXELS
                ),
                "maximumTotalPixels": (
                    MAX_ENCLOSED_GAP_TOTAL_PIXELS
                ),
                "count": len(repairs),
                "pixels": repairs,
                "method": (
                    "bounded fully enclosed coverage gaps are "
                    "filled inward in synchronous layers using "
                    "the mean RGB of currently covered "
                    "eight-neighbor pixels"
                ),
            },
            "coverageAfterRepair": {
                "requiredSurfacePixels": (
                    required_surface_pixels
                ),
                "coveredRequiredSurfacePixels": (
                    covered_after_repair
                ),
                "missingRequiredSurfacePixels": (
                    missing_after_repair
                ),
                "coveredVisualGridPixels": int(
                    np.count_nonzero(coverage)
                ),
            },
        },
        "role": (
            "Presentation input only; not authoritative "
            "simulation semantics."
        ),
    }

    manifest_path = (
        output_directory
        / MANIFEST_FILENAME
    )

    manifest_path.write_text(
        json.dumps(manifest, indent=2) + "\n"
    )

    surface_values = imagery[
        :,
        required_surface,
    ]

    print("\n=== OUTPUT ===")
    print("visual RGB:", imagery_path)
    print("manifest:", manifest_path)

    print("\n=== RGB STATS ON REQUIRED SURFACE ===")

    for band_index, band_name in enumerate(
        ("red", "green", "blue")
    ):
        values = surface_values[band_index]

        print(
            f"{band_name} min:",
            int(np.min(values)),
        )
        print(
            f"{band_name} max:",
            int(np.max(values)),
        )
        print(
            f"{band_name} mean:",
            float(np.mean(values)),
        )


if __name__ == "__main__":
    main()
