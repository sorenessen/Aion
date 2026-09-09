#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
import rasterio
from affine import Affine
from rasterio.enums import Resampling
from rasterio.warp import calculate_default_transform, reproject


ROOT = Path(__file__).resolve().parents[1]
DEFINITION_PATH = (
    ROOT
    / "src"
    / "Est.Web"
    / "src"
    / "surface"
    / "surface-categories.json"
)


def load_definition() -> dict:
    return json.loads(DEFINITION_PATH.read_text())


def build_model(definition: dict):
    categories = definition["categories"]
    source_classes = definition["sources"]["nlcd"]["classes"]

    category_ids = {
        name: int(category["id"])
        for name, category in categories.items()
    }

    category_values = {
        int(category["id"]): category["value"]
        for category in categories.values()
    }

    preview_rgba = {
        int(category["id"]): tuple(category["previewRgba"])
        for category in categories.values()
    }

    nlcd_to_category_name = {
        int(source_value): source_class["category"]
        for source_value, source_class in source_classes.items()
    }

    nlcd_labels = {
        int(source_value): source_class["label"]
        for source_value, source_class in source_classes.items()
    }

    lookup = np.zeros(256, dtype=np.uint8)

    for source_value, category_name in nlcd_to_category_name.items():
        lookup[source_value] = category_ids[category_name]

    return (
        categories,
        category_ids,
        category_values,
        preview_rgba,
        nlcd_to_category_name,
        nlcd_labels,
        lookup,
    )


def verify_source_values(
    data: np.ndarray,
    valid_source_values: set[int],
    nodata: int | None,
) -> list[int]:
    present = {int(value) for value in np.unique(data)}
    expected = set(valid_source_values)

    if nodata is not None:
        expected.add(nodata)

    return sorted(present - expected)


def category_counts(
    data: np.ndarray,
    category_values: dict[int, str],
) -> dict[str, int]:
    result = {
        value: 0
        for value in category_values.values()
    }

    values, counts = np.unique(data, return_counts=True)

    for value, count in zip(values.tolist(), counts.tolist()):
        result[category_values[int(value)]] = int(count)

    return result


def build_rgba(
    mapped: np.ndarray,
    preview_rgba: dict[int, tuple[int, int, int, int]],
) -> np.ndarray:
    rgba_lookup = np.zeros((256, 4), dtype=np.uint8)

    for category_id, rgba in preview_rgba.items():
        rgba_lookup[category_id] = rgba

    return rgba_lookup[mapped]


def write_category_raster(
    source: rasterio.io.DatasetReader,
    mapped: np.ndarray,
    output_path: Path,
    unknown_id: int,
    definition_version: int,
) -> None:
    profile = source.profile.copy()
    profile.update(
        driver="GTiff",
        dtype="uint8",
        count=1,
        nodata=unknown_id,
        compress="deflate",
        predictor=1,
    )

    output_path.parent.mkdir(parents=True, exist_ok=True)

    with rasterio.open(output_path, "w", **profile) as destination:
        destination.write(mapped, 1)
        destination.update_tags(
            EST_SURFACE_CATEGORY_VERSION=str(definition_version),
            SOURCE_PRODUCT="USGS Annual NLCD Land Cover 2025 Collection 1.2",
        )


def write_native_preview(
    source: rasterio.io.DatasetReader,
    lookup: np.ndarray,
    nodata: int | None,
    unknown_id: int,
    preview_rgba: dict[int, tuple[int, int, int, int]],
    output_path: Path,
    maximum_dimension: int,
) -> tuple[int, int]:
    scale = max(
        source.width / maximum_dimension,
        source.height / maximum_dimension,
        1.0,
    )

    width = max(1, round(source.width / scale))
    height = max(1, round(source.height / scale))

    source_preview = source.read(
        1,
        out_shape=(height, width),
        resampling=Resampling.nearest,
    )

    mapped = lookup[source_preview]

    if nodata is not None:
        mapped[source_preview == nodata] = unknown_id

    rgba = build_rgba(mapped, preview_rgba)

    output_path.parent.mkdir(parents=True, exist_ok=True)

    with rasterio.open(
        output_path,
        "w",
        driver="PNG",
        width=width,
        height=height,
        count=4,
        dtype="uint8",
        transform=source.transform * Affine.scale(
            source.width / width,
            source.height / height,
        ),
        crs=source.crs,
    ) as destination:
        for band in range(4):
            destination.write(rgba[:, :, band], band + 1)

    return width, height


