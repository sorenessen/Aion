"""Tests for TMS generation geometry and presentation inputs."""

from __future__ import annotations

import sys
import unittest
from pathlib import Path

import rasterio
from affine import Affine
from rasterio.io import MemoryFile

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.generate_tms import (  # noqa: E402
    select_pyramid_geometry,
    validate_aligned_presentation_input,
    validate_visual_rgb_coverage,
)
from surface.tms import (  # noqa: E402
    choose_maximum_level,
)


SEMANTIC_CRS = (
    '+proj=aea +lat_0=23 +lon_0=-96 '
    '+lat_1=29.5 +lat_2=45.5 '
    '+x_0=0 +y_0=0 +datum=WGS84 '
    '+units=m +no_defs'
)

LEFT = -2069895.0
TOP = 3007665.0
WIDTH_METRES = 171150.0
HEIGHT_METRES = 126900.0


class RasterFixture:
    def __init__(
        self,
        width: int,
        height: int,
        resolution: float,
        count: int = 1,
        dtype: str = "uint8",
    ):
        self.memory_file = MemoryFile()

        self.dataset = self.memory_file.open(
            driver="GTiff",
            width=width,
            height=height,
            count=count,
            dtype=dtype,
            crs=SEMANTIC_CRS,
            transform=Affine(
                resolution,
                0.0,
                LEFT,
                0.0,
                -resolution,
                TOP,
            ),
        )

    def close(self):
        self.dataset.close()
        self.memory_file.close()


class GenerateTmsGeometryTests(unittest.TestCase):
    def setUp(self):
        self.semantic = RasterFixture(
            width=5705,
            height=4230,
            resolution=30.0,
        )

        self.visual = RasterFixture(
            width=17115,
            height=12690,
            resolution=10.0,
            count=3,
        )

    def tearDown(self):
        self.visual.close()
        self.semantic.close()

    def test_visual_grid_can_differ_from_semantic_grid(self):
        (
            bounds,
            semantic_resolution,
            visual_resolution,
            detail_resolution,
            detail_resolution_source,
        ) = select_pyramid_geometry(
            self.semantic.dataset,
            self.visual.dataset,
        )

        self.assertIsNotNone(bounds)

        self.assertGreater(
            semantic_resolution,
            visual_resolution,
        )

        self.assertEqual(
            detail_resolution,
            visual_resolution,
        )

        self.assertEqual(
            detail_resolution_source,
            "visualRgb",
        )

        self.assertEqual(
            choose_maximum_level(detail_resolution),
            13,
        )

    def test_without_visual_rgb_semantic_resolution_drives_detail(self):
        (
            _,
            semantic_resolution,
            visual_resolution,
            detail_resolution,
            detail_resolution_source,
        ) = select_pyramid_geometry(
            self.semantic.dataset,
            None,
        )

        self.assertIsNone(
            visual_resolution
        )

        self.assertEqual(
            detail_resolution,
            semantic_resolution,
        )

        self.assertEqual(
            detail_resolution_source,
            "semanticSource",
        )

        self.assertEqual(
            choose_maximum_level(detail_resolution),
            11,
        )

    def test_exact_alignment_rule_remains_for_slope_inputs(self):
        with self.assertRaisesRegex(
            RuntimeError,
            "not aligned with the Est semantic source grid",
        ):
            validate_aligned_presentation_input(
                self.semantic.dataset,
                self.visual.dataset,
                "Slope",
            )

    def test_visual_coverage_preflight_accepts_required_surface(self):
        import numpy as np

        semantic = RasterFixture(
            width=2,
            height=2,
            resolution=30.0,
        )

        visual = RasterFixture(
            width=6,
            height=6,
            resolution=10.0,
            count=3,
        )

        try:
            semantic.dataset.write(
                np.ones(
                    (2, 2),
                    dtype="uint8",
                ),
                1,
            )

            visual.dataset.write_mask(
                np.full(
                    (6, 6),
                    255,
                    dtype="uint8",
                )
            )

            validate_visual_rgb_coverage(
                semantic.dataset,
                visual.dataset,
            )
        finally:
            visual.close()
            semantic.close()

    def test_visual_coverage_preflight_rejects_missing_surface(self):
        import numpy as np

        semantic = RasterFixture(
            width=2,
            height=2,
            resolution=30.0,
        )

        visual = RasterFixture(
            width=6,
            height=6,
            resolution=10.0,
            count=3,
        )

        try:
            semantic.dataset.write(
                np.ones(
                    (2, 2),
                    dtype="uint8",
                ),
                1,
            )

            mask = np.full(
                (6, 6),
                255,
                dtype="uint8",
            )
            mask[2, 2] = 0
            visual.dataset.write_mask(mask)

            with self.assertRaisesRegex(
                RuntimeError,
                "1 missing",
            ):
                validate_visual_rgb_coverage(
                    semantic.dataset,
                    visual.dataset,
                )
        finally:
            visual.close()
            semantic.close()


if __name__ == "__main__":
    unittest.main()
