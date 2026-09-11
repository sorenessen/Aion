"""Tests for Est imagery presentation-input preparation."""

import sys
import unittest
from pathlib import Path
from unittest.mock import patch

import numpy as np

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.prepare_imagery import (  # noqa: E402
    Observation,
    compose_imagery,
    repair_isolated_pixels,
)


class PrepareImageryTests(unittest.TestCase):
    def test_compose_imagery_preserves_first_valid_observation(self):
        semantic_surface = np.ones(
            (2, 2),
            dtype=bool,
        )

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
                semantic_surface=semantic_surface,
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
            records[1]["validSemanticPixels"],
            3,
        )
        self.assertEqual(
            records[1]["contributedPixels"],
            2,
        )

    def test_repair_isolated_pixel_uses_eight_neighbors(self):
        semantic_surface = np.ones(
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

                imagery[:, row, col] = neighbor_values[index]
                index += 1

        repairs = repair_isolated_pixels(
            imagery=imagery,
            coverage=coverage,
            semantic_surface=semantic_surface,
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

    def test_repair_rejects_non_isolated_missing_pixels(self):
        semantic_surface = np.ones(
            (4, 4),
            dtype=bool,
        )

        coverage = np.ones(
            (4, 4),
            dtype=bool,
        )

        coverage[1, 1] = False
        coverage[1, 2] = False

        imagery = np.zeros(
            (3, 4, 4),
            dtype=np.uint8,
        )

        with self.assertRaisesRegex(
            RuntimeError,
            "not an isolated single-pixel hole",
        ):
            repair_isolated_pixels(
                imagery=imagery,
                coverage=coverage,
                semantic_surface=semantic_surface,
            )


if __name__ == "__main__":
    unittest.main()
