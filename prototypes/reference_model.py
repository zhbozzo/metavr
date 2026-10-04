"""Mathematical reference only: not Unity, WebXR or a playable game.

This small model uses yaw rotations, not full 6DoF quaternions. It checks
coordinate/ownership invariants independently of a headset or Meta SDK.
"""
from __future__ import annotations

from dataclasses import dataclass, field
from math import cos, isfinite, sin, sqrt


@dataclass(frozen=True)
class Vec3:
    x: float
    y: float
    z: float

    def __post_init__(self) -> None:
        if not all(isfinite(v) for v in (self.x, self.y, self.z)):
            raise ValueError("Coordinates must be finite")

    def __add__(self, other: Vec3) -> Vec3:
        return Vec3(self.x + other.x, self.y + other.y, self.z + other.z)

    def __sub__(self, other: Vec3) -> Vec3:
        return Vec3(self.x - other.x, self.y - other.y, self.z - other.z)

    def scaled(self, amount: float) -> Vec3:
        if not isfinite(amount):
            raise ValueError("Multiplier must be finite")
        return Vec3(self.x * amount, self.y * amount, self.z * amount)

    def dot(self, other: Vec3) -> float:
        return self.x * other.x + self.y * other.y + self.z * other.z

    def normalized(self) -> Vec3:
        length = sqrt(self.dot(self))
        if not isfinite(length) or length <= 1e-12:
            raise ValueError("Cannot normalize a zero or unbounded vector")
        return self.scaled(1.0 / length)


def yaw_rotate(point: Vec3, radians: float) -> Vec3:
    """Rotate around +Y using the documented X/Z sign convention."""
    if not isfinite(radians):
        raise ValueError("Angle must be finite")
    c, s = cos(radians), sin(radians)
    return Vec3(c * point.x + s * point.z, point.y, -s * point.x + c * point.z)


@dataclass(frozen=True)
class ScaleFrame:
    origin: Vec3
    yaw_radians: float = 0.0
    scale: float = 1.0

    def __post_init__(self) -> None:
        if not isinstance(self.origin, Vec3):
            raise TypeError("origin must be Vec3")
        if not isfinite(self.scale) or self.scale <= 0:
            raise ValueError("Scale must be positive and finite")
        if not isfinite(self.yaw_radians):
            raise ValueError("Angle must be finite")

    def to_world(self, canonical: Vec3) -> Vec3:
        return self.origin + yaw_rotate(canonical.scaled(self.scale), self.yaw_radians)

    def to_local(self, world: Vec3) -> Vec3:
        return yaw_rotate(world - self.origin, -self.yaw_radians).scaled(1.0 / self.scale)


def map_between(point: Vec3, source: ScaleFrame, destination: ScaleFrame) -> Vec3:
    """Interpret point in source's world space; map through canonical space."""
    return destination.to_world(source.to_local(point))


def reflected_direction(direction: Vec3, normal: Vec3) -> Vec3:
    """Return a unit reflected direction. Speed belongs to separate state."""
    d, n = direction.normalized(), normal.normalized()
    return (d - n.scaled(2.0 * d.dot(n))).normalized()


@dataclass
class CapturableEntity:
    """Single-entity ownership reference; not a full game/input controller.

    The caller supplies canonical hand positions. An invalid drop cancels;
    a valid drop resolves once. Global one-entity-per-hand policy belongs
    to the future simulation/controller, not this isolated object.
    """
    entity_id: str
    position: Vec3
    owner: str | None = field(default=None, init=False)
    resolved: bool = field(default=False, init=False)
    _start: Vec3 | None = field(default=None, init=False, repr=False)
    _offset: Vec3 | None = field(default=None, init=False, repr=False)

    def __post_init__(self) -> None:
        if not isinstance(self.entity_id, str) or not self.entity_id.strip():
            raise ValueError("entity_id must be nonempty")
        if not isinstance(self.position, Vec3):
            raise TypeError("position must be Vec3")

    def begin(self, hand_id: str, hand_position: Vec3) -> bool:
        if not isinstance(hand_id, str) or not hand_id.strip():
            raise ValueError("hand_id must be nonempty")
        if not isinstance(hand_position, Vec3):
            raise TypeError("hand_position must be Vec3")
        if self.resolved or self.owner is not None:
            return False
        self.owner = hand_id
        self._start = self.position
        self._offset = self.position - hand_position
        return True

    def move(self, hand_id: str, hand_position: Vec3) -> None:
        if self.owner != hand_id or self.owner is None or self._offset is None:
            raise ValueError("Only the active owner can move this entity")
        self.position = hand_position + self._offset

    def cancel(self, hand_id: str) -> bool:
        if self.owner != hand_id or self.owner is None:
            return False
        if self._start is not None:
            self.position = self._start
        self._clear_capture()
        return True

    def release(self, hand_id: str, valid_target: bool) -> bool:
        if not isinstance(valid_target, bool):
            raise TypeError("valid_target must be bool")
        if self.owner != hand_id or self.owner is None or self.resolved:
            return False
        if not valid_target:
            self.cancel(hand_id)
            return False
        self.resolved = True
        self._clear_capture()
        return True

    def _clear_capture(self) -> None:
        self.owner = None
        self._start = None
        self._offset = None