def write_geographic_preview(
    source: rasterio.io.DatasetReader,
    lookup: np.ndarray,
    nodata: int | None,
    unknown_id: int,
    preview_rgba: dict[int, tuple[int, int, int, int]],
    output_path: Path,
    maximum_dimension: int,
):
    target_crs = "EPSG:4326"

    transform, width, height = calculate_default_transform(
        source.crs,
        target_crs,
        source.width,
        source.height,
        *source.bounds,
    )

    scale = max(
        width / maximum_dimension,
        height / maximum_dimension,
        1.0,
    )

    target_width = max(1, round(width / scale))
    target_height = max(1, round(height / scale))

    transform = transform * Affine.scale(
        width / target_width,
        height / target_height,
    )

    destination_source = np.full(
        (target_height, target_width),
        250 if nodata is None else nodata,
        dtype=np.uint8,
    )

    reproject(
        source=rasterio.band(source, 1),
        destination=destination_source,
        src_transform=source.transform,
        src_crs=source.crs,
        src_nodata=nodata,
        dst_transform=transform,
        dst_crs=target_crs,
        dst_nodata=nodata,
        resampling=Resampling.nearest,
    )

    mapped = lookup[destination_source]

    if nodata is not None:
        mapped[destination_source == nodata] = unknown_id

    rgba = build_rgba(mapped, preview_rgba)

    output_path.parent.mkdir(parents=True, exist_ok=True)

    with rasterio.open(
        output_path,
        "w",
        driver="PNG",
        width=target_width,
        height=target_height,
        count=4,
        dtype="uint8",
        transform=transform,
        crs=target_crs,
    ) as destination:
        for band in range(4):
            destination.write(rgba[:, :, band], band + 1)

        bounds = destination.bounds

    return target_width, target_height, transform, bounds


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Convert an Annual NLCD categorical raster into "
            "Est-owned surface categories."
        )
    )
    parser.add_argument("input", type=Path)
    parser.add_argument(
        "--output-directory",
        type=Path,
        default=Path("data/generated/nlcd"),
    )
    parser.add_argument(
        "--preview-max-dimension",
        type=int,
        default=1800,
    )

    args = parser.parse_args()

    definition = load_definition()

    (
        categories,
        category_ids,
        category_values,
        preview_rgba,
        nlcd_to_category_name,
        nlcd_labels,
        lookup,
    ) = build_model(definition)

    unknown_id = category_ids["Unknown"]

    output_directory = args.output_directory
    category_path = output_directory / "surface-categories.tif"
    native_preview_path = output_directory / "surface-preview-native.png"
    geographic_preview_path = (
        output_directory / "surface-preview-geographic.png"
    )
    manifest_path = output_directory / "surface-manifest.json"

    with rasterio.open(args.input) as source:
        if source.crs is None:
            raise SystemExit("Source NLCD raster has no CRS.")

        source_data = source.read(1)
        nodata = None if source.nodata is None else int(source.nodata)

        unexpected = verify_source_values(
            source_data,
            set(nlcd_to_category_name),
            nodata,
        )

        if unexpected:
            raise SystemExit(
                f"Unexpected NLCD source values encountered: {unexpected}"
            )

        mapped = lookup[source_data]

        if nodata is not None:
            mapped[source_data == nodata] = unknown_id

        write_category_raster(
            source,
            mapped,
            category_path,
            unknown_id,
            int(definition["version"]),
        )

        native_width, native_height = write_native_preview(
            source,
            lookup,
            nodata,
            unknown_id,
            preview_rgba,
            native_preview_path,
            args.preview_max_dimension,
        )

        (
            geographic_width,
            geographic_height,
            geographic_transform,
            geographic_bounds,
        ) = write_geographic_preview(
            source,
            lookup,
            nodata,
            unknown_id,
            preview_rgba,
            geographic_preview_path,
            args.preview_max_dimension,
        )

        manifest = {
            "surfaceCategoryDefinitionVersion": definition["version"],
            "source": {
                "path": str(args.input),
                "driver": source.driver,
                "width": source.width,
                "height": source.height,
                "dtype": source.dtypes[0],
                "nodata": source.nodata,
                "crs": source.crs.to_wkt(),
                "transform": list(source.transform),
                "bounds": {
                    "left": source.bounds.left,
                    "bottom": source.bounds.bottom,
                    "right": source.bounds.right,
                    "top": source.bounds.top
                },
                "resolution": {
                    "x": source.res[0],
                    "y": source.res[1]
                }
            },
            "mapping": {
                str(source_value): {
                    "sourceLabel": nlcd_labels[source_value],
                    "surfaceCategory": (
                        categories[
                            nlcd_to_category_name[source_value]
                        ]["value"]
                    ),
                    "surfaceCategoryId": (
                        category_ids[
                            nlcd_to_category_name[source_value]
                        ]
                    )
                }
                for source_value in sorted(nlcd_to_category_name)
            },
            "surfaceCategories": {
                category["value"]: category["id"]
                for category in categories.values()
            },
            "categoryCounts": category_counts(
                mapped,
                category_values,
            ),
            "nativePreview": {
                "path": str(native_preview_path),
                "width": native_width,
                "height": native_height,
                "note": (
                    "Native Albers diagnostic preview using Est "
                    "surface-category colors."
                )
            },
            "geographicPreview": {
                "path": str(geographic_preview_path),
                "crs": "EPSG:4326",
                "width": geographic_width,
                "height": geographic_height,
                "transform": list(geographic_transform),
                "bounds": {
                    "west": geographic_bounds.left,
                    "south": geographic_bounds.bottom,
                    "east": geographic_bounds.right,
                    "north": geographic_bounds.top
                },
                "note": (
                    "North-up geographic diagnostic preview. "
                    "Nearest-neighbor reprojection preserves categorical "
                    "boundaries. Palette is not final Est art direction."
                )
            }
        }

    output_directory.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(
        json.dumps(manifest, indent=2) + "\n"
    )

    print("Converted NLCD source raster using shared Est surface semantics.")
    print(f"Categories:         {category_path}")
    print(f"Native preview:     {native_preview_path}")
    print(f"Geographic preview: {geographic_preview_path}")
    print(f"Manifest:           {manifest_path}")

    print("\n=== EST CATEGORY COUNTS ===")
    for category, count in manifest["categoryCounts"].items():
        print(f"{category:>12}: {count:>10,}")

    bounds = manifest["geographicPreview"]["bounds"]

    print("\n=== GEOGRAPHIC BOUNDS ===")
    print(f"west:  {bounds['west']:.8f}")
    print(f"south: {bounds['south']:.8f}")
    print(f"east:  {bounds['east']:.8f}")
    print(f"north: {bounds['north']:.8f}")


if __name__ == "__main__":
    main()
