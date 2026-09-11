"""Est-owned surface presentation definitions.

Surface categories describe semantic identity. Presentation definitions
describe how those categories are rendered into presentation assets.
Renderers consume the resulting assets and do not own category semantics.
"""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path

import numpy as np


ROOT = Path(__file__).resolve().parents[2]

CATEGORY_DEFINITION_PATH = (
    ROOT
    / "src"
    / "Est.Web"
    / "src"
    / "surface"
    / "surface-categories.json"
)

PRESENTATION_DEFINITION_PATH = (
    ROOT
    / "src"
    / "Est.Web"
    / "src"
    / "surface"
    / "surface-presentation.json"
)


@dataclass(frozen=True)
class MaterialVariation:
    broad: float
    medium: float
    fine: float


@dataclass(frozen=True)
class SlopeResponse:
    maximum_degrees: float
    darkening: float


@dataclass(frozen=True)
class SurfaceMaterial:
    rgba: tuple[int, int, int, int]
    variation: MaterialVariation


@dataclass(frozen=True)
class SurfacePresentation:
    version: int
    name: str
    materials_by_id: dict[int, SurfaceMaterial]
    slope_response: SlopeResponse | None = None

    @property
    def rgba_by_id(
        self,
    ) -> dict[int, tuple[int, int, int, int]]:
        return {
            category_id: material.rgba
            for category_id, material
            in self.materials_by_id.items()
        }


def load_surface_presentation_model() -> SurfacePresentation:
    category_definition = json.loads(
        CATEGORY_DEFINITION_PATH.read_text()
    )
    presentation_definition = json.loads(
        PRESENTATION_DEFINITION_PATH.read_text()
    )

    categories = category_definition["categories"]
    presentation_categories = presentation_definition["categories"]

    semantic_names = set(categories)
    presentation_names = set(presentation_categories)

    if presentation_names != semantic_names:
        missing = sorted(semantic_names - presentation_names)
        unexpected = sorted(presentation_names - semantic_names)

        raise ValueError(
            "Surface presentation categories must exactly match "
            f"semantic categories; missing={missing}, "
            f"unexpected={unexpected}"
        )

    materials_by_id: dict[int, SurfaceMaterial] = {}

    for name, category in categories.items():
        presentation = presentation_categories[name]
        rgba = presentation["rgba"]
        variation = presentation.get("variation", {})

        if (
            not isinstance(rgba, list)
            or len(rgba) != 4
            or any(
                not isinstance(channel, int)
                or not 0 <= channel <= 255
                for channel in rgba
            )
        ):
            raise ValueError(
                f"Invalid RGBA presentation value for {name}: {rgba}"
            )

        if not isinstance(variation, dict):
            raise ValueError(
                "Invalid presentation variation for "
                f"{name}: {variation}"
            )

        expected_variation_keys = {"broad", "medium", "fine"}

        if set(variation) != expected_variation_keys:
            raise ValueError(
                "Presentation variation for "
                f"{name} must define exactly "
                "broad, medium, and fine."
            )

        variation_values: dict[str, float] = {}

        for key in ("broad", "medium", "fine"):
            value = variation[key]

            if (
                not isinstance(value, (int, float))
                or isinstance(value, bool)
                or not 0.0 <= float(value) <= 1.0
            ):
                raise ValueError(
                    "Invalid presentation variation "
                    f"{key} for {name}: {value}"
                )

            variation_values[key] = float(value)

        materials_by_id[int(category["id"])] = SurfaceMaterial(
            rgba=tuple(rgba),
            variation=MaterialVariation(**variation_values),
        )

    slope_response_definition = presentation_definition.get(
        "slopeResponse"
    )

    slope_response = None

    if slope_response_definition is not None:
        if (
            not isinstance(slope_response_definition, dict)
            or set(slope_response_definition)
            != {"maximumDegrees", "darkening"}
        ):
            raise ValueError(
                "slopeResponse must define exactly "
                "maximumDegrees and darkening."
            )

        maximum_degrees = slope_response_definition[
            "maximumDegrees"
        ]
        darkening = slope_response_definition["darkening"]

        if (
            not isinstance(maximum_degrees, (int, float))
            or isinstance(maximum_degrees, bool)
            or float(maximum_degrees) <= 0.0
        ):
            raise ValueError(
                "slopeResponse maximumDegrees must be positive."
            )

        if (
            not isinstance(darkening, (int, float))
            or isinstance(darkening, bool)
            or not 0.0 <= float(darkening) <= 1.0
        ):
            raise ValueError(
                "slopeResponse darkening must be between 0 and 1."
            )

        slope_response = SlopeResponse(
            maximum_degrees=float(maximum_degrees),
            darkening=float(darkening),
        )

    unknown_id = int(categories["Unknown"]["id"])
    unknown = materials_by_id[unknown_id]

    if unknown.rgba[3] != 0:
        raise ValueError(
            "Unknown surface presentation must remain transparent."
        )

    if unknown.variation != MaterialVariation(0.0, 0.0, 0.0):
        raise ValueError(
            "Unknown surface presentation cannot have material variation."
        )

    return SurfacePresentation(
        version=int(presentation_definition["version"]),
        name=str(presentation_definition["name"]),
        materials_by_id=materials_by_id,
        slope_response=slope_response,
    )


