"""Geographic TMS tile geometry.

Level zero has two columns and one row. Each subsequent level doubles
both dimensions. TMS Y coordinates increase from south to north.
"""

from __future__ import annotations

from dataclasses import dataclass
from math import ceil, floor

from affine import Affine
from rasterio.coords import BoundingBox


TILE_SIZE = 256


@dataclass(frozen=True)
class Tile:
    level: int
    x: int
    y: int


def dimensions(level: int) -> tuple[int, int]:
    if level < 0:
        raise ValueError("Tile level must be nonnegative.")

    return 2 << level, 1 << level


def degrees_per_pixel(
    level: int,
    tile_size: int = TILE_SIZE,
) -> float:
    if tile_size <= 0:
        raise ValueError("Tile size must be positive.")

    columns, _ = dimensions(level)
    return 360.0 / (columns * tile_size)


def tile_bounds(tile: Tile) -> BoundingBox:
    columns, rows = dimensions(tile.level)

    if not (0 <= tile.x < columns and 0 <= tile.y < rows):
        raise ValueError("Tile coordinates are outside the geographic grid.")

    width = 360.0 / columns
    height = 180.0 / rows

    west = -180.0 + tile.x * width
    south = -90.0 + tile.y * height

    return BoundingBox(
        left=west,
        bottom=south,
        right=west + width,
        top=south + height,
    )


def tile_transform(
    tile: Tile,
    tile_size: int = TILE_SIZE,
) -> Affine:
    if tile_size <= 0:
        raise ValueError("Tile size must be positive.")

    bounds = tile_bounds(tile)

    return Affine(
        (bounds.right - bounds.left) / tile_size,
        0.0,
        bounds.left,
        0.0,
        -(bounds.top - bounds.bottom) / tile_size,
        bounds.top,
    )


def tiles_for_bounds(
    bounds: BoundingBox,
    level: int,
) -> list[Tile]:
    """Return tiles intersecting a non-wrapping geographic rectangle."""

    columns, rows = dimensions(level)

    if not (
        -180.0 <= bounds.left < bounds.right <= 180.0
        and -90.0 <= bounds.bottom < bounds.top <= 90.0
    ):
        raise ValueError("Bounds must be a valid geographic rectangle.")

    width = 360.0 / columns
    height = 180.0 / rows

    west = max(0, floor((bounds.left + 180.0) / width))
    east = min(
        columns - 1,
        ceil((bounds.right + 180.0) / width) - 1,
    )
    south = max(0, floor((bounds.bottom + 90.0) / height))
    north = min(
        rows - 1,
        ceil((bounds.top + 90.0) / height) - 1,
    )

    return [
        Tile(level, x, y)
        for y in range(south, north + 1)
        for x in range(west, east + 1)
    ]


def choose_minimum_level(
    bounds: BoundingBox,
    maximum_tiles: int = 4,
) -> int:
    if maximum_tiles <= 0:
        raise ValueError("Maximum tile count must be positive.")

    level = 0

    while True:
        next_level = level + 1

        if len(tiles_for_bounds(bounds, next_level)) > maximum_tiles:
            return level

        level = next_level

        if level >= 30:
            return level


def choose_maximum_level(
    source_degrees_per_pixel: float,
    tile_size: int = TILE_SIZE,
) -> int:
    if source_degrees_per_pixel <= 0:
        raise ValueError("Source resolution must be positive.")

    level = 0

    while degrees_per_pixel(level, tile_size) > source_degrees_per_pixel:
        level += 1

        if level >= 30:
            return level

    return level
