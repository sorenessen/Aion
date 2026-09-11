#!/usr/bin/env python3

"""Validate an Est static geographic TMS surface pyramid."""

from __future__ import annotations

import argparse
import json
import sys
import warnings
from pathlib import Path
from xml.etree import ElementTree

import numpy as np
import rasterio
from rasterio.errors import NotGeoreferencedWarning

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.presentation import load_surface_presentation_model  # noqa: E402
from surface.tms import TILE_SIZE, Tile, tile_bounds  # noqa: E402


def assert_close(
    actual: float,
    expected: float,
    label: str,
    tolerance: float = 1e-10,
) -> None:
    if abs(actual - expected) > tolerance:
        raise ValueError(
            f"{label}: expected {expected}, got {actual}"
        )


def validate_tile(
    path: Path,
    tile: Tile,
) -> tuple[int, int]:
    # Validate the tile coordinates against the geographic grid.
    # Individual PNG files intentionally carry no georeferencing
    # sidecars; their geographic placement comes from TMS metadata
    # plus the level/x/y path.
    tile_bounds(tile)

    with warnings.catch_warnings():
        warnings.simplefilter(
            "ignore",
            NotGeoreferencedWarning,
        )

        with rasterio.open(path) as dataset:
            if (
                dataset.width != TILE_SIZE
                or dataset.height != TILE_SIZE
            ):
                raise ValueError(
                    f"{path}: expected "
                    f"{TILE_SIZE}x{TILE_SIZE}, "
                    f"got {dataset.width}x{dataset.height}"
                )

            if dataset.count != 4:
                raise ValueError(
                    f"{path}: expected four RGBA bands, "
                    f"got {dataset.count}"
                )

            if any(
                dtype != "uint8"
                for dtype in dataset.dtypes
            ):
                raise ValueError(
                    f"{path}: expected uint8 bands, "
                    f"got {dataset.dtypes}"
                )

            bands = dataset.read()

    rgba = np.moveaxis(bands, 0, -1)
    alpha = rgba[:, :, 3]

    unexpected_alpha = set(
        int(value)
        for value in np.unique(alpha)
    ) - {0, 255}

    if unexpected_alpha:
        raise ValueError(
            f"{path}: contains unexpected alpha values: "
            f"{sorted(unexpected_alpha)}"
        )

    transparent = alpha == 0

    if np.any(rgba[transparent, :3] != 0):
        raise ValueError(
            f"{path}: transparent pixels must have zero RGB."
        )

    opaque_pixel_count = int(
        np.count_nonzero(alpha)
    )
    distinct_rgb_count = len(
        np.unique(
            rgba[alpha == 255, :3],
            axis=0,
        )
    )

    return opaque_pixel_count, distinct_rgb_count


def validate_tilemapresource(
    path: Path,
    minimum_level: int,
    maximum_level: int,
    bounds: dict,
) -> None:
    root = ElementTree.parse(path).getroot()

    if root.tag != "TileMap":
        raise ValueError(
            f"{path}: root element must be TileMap"
        )

    srs = root.findtext("SRS")

    if srs != "EPSG:4326":
        raise ValueError(
            f"{path}: expected EPSG:4326 SRS, got {srs!r}"
        )

    bounding_box = root.find("BoundingBox")

    if bounding_box is None:
        raise ValueError(
            f"{path}: missing BoundingBox"
        )

    assert_close(
        float(bounding_box.attrib["minx"]),
        bounds["west"],
        "tilemapresource west bound",
    )
    assert_close(
        float(bounding_box.attrib["miny"]),
        bounds["south"],
        "tilemapresource south bound",
    )
    assert_close(
        float(bounding_box.attrib["maxx"]),
        bounds["east"],
        "tilemapresource east bound",
    )
    assert_close(
        float(bounding_box.attrib["maxy"]),
        bounds["north"],
        "tilemapresource north bound",
    )

    tile_format = root.find("TileFormat")

    if tile_format is None:
        raise ValueError(
            f"{path}: missing TileFormat"
        )

    expected_format = {
        "width": str(TILE_SIZE),
        "height": str(TILE_SIZE),
        "mime-type": "image/png",
        "extension": "png",
    }

    for key, expected in expected_format.items():
        actual = tile_format.attrib.get(key)

        if actual != expected:
            raise ValueError(
                f"{path}: TileFormat {key} expected "
                f"{expected!r}, got {actual!r}"
            )

    tile_sets = root.find("TileSets")

    if tile_sets is None:
        raise ValueError(
            f"{path}: missing TileSets"
        )

    if tile_sets.attrib.get("profile") != "geodetic":
        raise ValueError(
            f"{path}: TileSets profile must be geodetic"
        )

    orders = [
        int(tile_set.attrib["order"])
        for tile_set in tile_sets.findall("TileSet")
    ]

    expected_orders = list(
        range(minimum_level, maximum_level + 1)
    )

    if orders != expected_orders:
        raise ValueError(
            f"{path}: expected TileSet orders "
            f"{expected_orders}, got {orders}"
        )


