import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))

from local_scene.prepare_osm_buildings import (  # noqa: E402
    normalized_height,
    prepare_building_feature,
    prepare_feature_collection,
)


class HeightNormalizationTests(unittest.TestCase):
    def test_explicit_height_wins(self):
        height, source = normalized_height(
            {
                "height": "12",
                "building:levels": "9",
            }
        )

        self.assertEqual(12.0, height)
        self.assertEqual("explicit", source)

    def test_metric_height_suffix_is_accepted(self):
        height, source = normalized_height({"height": "18.5 m"})

        self.assertEqual(18.5, height)
        self.assertEqual("explicit", source)

    def test_levels_use_three_metre_fallback(self):
        height, source = normalized_height({"building:levels": "6"})

        self.assertEqual(18.0, height)
        self.assertEqual("levels", source)

    def test_missing_height_uses_default(self):
        height, source = normalized_height({})

        self.assertEqual(9.0, height)
        self.assertEqual("default", source)


class BuildingPreparationTests(unittest.TestCase):
    def test_prepares_renderer_neutral_polygon(self):
        element = {
            "type": "way",
            "id": 31494476,
            "tags": {
                "building": "government",
                "building:levels": "6",
                "name": "Washington State Capitol",
            },
            "geometry": [
                {"lat": 47.0, "lon": -122.0},
                {"lat": 47.0, "lon": -121.9},
                {"lat": 47.1, "lon": -121.9},
                {"lat": 47.0, "lon": -122.0},
            ],
        }

        feature = prepare_building_feature(element)

        self.assertEqual("Feature", feature["type"])
        self.assertEqual("Polygon", feature["geometry"]["type"])
        self.assertEqual(
            [-122.0, 47.0],
            feature["geometry"]["coordinates"][0][0],
        )
        self.assertEqual("osm-way-31494476", feature["properties"]["id"])
        self.assertEqual(
            "Washington State Capitol",
            feature["properties"]["name"],
        )
        self.assertEqual(18.0, feature["properties"]["heightMeters"])
        self.assertEqual("levels", feature["properties"]["heightSource"])
        self.assertEqual(
            {
                "dataset": "OpenStreetMap",
                "elementType": "way",
                "elementId": 31494476,
            },
            feature["properties"]["source"],
        )

    def test_open_way_is_rejected(self):
        element = {
            "type": "way",
            "id": 1,
            "tags": {"building": "yes"},
            "geometry": [
                {"lat": 47.0, "lon": -122.0},
                {"lat": 47.0, "lon": -121.9},
                {"lat": 47.1, "lon": -121.9},
                {"lat": 47.1, "lon": -122.0},
            ],
        }

        with self.assertRaisesRegex(ValueError, "not closed"):
            prepare_building_feature(element)

    def test_collection_skips_relations_for_initial_spike(self):
        way = {
            "type": "way",
            "id": 2,
            "tags": {"building": "yes"},
            "geometry": [
                {"lat": 47.0, "lon": -122.0},
                {"lat": 47.0, "lon": -121.9},
                {"lat": 47.1, "lon": -121.9},
                {"lat": 47.0, "lon": -122.0},
            ],
        }

        relation = {
            "type": "relation",
            "id": 3,
            "tags": {"building": "yes"},
        }

        collection = prepare_feature_collection([relation, way])

        self.assertEqual(1, collection["estLocalSceneVersion"])
        self.assertEqual(1, len(collection["features"]))
        self.assertEqual("osm-way-2", collection["features"][0]["properties"]["id"])


if __name__ == "__main__":
    unittest.main()
