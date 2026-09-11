#!/usr/bin/env python3

"""Prepare continuous imagery presentation input for Est.

Sentinel-2 visual imagery is normalized onto Est's existing regional
metric working grid. Source acquisition and mosaicking details remain
inside this preparation boundary so downstream presentation code can
consume a generic aligned RGB field without knowing about Sentinel,
MGRS, STAC, or source-scene geometry.

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

MAX_ISOLATED_PIXEL_REPAIRS = 16


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

            masks = vrt.read_masks(
                indexes=(1, 2, 3),
            )

    valid = np.all(masks > 0, axis=0)

    return values, valid


def compose_imagery(
    observations: tuple[Observation, ...],
    destination_crs: rasterio.crs.CRS,
    destination_transform: rasterio.Affine,
    width: int,
    height: int,
    semantic_surface: np.ndarray,
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

        useful = valid & semantic_surface
        write_mask = useful & ~coverage

        useful_pixels = int(
            np.count_nonzero(useful)
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
                "validSemanticPixels": useful_pixels,
                "contributedPixels": contributed_pixels,
            }
        )

        print("  valid semantic pixels:", useful_pixels)
        print("  contributed pixels:", contributed_pixels)

    return imagery, coverage, contribution_records


def repair_isolated_pixels(
    imagery: np.ndarray,
    coverage: np.ndarray,
    semantic_surface: np.ndarray,
) -> list[dict[str, int]]:
    missing = semantic_surface & ~coverage

    rows, cols = np.where(missing)

    if len(rows) == 0:
        return []

    if len(rows) > MAX_ISOLATED_PIXEL_REPAIRS:
        raise RuntimeError(
            "Imagery coverage contains too many missing semantic "
            f"pixels for isolated repair: {len(rows)} missing, "
            f"limit {MAX_ISOLATED_PIXEL_REPAIRS}."
        )

    repairs: list[dict[str, int]] = []

    for row, col in zip(rows, cols):
        r0 = row - 1
        r1 = row + 2
        c0 = col - 1
        c1 = col + 2

        if (
            r0 < 0
            or c0 < 0
            or r1 > coverage.shape[0]
            or c1 > coverage.shape[1]
        ):
            raise RuntimeError(
                "Missing imagery pixel touches the working-grid "
                f"edge at row {row}, col {col}."
            )

        neighborhood_coverage = coverage[
            r0:r1,
            c0:c1,
        ].copy()

        neighborhood_coverage[1, 1] = True

        if not np.all(neighborhood_coverage):
            raise RuntimeError(
                "Missing imagery is not an isolated single-pixel "
                f"hole at row {row}, col {col}."
            )

        neighbors = []

        for neighbor_row in range(r0, r1):
            for neighbor_col in range(c0, c1):
                if (
                    neighbor_row == row
                    and neighbor_col == col
                ):
                    continue

                neighbors.append(
                    imagery[
                        :,
                        neighbor_row,
                        neighbor_col,
                    ].astype(np.float64)
                )

        replacement = np.rint(
            np.mean(
                np.stack(neighbors, axis=0),
                axis=0,
            )
        ).clip(0, 255).astype(np.uint8)

        imagery[:, row, col] = replacement
        coverage[row, col] = True

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
    semantic_surface: np.ndarray,
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
    output[:, ~semantic_surface] = 0

    with rasterio.Env(GDAL_TIFF_INTERNAL_MASK=True):
        with rasterio.open(
            path,
            "w",
            **profile,
        ) as destination:
            destination.write(output)
            destination.write_mask(
                semantic_surface.astype(np.uint8) * 255
            )


def main() -> None:
    args = parse_args()

    reference = args.reference.resolve()
    output_directory = args.output_directory.resolve()

    (
        target_crs,
        target_transform,
        width,
        height,
        bounds,
        resolution,
        semantic_surface,
    ) = read_reference(reference)

    semantic_pixels = int(
        np.count_nonzero(semantic_surface)
    )

    print("=== EST IMAGERY PREPARATION ===")
    print("reference:", reference)
    print("source type: Sentinel-2 Level-2A TCI")
    print("observations:", len(OBSERVATIONS))
    print("target CRS:", target_crs)
    print("target dimensions:", f"{width} x {height}")
    print("target resolution:", resolution)
    print("target bounds:", bounds)
    print("semantic pixels:", semantic_pixels)

    imagery, coverage, contributions = compose_imagery(
        observations=OBSERVATIONS,
        destination_crs=target_crs,
        destination_transform=target_transform,
        width=width,
        height=height,
        semantic_surface=semantic_surface,
    )

    covered_before_repair = int(
        np.count_nonzero(
            coverage & semantic_surface
        )
    )

    missing_before_repair = (
        semantic_pixels - covered_before_repair
    )

    print("\n=== COVERAGE BEFORE REPAIR ===")
    print(
        "covered semantic pixels:",
        covered_before_repair,
    )
    print(
        "missing semantic pixels:",
        missing_before_repair,
    )
    print(
        "coverage percent:",
        round(
            covered_before_repair
            / semantic_pixels
            * 100.0,
            6,
        ),
    )

    repairs = repair_isolated_pixels(
        imagery=imagery,
        coverage=coverage,
        semantic_surface=semantic_surface,
    )

    covered_after_repair = int(
        np.count_nonzero(
            coverage & semantic_surface
        )
    )

    missing_after_repair = (
        semantic_pixels - covered_after_repair
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
        "covered semantic pixels:",
        covered_after_repair,
    )
    print(
        "missing semantic pixels:",
        missing_after_repair,
    )

    if missing_after_repair:
        raise RuntimeError(
            "Imagery preparation did not completely cover "
            "the Est semantic surface after controlled repair."
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
        semantic_surface=semantic_surface,
        crs=target_crs,
        transform=target_transform,
    )

    manifest = {
        "purpose": "Est continuous visual presentation input",
        "referenceGrid": str(
            reference.relative_to(ROOT)
        ),
        "sourceType": "Sentinel-2 Level-2A true-color imagery",
        "observations": contributions,
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
            "composition": (
                "ordered first-valid observations; "
                "2026-07-13 primary imagery wins and selected "
                "2026-07-30 observations fill uncovered pixels"
            ),
            "coverageBeforeRepair": {
                "coveredSemanticPixels": (
                    covered_before_repair
                ),
                "missingSemanticPixels": (
                    missing_before_repair
                ),
            },
            "isolatedPixelRepair": {
                "maximumAllowed": (
                    MAX_ISOLATED_PIXEL_REPAIRS
                ),
                "count": len(repairs),
                "pixels": repairs,
                "method": (
                    "mean RGB of the eight fully covered "
                    "neighboring pixels, rounded to uint8"
                ),
            },
            "coverageAfterRepair": {
                "coveredSemanticPixels": (
                    covered_after_repair
                ),
                "missingSemanticPixels": (
                    missing_after_repair
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
        semantic_surface,
    ]

    print("\n=== OUTPUT ===")
    print("visual RGB:", imagery_path)
    print("manifest:", manifest_path)

    print("\n=== RGB STATS ON SEMANTIC SURFACE ===")

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
