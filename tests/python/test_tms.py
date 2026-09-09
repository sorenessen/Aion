"""Tests for the offline geographic TMS grid."""

import sys
import unittest
from pathlib import Path

from rasterio.coords import BoundingBox

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.tms import (  # noqa: E402
    Tile,
    choose_maximum_level,
    choose_minimum_level,
    degrees_per_pixel,
    dimensions,
    tile_bounds,
    tile_transform,
    tiles_for_bounds,
)


STUDY_BOUNDS = BoundingBox(
    -123.61633988971845,
    46.23644679019992,
    -121.00788955795646,
    47.76234468508979,
)


class GeographicTmsTests(unittest.TestCase):
    def test_level_zero_has_two_hemisphere_tiles(self):
        self.assertEqual(dimensions(0), (2, 1))
        self.assertEqual(
            tile_bounds(Tile(0, 0, 0)),
            BoundingBox(-180.0, -90.0, 0.0, 90.0),
        )
        self.assertEqual(
            tile_bounds(Tile(0, 1, 0)),
            BoundingBox(0.0, -90.0, 180.0, 90.0),
        )

    def test_y_increases_from_south_to_north(self):
        self.assertEqual(
            tile_bounds(Tile(1, 0, 0)),
            BoundingBox(-180.0, -90.0, -90.0, 0.0),
        )
        self.assertEqual(
            tile_bounds(Tile(1, 0, 1)),
            BoundingBox(-180.0, 0.0, -90.0, 90.0),
        )

    def test_transform_is_north_up(self):
        transform = tile_transform(Tile(1, 0, 1))

        self.assertEqual(transform.a, 90.0 / 256)
        self.assertEqual(transform.e, -90.0 / 256)
        self.assertEqual(transform.c, -180.0)
        self.assertEqual(transform.f, 90.0)

    def test_regional_bounds_select_expected_tiles(self):
        self.assertEqual(
            tiles_for_bounds(
                BoundingBox(-123.3, 46.6, -121.3, 47.4),
                2,
            ),
            [Tile(2, 1, 3)],
        )

    def test_exact_tile_boundary_does_not_include_neighbor(self):
        self.assertEqual(
            tiles_for_bounds(
                BoundingBox(-180.0, -90.0, -90.0, 0.0),
                1,
            ),
            [Tile(1, 0, 0)],
        )

    def test_invalid_coordinates_are_rejected(self):
        with self.assertRaises(ValueError):
            tile_bounds(Tile(0, 2, 0))

        with self.assertRaises(ValueError):
            dimensions(-1)

        with self.assertRaises(ValueError):
            tile_transform(Tile(0, 0, 0), 0)

    def test_invalid_bounds_are_rejected(self):
        with self.assertRaises(ValueError):
            tiles_for_bounds(
                BoundingBox(10.0, 0.0, -10.0, 20.0),
                2,
            )

    def test_degrees_per_pixel_halves_each_level(self):
        self.assertAlmostEqual(
            degrees_per_pixel(11),
            0.00034332275390625,
        )
        self.assertAlmostEqual(
            degrees_per_pixel(10),
            degrees_per_pixel(11) * 2,
        )

    def test_study_minimum_level_keeps_tile_count_at_four(self):
        self.assertEqual(choose_minimum_level(STUDY_BOUNDS), 7)
        self.assertEqual(len(tiles_for_bounds(STUDY_BOUNDS, 7)), 4)
        self.assertEqual(len(tiles_for_bounds(STUDY_BOUNDS, 8)), 12)

    def test_source_resolution_selects_level_eleven(self):
        self.assertEqual(
            choose_maximum_level(0.0003795766),
            11,
        )


if __name__ == "__main__":
    unittest.main()