def load_surface_presentation() -> tuple[
    int,
    str,
    dict[int, tuple[int, int, int, int]],
]:
    presentation = load_surface_presentation_model()

    return (
        presentation.version,
        presentation.name,
        presentation.rgba_by_id,
    )


def _smoothstep(value: np.ndarray) -> np.ndarray:
    return value * value * (3.0 - 2.0 * value)


def _hash_lattice(
    x: np.ndarray,
    y: np.ndarray,
) -> np.ndarray:
    value = np.sin(
        x * 12.9898
        + y * 78.233
        + 37.719
    ) * 43758.5453

    fraction = value - np.floor(value)

    return fraction * 2.0 - 1.0


def _value_noise(
    longitude: np.ndarray,
    latitude: np.ndarray,
    scale_degrees: float,
) -> np.ndarray:
    x = longitude / scale_degrees
    y = latitude / scale_degrees

    x0 = np.floor(x)
    y0 = np.floor(y)
    x1 = x0 + 1.0
    y1 = y0 + 1.0

    tx = _smoothstep(x - x0)
    ty = _smoothstep(y - y0)

    n00 = _hash_lattice(x0, y0)
    n10 = _hash_lattice(x1, y0)
    n01 = _hash_lattice(x0, y1)
    n11 = _hash_lattice(x1, y1)

    nx0 = n00 + (n10 - n00) * tx
    nx1 = n01 + (n11 - n01) * tx

    return nx0 + (nx1 - nx0) * ty


def _material_noise_components(
    longitude: np.ndarray,
    latitude: np.ndarray,
) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    broad = _value_noise(
        longitude,
        latitude,
        0.018,
    )
    medium = _value_noise(
        longitude,
        latitude,
        0.006,
    )
    fine = _value_noise(
        longitude,
        latitude,
        0.002,
    )

    return broad, medium, fine


def render_surface_material(
    categories: np.ndarray,
    longitude: np.ndarray,
    latitude: np.ndarray,
    presentation: SurfacePresentation,
    slope_degrees: np.ndarray | None = None,
) -> np.ndarray:
    if categories.shape != longitude.shape:
        raise ValueError(
            "Category and longitude arrays must have matching shapes."
        )

    if categories.shape != latitude.shape:
        raise ValueError(
            "Category and latitude arrays must have matching shapes."
        )

    if slope_degrees is not None:
        if categories.shape != slope_degrees.shape:
            raise ValueError(
                "Category and slope arrays must have matching shapes."
            )

        if not np.all(np.isfinite(slope_degrees)):
            raise ValueError(
                "Slope values must be finite."
            )

        if np.any(slope_degrees < 0.0):
            raise ValueError(
                "Slope values cannot be negative."
            )

    broad_noise, medium_noise, fine_noise = (
        _material_noise_components(
            longitude,
            latitude,
        )
    )

    rgba = np.zeros(
        (*categories.shape, 4),
        dtype=np.uint8,
    )

    for category_id, material in (
        presentation.materials_by_id.items()
    ):
        mask = categories == category_id

        if not np.any(mask):
            continue

        base = np.asarray(
            material.rgba[:3],
            dtype=np.float64,
        )

        tone = (
            broad_noise[mask] * material.variation.broad
            + medium_noise[mask] * material.variation.medium
            + fine_noise[mask] * material.variation.fine
        )

        factor = 1.0 + tone[:, np.newaxis]

        if (
            slope_degrees is not None
            and presentation.slope_response is not None
        ):
            response = presentation.slope_response

            normalized_slope = np.clip(
                slope_degrees[mask]
                / response.maximum_degrees,
                0.0,
                1.0,
            )

            slope_factor = (
                1.0
                - normalized_slope
                * response.darkening
            )

            factor = (
                factor
                * slope_factor[:, np.newaxis]
            )

        varied_rgb = np.clip(
            np.rint(base * factor),
            0,
            255,
        ).astype(np.uint8)

        rgba[mask, :3] = varied_rgb
        rgba[mask, 3] = material.rgba[3]

    return rgba
