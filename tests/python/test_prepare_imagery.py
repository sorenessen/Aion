"""Tests for Est imagery presentation-input preparation."""

import sys
import unittest
from pathlib import Path
from unittest.mock import patch

import numpy as np
from rasterio.coords import BoundingBox
from rasterio.crs import CRS
from rasterio.transform import from_origin

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.prepare_imagery import (  # noqa: E402
    Observation,
    compose_imagery,
    create_visual_grid,
    project_required_surface,
    repair_enclosed_gaps,
)


class PrepareImageryTests(unittest.TestCase):
    def test_create_visual_grid_uses_reference_bounds_at_ten_metres(self):
        bounds = BoundingBox(
            left=-2069895.0,
            bottom=2880765.0,
            right=-1898745.0,
            top=3007665.0,
        )

        transform, width, height, resolution = create_visual_grid(bounds)

        self.assertEqual(width, 17115)
        self.assertEqual(height, 12690)
        self.assertEqual(resolution, (10.0, 10.0))
        self.assertEqual(
            transform,
            from_origin(
                bounds.left,
                bounds.top,
                10.0,
                10.0,
            ),
        )

    def test_create_visual_grid_rejects_non_divisible_bounds(self):
        bounds = BoundingBox(
            left=0.0,
            bottom=0.0,
            right=101.0,
            top=100.0,
        )

        with self.assertRaisesRegex(
            RuntimeError,
            "not evenly divisible",
        ):
            create_visual_grid(bounds)

    def test_project_required_surface_uses_nearest_neighbor(self):
        semantic_surface = np.array(
            [
                [True, False],
                [False, True],
            ],
            dtype=bool,
        )

        projected = project_required_surface(
            semantic_surface=semantic_surface,
            source_crs=CRS.from_epsg(3857),
            source_transform=from_origin(
                0.0,
                60.0,
                30.0,
                30.0,
            ),
            destination_transform=from_origin(
                0.0,
                60.0,
                10.0,
                10.0,
            ),
            width=6,
            height=6,
        )

        expected = np.block(
            [
                [
                    np.ones((3, 3), dtype=bool),
                    np.zeros((3, 3), dtype=bool),
                ],
                [
                    np.zeros((3, 3), dtype=bool),
                    np.ones((3, 3), dtype=bool),
                ],
            ]
        )

        np.testing.assert_array_equal(projected, expected)

    def test_compose_imagery_preserves_first_valid_observation(self):
        primary = Observation(
            priority=10,
            date="2026-07-13",
            tile="primary",
            item_id="primary",
            cloud_cover_percent=0.0,
            href="primary",
            role="primary",
        )

        fallback = Observation(
            priority=20,
            date="2026-07-30",
            tile="fallback",
            item_id="fallback",
            cloud_cover_percent=0.0,
            href="fallback",
            role="fallback",
        )

        primary_values = np.zeros(
            (3, 2, 2),
            dtype=np.uint8,
        )
        primary_values[:, 0, 0] = (10, 20, 30)
        primary_values[:, 0, 1] = (40, 50, 60)

        primary_valid = np.array(
            [
                [True, True],
                [False, False],
            ],
            dtype=bool,
        )

        fallback_values = np.zeros(
            (3, 2, 2),
            dtype=np.uint8,
        )
        fallback_values[:, 0, 1] = (200, 201, 202)
        fallback_values[:, 1, 0] = (70, 80, 90)
        fallback_values[:, 1, 1] = (100, 110, 120)

        fallback_valid = np.array(
            [
                [False, True],
                [True, True],
            ],
            dtype=bool,
        )

        def fake_read(
            observation,
            destination_crs,
            destination_transform,
            width,
            height,
        ):
            del (
                destination_crs,
                destination_transform,
                width,
                height,
            )

            if observation.item_id == "primary":
                return primary_values, primary_valid

            if observation.item_id == "fallback":
                return fallback_values, fallback_valid

            raise AssertionError(
                f"Unexpected observation: {observation.item_id}"
            )

        with patch(
            "surface.prepare_imagery.read_aligned_observation",
            side_effect=fake_read,
        ):
            imagery, coverage, records = compose_imagery(
                observations=(fallback, primary),
                destination_crs=None,
                destination_transform=None,
                width=2,
                height=2,
            )

        np.testing.assert_array_equal(
            imagery[:, 0, 0],
            np.array([10, 20, 30], dtype=np.uint8),
        )

        np.testing.assert_array_equal(
            imagery[:, 0, 1],
            np.array([40, 50, 60], dtype=np.uint8),
        )

        np.testing.assert_array_equal(
            imagery[:, 1, 0],
            np.array([70, 80, 90], dtype=np.uint8),
        )

        np.testing.assert_array_equal(
            imagery[:, 1, 1],
            np.array([100, 110, 120], dtype=np.uint8),
        )

        self.assertTrue(np.all(coverage))

        self.assertEqual(
            records[0]["itemId"],
            "primary",
        )
        self.assertEqual(
            records[0]["contributedPixels"],
            2,
        )

        self.assertEqual(
            records[1]["itemId"],
            "fallback",
        )
        self.assertEqual(
            records[1]["validPixels"],
            3,
        )
        self.assertEqual(
            records[1]["contributedPixels"],
            2,
        )

    def test_repair_enclosed_gap_uses_eight_neighbors(self):
        required_surface = np.ones(
            (3, 3),
            dtype=bool,
        )

        coverage = np.ones(
            (3, 3),
            dtype=bool,
        )
        coverage[1, 1] = False

        imagery = np.zeros(
            (3, 3, 3),
            dtype=np.uint8,
        )

        neighbor_values = [
            (10, 20, 30),
            (20, 30, 40),
            (30, 40, 50),
            (40, 50, 60),
            (60, 70, 80),
            (70, 80, 90),
            (80, 90, 100),
            (90, 100, 110),
        ]

        index = 0

        for row in range(3):
            for col in range(3):
                if row == 1 and col == 1:
                    continue

                imagery[:, row, col] = (
                    neighbor_values[index]
                )
                index += 1

        repairs = repair_enclosed_gaps(
            imagery=imagery,
            coverage=coverage,
            required_surface=required_surface,
        )

        self.assertEqual(
            repairs,
            [{"row": 1, "col": 1}],
        )

        self.assertTrue(coverage[1, 1])

        expected = np.rint(
            np.mean(
                np.array(
                    neighbor_values,
                    dtype=np.float64,
                ),
                axis=0,
            )
        ).astype(np.uint8)

        np.testing.assert_array_equal(
            imagery[:, 1, 1],
            expected,
        )

    def test_repair_enclosed_multi_pixel_gap(self):
        required_surface = np.ones(
            (7, 7),
            dtype=bool,
        )

        coverage = np.ones(
            (7, 7),
            dtype=bool,
        )

        coverage[2:5, 2:5] = False

        imagery = np.full(
            (3, 7, 7),
            120,
            dtype=np.uint8,
        )

        imagery[:, ~coverage] = 0

        repairs = repair_enclosed_gaps(
            imagery=imagery,
            coverage=coverage,
            required_surface=required_surface,
        )

        self.assertEqual(
            len(repairs),
            9,
        )

        self.assertTrue(
            np.all(coverage)
        )

        np.testing.assert_array_equal(
            imagery[:, 2:5, 2:5],
            np.full(
                (3, 3, 3),
                120,
                dtype=np.uint8,
            ),
        )

    def test_repair_rejects_real_coverage_boundary(self):
        required_surface = np.ones(
            (6, 6),
            dtype=bool,
        )

        coverage = np.ones(
            (6, 6),
            dtype=bool,
        )

        coverage[2, 2] = False
        coverage[2, 3] = False

        required_surface[2, 3] = False

        imagery = np.zeros(
            (3, 6, 6),
            dtype=np.uint8,
        )

        with self.assertRaisesRegex(
            RuntimeError,
            "not fully enclosed by covered imagery",
        ):
            repair_enclosed_gaps(
                imagery=imagery,
                coverage=coverage,
                required_surface=required_surface,
            )

    def test_repair_rejects_oversized_component(self):
        required_surface = np.ones(
            (8, 8),
            dtype=bool,
        )

        coverage = np.ones(
            (8, 8),
            dtype=bool,
        )

        coverage[1:7, 1:7] = False

        imagery = np.zeros(
            (3, 8, 8),
            dtype=np.uint8,
        )

        with self.assertRaisesRegex(
            RuntimeError,
            "component exceeds",
        ):
            repair_enclosed_gaps(
                imagery=imagery,
                coverage=coverage,
                required_surface=required_surface,
            )

    def test_repair_rejects_excessive_total_area(self):
        required_surface = np.ones(
            (51, 51),
            dtype=bool,
        )

        coverage = np.ones(
            (51, 51),
            dtype=bool,
        )

        coverage[
            1:50:2,
            1:50:2,
        ] = False

        imagery = np.zeros(
            (3, 51, 51),
            dtype=np.uint8,
        )

        with self.assertRaisesRegex(
            RuntimeError,
            "too many missing required surface pixels",
        ):
            repair_enclosed_gaps(
                imagery=imagery,
                coverage=coverage,
                required_surface=required_surface,
            )


if __name__ == "__main__":
    unittest.main()
