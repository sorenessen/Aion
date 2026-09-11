#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path
from xml.etree.ElementTree import Element, SubElement, ElementTree

import numpy as np
import rasterio
from rasterio.coords import BoundingBox
from rasterio.enums import Resampling
from rasterio.warp import calculate_default_transform, reproject

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.tms import (  # noqa: E402
    TILE_SIZE,
    Tile,
    choose_maximum_level,
    choose_minimum_level,
    degrees_per_pixel,
    tile_bounds,
    tile_transform,
    tiles_for_bounds,
)
from surface.presentation import (  # noqa: E402
    SurfacePresentation,
    load_surface_presentation_model,
    render_surface_material,
)
from surface.validate_tms import validate_pyramid  # noqa: E402


DEFINITION_PATH = (
    ROOT
    / "src"
    / "Est.Web"
    / "src"
    / "surface"
    / "surface-categories.json"
)


def load_category_model() -> tuple[
    int,
    SurfacePresentation,
    set[int],
]:
    definition = json.loads(DEFINITION_PATH.read_text())
    presentation = load_surface_presentation_model()

    valid_category_ids = {
        int(category["id"])
        for category in definition["categories"].values()
    }

    if (
        set(presentation.materials_by_id)
        != valid_category_ids
    ):
        raise RuntimeError(
            "Surface presentation category IDs do not match "
            "the semantic category definition."
        )

    return (
        int(definition["version"]),
        presentation,
        valid_category_ids,
    )


def geographic_source_geometry(
    source: rasterio.io.DatasetReader,
) -> tuple[BoundingBox, float]:
    transform, width, height = calculate_default_transform(
        source.crs,
        "EPSG:4326",
        source.width,
        source.height,
        *source.bounds,
    )

    bounds = BoundingBox(
        left=transform.c,
        bottom=transform.f + transform.e * height,
        right=transform.c + transform.a * width,
        top=transform.f,
    )

    resolution = max(abs(transform.a), abs(transform.e))

    return bounds, resolution


def render_tile(
    source: rasterio.io.DatasetReader,
    tile: Tile,
    presentation: SurfacePresentation,
    valid_category_ids: set[int],
) -> tuple[np.ndarray, set[int]]:
    destination = np.zeros(
        (TILE_SIZE, TILE_SIZE),
        dtype=np.uint8,
    )

    reproject(
        source=rasterio.band(source, 1),
        destination=destination,
        src_transform=source.transform,
        src_crs=source.crs,
        src_nodata=source.nodata,
        dst_transform=tile_transform(tile),
        dst_crs="EPSG:4326",
        dst_nodata=0,
        resampling=Resampling.nearest,
    )

    present_ids = {
        int(value)
        for value in np.unique(destination)
    }

    unexpected_ids = sorted(
        present_ids - valid_category_ids
    )

    if unexpected_ids:
        raise RuntimeError(
            "Unexpected Est surface category IDs in tile "
            f"{tile}: {unexpected_ids}"
        )

    transform = tile_transform(tile)

    columns = np.arange(
        TILE_SIZE,
        dtype=np.float64,
    ) + 0.5
    rows = np.arange(
        TILE_SIZE,
        dtype=np.float64,
    ) + 0.5

    longitude = (
        transform.c
        + columns[np.newaxis, :]
        * transform.a
    )
    longitude = np.broadcast_to(
        longitude,
        destination.shape,
    )

    latitude = (
        transform.f
        + rows[:, np.newaxis]
        * transform.e
    )
    latitude = np.broadcast_to(
        latitude,
        destination.shape,
    )

    rgba = render_surface_material(
        destination,
        longitude,
        latitude,
        presentation,
    )

    return rgba, present_ids


def write_png(
    output_path: Path,
    tile: Tile,
    rgba: np.ndarray,
) -> None:
    output_path.parent.mkdir(
        parents=True,
        exist_ok=True,
    )

    with rasterio.open(
        output_path,
        "w",
        driver="PNG",
        width=TILE_SIZE,
        height=TILE_SIZE,
        count=4,
        dtype="uint8",
        transform=tile_transform(tile),
        crs="EPSG:4326",
    ) as output:
        for band in range(4):
            output.write(
                rgba[:, :, band],
                band + 1,
            )

    aux_path = Path(f"{output_path}.aux.xml")

    if aux_path.exists():
        aux_path.unlink()


def write_tilemapresource(
    output_directory: Path,
    bounds: BoundingBox,
    minimum_level: int,
    maximum_level: int,
) -> None:
    root = Element(
        "TileMap",
        {
            "version": "1.0.0",
            "tilemapservice": "http://tms.osgeo.org/1.0.0",
        },
    )

    SubElement(root, "Title").text = (
        "Est NLCD 2025 Regional Surface Study"
    )
    SubElement(root, "Abstract").text = (
        "Est-owned categorical surface rendering tiles "
        "derived from USGS Annual NLCD 2025."
    )
    SubElement(root, "SRS").text = "EPSG:4326"

    SubElement(
        root,
        "BoundingBox",
        {
            "minx": repr(bounds.left),
            "miny": repr(bounds.bottom),
            "maxx": repr(bounds.right),
            "maxy": repr(bounds.top),
        },
    )

    SubElement(
        root,
        "Origin",
        {
            "x": "-180.0",
            "y": "-90.0",
        },
    )

    SubElement(
        root,
        "TileFormat",
        {
            "width": str(TILE_SIZE),
            "height": str(TILE_SIZE),
            "mime-type": "image/png",
            "extension": "png",
        },
    )

    tile_sets = SubElement(
        root,
        "TileSets",
        {"profile": "geodetic"},
    )

    for level in range(
        minimum_level,
        maximum_level + 1,
    ):
        SubElement(
            tile_sets,
            "TileSet",
            {
                "href": str(level),
                "units-per-pixel": repr(
                    degrees_per_pixel(level)
                ),
                "order": str(level),
            },
        )

    tree = ElementTree(root)
    tree.write(
        output_directory / "tilemapresource.xml",
        encoding="utf-8",
        xml_declaration=True,
    )


