#!/usr/bin/env python3

"""Prepare OSM building ways as renderer-neutral GeoJSON features."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any, Iterable

DEFAULT_LEVEL_HEIGHT_METRES = 3.0
DEFAULT_BUILDING_HEIGHT_METRES = 9.0


def parse_positive_float(value: str | None) -> float | None:
    if value is None:
        return None

    text = value.strip().lower()

    if text.endswith("m"):
        text = text[:-1].strip()

    try:
        parsed = float(text)
    except ValueError:
        return None

    return parsed if parsed > 0 else None


def normalized_height(
    tags: dict[str, str],
    *,
    level_height_metres: float = DEFAULT_LEVEL_HEIGHT_METRES,
    default_height_metres: float = DEFAULT_BUILDING_HEIGHT_METRES,
) -> tuple[float, str]:
    explicit_height = parse_positive_float(tags.get("height"))

    if explicit_height is not None:
        return explicit_height, "explicit"

    levels = parse_positive_float(tags.get("building:levels"))

    if levels is not None:
        return levels * level_height_metres, "levels"

    return default_height_metres, "default"


def way_coordinates(element: dict[str, Any]) -> list[list[float]]:
    geometry = element.get("geometry")

    if not isinstance(geometry, list) or len(geometry) < 4:
        raise ValueError(
            f"OSM way {element.get('id')} does not contain a usable polygon geometry."
        )

    coordinates: list[list[float]] = []

    for point in geometry:
        if not isinstance(point, dict):
            raise ValueError(
                f"OSM way {element.get('id')} contains an invalid geometry point."
            )

        lat = point.get("lat")
        lon = point.get("lon")

        if not isinstance(lat, (int, float)) or not isinstance(lon, (int, float)):
            raise ValueError(
                f"OSM way {element.get('id')} contains an invalid coordinate."
            )

        coordinates.append([float(lon), float(lat)])

    if coordinates[0] != coordinates[-1]:
        raise ValueError(f"OSM way {element.get('id')} is not closed.")

    if len({tuple(point) for point in coordinates[:-1]}) < 3:
        raise ValueError(
            f"OSM way {element.get('id')} does not contain three distinct vertices."
        )

    return coordinates


def prepare_building_feature(
    element: dict[str, Any],
    *,
    level_height_metres: float = DEFAULT_LEVEL_HEIGHT_METRES,
    default_height_metres: float = DEFAULT_BUILDING_HEIGHT_METRES,
) -> dict[str, Any]:
    if element.get("type") != "way":
        raise ValueError("Only OSM building ways are supported by this evaluation spike.")

    osm_id = element.get("id")

    if not isinstance(osm_id, int):
        raise ValueError("OSM building way is missing an integer id.")

    tags = element.get("tags")

    if not isinstance(tags, dict) or "building" not in tags:
        raise ValueError(f"OSM way {osm_id} is not tagged as a building.")

    height_metres, height_source = normalized_height(
        tags,
        level_height_metres=level_height_metres,
        default_height_metres=default_height_metres,
    )

    properties: dict[str, Any] = {
        "id": f"osm-way-{osm_id}",
        "heightMeters": height_metres,
        "heightSource": height_source,
        "source": {
            "dataset": "OpenStreetMap",
            "elementType": "way",
            "elementId": osm_id,
        },
    }

    name = tags.get("name")

    if isinstance(name, str) and name.strip():
        properties["name"] = name.strip()

    return {
        "type": "Feature",
        "properties": properties,
        "geometry": {
            "type": "Polygon",
            "coordinates": [way_coordinates(element)],
        },
    }


def prepare_feature_collection(
    elements: Iterable[dict[str, Any]],
    *,
    level_height_metres: float = DEFAULT_LEVEL_HEIGHT_METRES,
    default_height_metres: float = DEFAULT_BUILDING_HEIGHT_METRES,
) -> dict[str, Any]:
    features = []

    for element in elements:
        if element.get("type") != "way":
            continue

        tags = element.get("tags")

        if not isinstance(tags, dict) or "building" not in tags:
            continue

        features.append(
            prepare_building_feature(
                element,
                level_height_metres=level_height_metres,
                default_height_metres=default_height_metres,
            )
        )

    features.sort(key=lambda feature: feature["properties"]["id"])

    return {
        "type": "FeatureCollection",
        "estLocalSceneVersion": 1,
        "features": features,
    }


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Prepare OSM building ways as an Est local-scene GeoJSON asset."
    )
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    parser.add_argument(
        "--level-height-metres",
        type=float,
        default=DEFAULT_LEVEL_HEIGHT_METRES,
    )
    parser.add_argument(
        "--default-height-metres",
        type=float,
        default=DEFAULT_BUILDING_HEIGHT_METRES,
    )
    args = parser.parse_args()

    if args.level_height_metres <= 0:
        parser.error("--level-height-metres must be positive.")

    if args.default_height_metres <= 0:
        parser.error("--default-height-metres must be positive.")

    source = json.loads(args.source.read_text())

    elements = source.get("elements")

    if not isinstance(elements, list):
        raise ValueError("OSM source does not contain an elements array.")

    collection = prepare_feature_collection(
        elements,
        level_height_metres=args.level_height_metres,
        default_height_metres=args.default_height_metres,
    )

    args.destination.parent.mkdir(parents=True, exist_ok=True)
    args.destination.write_text(json.dumps(collection, indent=2) + "\n")

    print(f"Prepared {len(collection['features'])} building features.")
    print(f"Wrote {args.destination}")


if __name__ == "__main__":
    main()