def validate_pyramid(
    directory: Path,
    verbose: bool = False,
) -> dict:
    directory = directory.resolve()

    manifest_path = directory / "surface-tms-manifest.json"
    tilemap_path = directory / "tilemapresource.xml"

    if not manifest_path.is_file():
        raise ValueError(
            f"Missing pyramid manifest: {manifest_path}"
        )

    if not tilemap_path.is_file():
        raise ValueError(
            f"Missing TMS metadata: {tilemap_path}"
        )

    manifest = json.loads(manifest_path.read_text())

    if manifest.get("scheme") != "TMS":
        raise ValueError("Manifest scheme must be TMS.")

    if manifest.get("profile") != "geodetic":
        raise ValueError(
            "Manifest profile must be geodetic."
        )

    if manifest.get("crs") != "EPSG:4326":
        raise ValueError(
            "Manifest CRS must be EPSG:4326."
        )

    if manifest.get("tileSize") != TILE_SIZE:
        raise ValueError(
            f"Manifest tileSize must be {TILE_SIZE}."
        )

    if manifest.get("yOrigin") != "south":
        raise ValueError(
            "Manifest yOrigin must be south."
        )

    presentation = load_surface_presentation_model()

    if (
        manifest.get("surfacePresentationVersion")
        != presentation.version
    ):
        raise ValueError(
            "Manifest surfacePresentationVersion does not "
            "match the current Est presentation definition."
        )

    if (
        manifest.get("surfacePresentationName")
        != presentation.name
    ):
        raise ValueError(
            "Manifest surfacePresentationName does not "
            "match the current Est presentation definition."
        )

    minimum_level = int(manifest["minimumLevel"])
    maximum_level = int(manifest["maximumLevel"])

    if maximum_level < minimum_level:
        raise ValueError(
            "Manifest maximumLevel precedes minimumLevel."
        )

    expected_levels = list(
        range(minimum_level, maximum_level + 1)
    )

    level_records = manifest.get("levels", [])

    manifest_levels = [
        int(record["level"])
        for record in level_records
    ]

    if manifest_levels != expected_levels:
        raise ValueError(
            "Manifest levels are not contiguous: "
            f"{manifest_levels}"
        )

    validate_tilemapresource(
        tilemap_path,
        minimum_level,
        maximum_level,
        manifest["bounds"],
    )

    total_opaque_pixels = 0
    maximum_distinct_rgb_per_tile = 0
    total_tiles = 0

    for record in level_records:
        level = int(record["level"])
        expected_count = int(record["tileCount"])

        level_directory = directory / str(level)

        if not level_directory.is_dir():
            raise ValueError(
                f"Missing level directory: {level_directory}"
            )

        tile_paths = sorted(
            level_directory.glob("*/*.png")
        )

        if len(tile_paths) != expected_count:
            raise ValueError(
                f"Level {level}: manifest declares "
                f"{expected_count} tiles, found "
                f"{len(tile_paths)}"
            )

        for path in tile_paths:
            try:
                x = int(path.parent.name)
                y = int(path.stem)
            except ValueError as error:
                raise ValueError(
                    f"Invalid TMS tile path: {path}"
                ) from error

            (
                opaque_pixel_count,
                distinct_rgb_count,
            ) = validate_tile(
                path,
                Tile(level, x, y),
            )

            total_opaque_pixels += opaque_pixel_count
            maximum_distinct_rgb_per_tile = max(
                maximum_distinct_rgb_per_tile,
                distinct_rgb_count,
            )

        total_tiles += len(tile_paths)

        if verbose:
            print(
                f"z={level}: validated "
                f"{len(tile_paths)} tiles"
            )

    declared_total = int(manifest["totalTileCount"])

    if total_tiles != declared_total:
        raise ValueError(
            f"Manifest declares {declared_total} total tiles, "
            f"validated {total_tiles}"
        )

    all_pngs = list(directory.rglob("*.png"))

    if len(all_pngs) != total_tiles:
        raise ValueError(
            "PNG files exist outside declared level structure: "
            f"declared {total_tiles}, found {len(all_pngs)}"
        )

    aux_files = list(directory.rglob("*.aux.xml"))

    if aux_files:
        raise ValueError(
            "Unexpected Rasterio/GDAL sidecar files found: "
            + ", ".join(str(path) for path in aux_files[:10])
        )

    return {
        "minimumLevel": minimum_level,
        "maximumLevel": maximum_level,
        "validatedTileCount": total_tiles,
        "opaquePixelCount": total_opaque_pixels,
        "maximumDistinctRgbPerTile": (
            maximum_distinct_rgb_per_tile
        ),
    }


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Validate an Est static geographic "
            "TMS surface pyramid."
        )
    )
    parser.add_argument(
        "directory",
        type=Path,
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
    )

    args = parser.parse_args()

    result = validate_pyramid(
        args.directory,
        verbose=args.verbose,
    )

    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
