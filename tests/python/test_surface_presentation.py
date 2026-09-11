"""Tests for Est-owned semantic surface presentation."""

import sys
import unittest
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from surface.presentation import (  # noqa: E402
    MaterialVariation,
    SurfaceMaterial,
    SurfacePresentation,
    load_surface_presentation_model,
    render_surface_material,
)


class SurfacePresentationTests(unittest.TestCase):
    def setUp(self):
        self.presentation = load_surface_presentation_model()

    def test_definition_covers_expected_semantic_categories(self):
        self.assertEqual(
            set(self.presentation.materials_by_id),
            set(range(10)),
        )

    def test_unknown_remains_fully_transparent_and_unvaried(self):
        unknown = self.presentation.materials_by_id[0]

        self.assertEqual(unknown.rgba, (0, 0, 0, 0))
        self.assertEqual(unknown.variation, MaterialVariation(0.0, 0.0, 0.0))

        categories = np.zeros((2, 2), dtype=np.uint8)
        longitude = np.array(
            [
                [-123.0, -122.0],
                [-121.0, -120.0],
            ],
            dtype=np.float64,
        )
        latitude = np.array(
            [
                [48.0, 48.0],
                [47.0, 47.0],
            ],
            dtype=np.float64,
        )

        rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            self.presentation,
        )

        self.assertTrue(np.all(rgba == 0))

    def test_rendering_is_deterministic(self):
        categories = np.full(
            (3, 3),
            4,
            dtype=np.uint8,
        )
        longitude = np.array(
            [
                [-122.52, -122.51, -122.50],
                [-122.52, -122.51, -122.50],
                [-122.52, -122.51, -122.50],
            ],
            dtype=np.float64,
        )
        latitude = np.array(
            [
                [47.62, 47.62, 47.62],
                [47.61, 47.61, 47.61],
                [47.60, 47.60, 47.60],
            ],
            dtype=np.float64,
        )

        first = render_surface_material(
            categories,
            longitude,
            latitude,
            self.presentation,
        )
        second = render_surface_material(
            categories,
            longitude,
            latitude,
            self.presentation,
        )

        np.testing.assert_array_equal(first, second)

    def test_same_geographic_coordinate_is_render_call_independent(self):
        category_id = 4
        longitude_value = -122.375
        latitude_value = 47.625

        single = render_surface_material(
            np.array([[category_id]], dtype=np.uint8),
            np.array([[longitude_value]], dtype=np.float64),
            np.array([[latitude_value]], dtype=np.float64),
            self.presentation,
        )

        larger = render_surface_material(
            np.full((3, 3), category_id, dtype=np.uint8),
            np.array(
                [
                    [-122.40, -122.39, -122.38],
                    [-122.38, longitude_value, -122.37],
                    [-122.36, -122.35, -122.34],
                ],
                dtype=np.float64,
            ),
            np.array(
                [
                    [47.64, 47.64, 47.64],
                    [47.63, latitude_value, 47.63],
                    [47.62, 47.62, 47.62],
                ],
                dtype=np.float64,
            ),
            self.presentation,
        )

        np.testing.assert_array_equal(
            single[0, 0],
            larger[1, 1],
        )

    def test_material_variation_does_not_change_alpha(self):
        categories = np.array(
            [
                [0, 1, 2, 3, 4],
                [5, 6, 7, 8, 9],
            ],
            dtype=np.uint8,
        )
        longitude = np.linspace(
            -123.0,
            -122.0,
            categories.size,
            dtype=np.float64,
        ).reshape(categories.shape)
        latitude = np.linspace(
            47.0,
            48.0,
            categories.size,
            dtype=np.float64,
        ).reshape(categories.shape)

        rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            self.presentation,
        )

        self.assertEqual(int(rgba[0, 0, 3]), 0)
        self.assertTrue(
            np.all(rgba[:, :, 3][categories != 0] == 255)
        )

    def test_material_variation_changes_rgb_without_changing_category(self):
        categories = np.full(
            (1, 4),
            4,
            dtype=np.uint8,
        )
        longitude = np.array(
            [[-122.50, -122.49, -122.48, -122.47]],
            dtype=np.float64,
        )
        latitude = np.full(
            (1, 4),
            47.60,
            dtype=np.float64,
        )

        rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            self.presentation,
        )

        unique_rgb = np.unique(
            rgba[:, :, :3].reshape(-1, 3),
            axis=0,
        )

        self.assertGreater(len(unique_rgb), 1)
        self.assertTrue(np.all(rgba[:, :, 3] == 255))

    def test_zero_variation_renders_exact_base_color(self):
        presentation = SurfacePresentation(
            version=1,
            name="test",
            materials_by_id={
                4: SurfaceMaterial(
                    rgba=(10, 20, 30, 255),
                    variation=MaterialVariation(0.0, 0.0, 0.0),
                ),
            },
        )

        categories = np.full(
            (2, 2),
            4,
            dtype=np.uint8,
        )
        longitude = np.array(
            [
                [-123.0, -122.0],
                [-121.0, -120.0],
            ],
            dtype=np.float64,
        )
        latitude = np.array(
            [
                [48.0, 48.0],
                [47.0, 47.0],
            ],
            dtype=np.float64,
        )

        rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            presentation,
        )

        expected = np.empty((2, 2, 4), dtype=np.uint8)
        expected[:, :] = (10, 20, 30, 255)

        np.testing.assert_array_equal(rgba, expected)

    def test_category_boundaries_are_not_blended(self):
        presentation = SurfacePresentation(
            version=1,
            name="test",
            materials_by_id={
                3: SurfaceMaterial(
                    rgba=(200, 100, 50, 255),
                    variation=MaterialVariation(0.0, 0.0, 0.0),
                ),
                4: SurfaceMaterial(
                    rgba=(20, 80, 30, 255),
                    variation=MaterialVariation(0.0, 0.0, 0.0),
                ),
            },
        )

        categories = np.array(
            [[3, 3, 4, 4]],
            dtype=np.uint8,
        )
        longitude = np.array(
            [[-122.03, -122.02, -122.01, -122.00]],
            dtype=np.float64,
        )
        latitude = np.full(
            (1, 4),
            47.0,
            dtype=np.float64,
        )

        rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            presentation,
        )

        self.assertEqual(
            tuple(rgba[0, 1]),
            (200, 100, 50, 255),
        )
        self.assertEqual(
            tuple(rgba[0, 2]),
            (20, 80, 30, 255),
        )

    def test_material_profiles_produce_distinct_spatial_response(self):
        longitude_values = np.linspace(
            -122.52,
            -122.49,
            64,
            dtype=np.float64,
        )
        latitude_values = np.linspace(
            47.59,
            47.62,
            64,
            dtype=np.float64,
        )

        longitude, latitude = np.meshgrid(
            longitude_values,
            latitude_values,
        )

        categories = np.full(
            longitude.shape,
            4,
            dtype=np.uint8,
        )

        broad_presentation = SurfacePresentation(
            version=1,
            name="broad-test",
            materials_by_id={
                4: SurfaceMaterial(
                    rgba=(180, 180, 180, 255),
                    variation=MaterialVariation(
                        broad=0.15,
                        medium=0.0,
                        fine=0.0,
                    ),
                ),
            },
        )

        fine_presentation = SurfacePresentation(
            version=1,
            name="fine-test",
            materials_by_id={
                4: SurfaceMaterial(
                    rgba=(180, 180, 180, 255),
                    variation=MaterialVariation(
                        broad=0.0,
                        medium=0.0,
                        fine=0.15,
                    ),
                ),
            },
        )

        broad_rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            broad_presentation,
        )
        fine_rgba = render_surface_material(
            categories,
            longitude,
            latitude,
            fine_presentation,
        )

        self.assertFalse(
            np.array_equal(
                broad_rgba[:, :, :3],
                fine_rgba[:, :, :3],
            )
        )

        broad_horizontal_change = np.mean(
            np.abs(
                np.diff(
                    broad_rgba[:, :, 0].astype(np.float64),
                    axis=1,
                )
            )
        )
        fine_horizontal_change = np.mean(
            np.abs(
                np.diff(
                    fine_rgba[:, :, 0].astype(np.float64),
                    axis=1,
                )
            )
        )

        self.assertGreater(
            fine_horizontal_change,
            broad_horizontal_change,
        )

    def test_mismatched_longitude_shape_is_rejected(self):
        categories = np.zeros((2, 2), dtype=np.uint8)
        longitude = np.zeros((1, 2), dtype=np.float64)
        latitude = np.zeros((2, 2), dtype=np.float64)

        with self.assertRaisesRegex(
            ValueError,
            "longitude",
        ):
            render_surface_material(
                categories,
                longitude,
                latitude,
                self.presentation,
            )

    def test_mismatched_latitude_shape_is_rejected(self):
        categories = np.zeros((2, 2), dtype=np.uint8)
        longitude = np.zeros((2, 2), dtype=np.float64)
        latitude = np.zeros((1, 2), dtype=np.float64)

        with self.assertRaisesRegex(
            ValueError,
            "latitude",
        ):
            render_surface_material(
                categories,
                longitude,
                latitude,
                self.presentation,
            )


if __name__ == "__main__":
    unittest.main()
