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
class SurfaceMaterial:
    rgba: tuple[int, int, int, int]
    variation: float


@dataclass(frozen=True)
class SurfacePresentation:
    version: int
    name: str
    materials_by_id: dict[int, SurfaceMaterial]

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
        variation = presentation.get("variation", 0.0)

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

        if (
            not isinstance(variation, (int, float))
            or isinstance(variation, bool)
            or not 0.0 <= float(variation) <= 1.0
        ):
            raise ValueError(
                "Invalid presentation variation for "
                f"{name}: {variation}"
            )

        materials_by_id[int(category["id"])] = SurfaceMaterial(
            rgba=tuple(rgba),
            variation=float(variation),
        )

    unknown_id = int(categories["Unknown"]["id"])
    unknown = materials_by_id[unknown_id]

    if unknown.rgba[3] != 0:
        raise ValueError(
            "Unknown surface presentation must remain transparent."
        )

    if unknown.variation != 0.0:
        raise ValueError(
            "Unknown surface presentation cannot have material variation."
        )

    return SurfacePresentation(
        version=int(presentation_definition["version"]),
        name=str(presentation_definition["name"]),
        materials_by_id=materials_by_id,
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


def _material_noise(
    longitude: np.ndarray,
    latitude: np.ndarray,
) -> np.ndarray:
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

    return (
        broad * 0.50
        + medium * 0.32
        + fine * 0.18
    )


def render_surface_material(
    categories: np.ndarray,
    longitude: np.ndarray,
    latitude: np.ndarray,
    presentation: SurfacePresentation,
) -> np.ndarray:
    if categories.shape != longitude.shape:
        raise ValueError(
            "Category and longitude arrays must have matching shapes."
        )

    if categories.shape != latitude.shape:
        raise ValueError(
            "Category and latitude arrays must have matching shapes."
        )

    noise = _material_noise(
        longitude,
        latitude,
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

        factor = (
            1.0
            + noise[mask, np.newaxis]
            * material.variation
        )

        varied_rgb = np.clip(
            np.rint(base * factor),
            0,
            255,
        ).astype(np.uint8)

        rgba[mask, :3] = varied_rgb
        rgba[mask, 3] = material.rgba[3]

    return rgba