def generate_pyramid_contents(
    source_path: Path,
    output_directory: Path,
) -> dict:
    (
        definition_version,
        presentation,
        valid_category_ids,
    ) = load_category_model()

    if output_directory.exists():
        shutil.rmtree(output_directory)

    output_directory.mkdir(
        parents=True,
        exist_ok=True,
    )

    with rasterio.open(source_path) as source:
        if source.crs is None:
            raise SystemExit(
                "Source category raster has no CRS."
            )

        bounds, source_resolution = (
            geographic_source_geometry(source)
        )

        minimum_level = choose_minimum_level(bounds)
        maximum_level = choose_maximum_level(
            source_resolution
        )

        if maximum_level < minimum_level:
            maximum_level = minimum_level

        levels = []
        total_tiles = 0

        for level in range(
            minimum_level,
            maximum_level + 1,
        ):
            tiles = tiles_for_bounds(
                bounds,
                level,
            )

            opaque_tiles = 0
            transparent_tiles = 0
            present_category_ids: set[int] = set()

            for tile in tiles:
                rgba, present_ids = render_tile(
                    source,
                    tile,
                    presentation,
                    valid_category_ids,
                )

                present_category_ids.update(
                    present_ids
                )

                opaque_pixel_count = int(
                    np.count_nonzero(
                        rgba[:, :, 3]
                    )
                )

                if opaque_pixel_count:
                    opaque_tiles += 1
                else:
                    transparent_tiles += 1

                output_path = (
                    output_directory
                    / str(tile.level)
                    / str(tile.x)
                    / f"{tile.y}.png"
                )

                write_png(
                    output_path,
                    tile,
                    rgba,
                )

            total_tiles += len(tiles)

            levels.append(
                {
                    "level": level,
                    "degreesPerPixel": (
                        degrees_per_pixel(level)
                    ),
                    "tileCount": len(tiles),
                    "opaqueTileCount": opaque_tiles,
                    "transparentTileCount": (
                        transparent_tiles
                    ),
                    "presentCategoryIds": sorted(
                        present_category_ids
                    ),
                }
            )

    write_tilemapresource(
        output_directory,
        bounds,
        minimum_level,
        maximum_level,
    )

    manifest = {
        "surfaceCategoryDefinitionVersion": (
            definition_version
        ),
        "surfacePresentationVersion": (
            presentation.version
        ),
        "surfacePresentationName": presentation.name,
        "source": str(source_path),
        "scheme": "TMS",
        "profile": "geodetic",
        "crs": "EPSG:4326",
        "tileSize": TILE_SIZE,
        "yOrigin": "south",
        "bounds": {
            "west": bounds.left,
            "south": bounds.bottom,
            "east": bounds.right,
            "north": bounds.top,
        },
        "sourceGeographicDegreesPerPixel": (
            source_resolution
        ),
        "minimumLevel": minimum_level,
        "maximumLevel": maximum_level,
        "totalTileCount": total_tiles,
        "levels": levels,
        "notes": [
            (
                "Nearest-neighbor reprojection preserves "
                "categorical surface values."
            ),
            (
                "Unknown category pixels are rendered "
                "transparent."
            ),
            (
                "TMS Y coordinates increase from south "
                "to north."
            ),
        ],
    }

    (
        output_directory / "surface-tms-manifest.json"
    ).write_text(
        json.dumps(
            manifest,
            indent=2,
        )
        + "\n"
    )

    return manifest


def publish_pyramid(
    source_path: Path,
    output_directory: Path,
) -> dict:
    output_directory = output_directory.resolve()
    parent = output_directory.parent
    staging = parent / f".{output_directory.name}.staging"
    previous = parent / f".{output_directory.name}.previous"

    if staging.exists():
        shutil.rmtree(staging)

    if previous.exists():
        shutil.rmtree(previous)

    moved_previous = False

    try:
        manifest = generate_pyramid_contents(
            source_path,
            staging,
        )

        validation = validate_pyramid(
            staging,
            verbose=False,
        )

        if (
            validation["validatedTileCount"]
            != manifest["totalTileCount"]
        ):
            raise RuntimeError(
                "Validated tile count does not match "
                "generated manifest."
            )

        if output_directory.exists():
            output_directory.rename(previous)
            moved_previous = True

        staging.rename(output_directory)

        if previous.exists():
            shutil.rmtree(previous)

        return manifest

    except Exception:
        if moved_previous:
            if output_directory.exists():
                shutil.rmtree(output_directory)

            if previous.exists():
                previous.rename(output_directory)

        raise

    finally:
        if staging.exists():
            shutil.rmtree(staging)

        if (
            previous.exists()
            and not moved_previous
        ):
            shutil.rmtree(previous)


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Generate a static geographic TMS pyramid "
            "from an Est surface-category raster."
        )
    )
    parser.add_argument(
        "source",
        type=Path,
    )
    parser.add_argument(
        "--output-directory",
        type=Path,
        default=Path(
            "data/generated/nlcd/surface-tms"
        ),
    )

    args = parser.parse_args()

    manifest = publish_pyramid(
        args.source,
        args.output_directory,
    )

    print(
        json.dumps(
            manifest,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
