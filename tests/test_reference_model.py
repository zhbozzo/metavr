"""Reference-model tests. Passing these does not validate Unity or Quest."""
import math
import unittest

from prototypes.reference_model import (
    CapturableEntity, ScaleFrame, Vec3, map_between, reflected_direction, yaw_rotate,
)


class ReferenceTests(unittest.TestCase):
    def assertVector(self, actual, expected):
        for a, b in zip((actual.x, actual.y, actual.z), (expected.x, expected.y, expected.z)):
            self.assertAlmostEqual(a, b, places=9)

    def test_identity_frame(self):
        frame = ScaleFrame(Vec3(0, 0, 0))
        self.assertVector(frame.to_world(Vec3(1, 2, 3)), Vec3(1, 2, 3))

    def test_round_trip_translated_rotated_scaled(self):
        point = Vec3(2.3, -1.2, 5.8)
        for angle in (0, math.pi / 2, -0.77, math.pi):
            for scale in (0.05, 0.2, 1, 4):
                with self.subTest(angle=angle, scale=scale):
                    frame = ScaleFrame(Vec3(8, -4, 2), angle, scale)
                    self.assertVector(frame.to_local(frame.to_world(point)), point)

    def test_known_yaw_direction(self):
        self.assertVector(yaw_rotate(Vec3(1, 0, 0), math.pi / 2), Vec3(0, 0, -1))

    def test_source_to_destination(self):
        canonical = Vec3(2, 0.5, 3)
        source = ScaleFrame(Vec3(1, 1, 0), 0.4, 0.1)
        destination = ScaleFrame(Vec3(-2, 0, 1), -0.8, 1)
        result = map_between(source.to_world(canonical), source, destination)
        self.assertVector(result, destination.to_world(canonical))

    def test_miniature_delta_is_amplified(self):
        source = ScaleFrame(Vec3(0, 0, 0), scale=0.1)
        destination = ScaleFrame(Vec3(0, 0, 0))
        self.assertVector(map_between(Vec3(0.02, 0, 0), source, destination), Vec3(0.2, 0, 0))

    def test_invalid_scale_rejected(self):
        for scale in (0, -1, math.nan, math.inf, -math.inf):
            with self.subTest(scale=scale), self.assertRaises(ValueError):
                ScaleFrame(Vec3(0, 0, 0), scale=scale)

    def test_nonfinite_coordinates_rejected(self):
        for value in (math.nan, math.inf, -math.inf):
            with self.subTest(value=value), self.assertRaises(ValueError):
                Vec3(value, 0, 0)

    def test_nonfinite_angle_rejected(self):
        with self.assertRaises(ValueError):
            ScaleFrame(Vec3(0, 0, 0), yaw_radians=math.nan)

    def test_reflection(self):
        expected = Vec3(1, 1, 0).normalized()
        self.assertVector(reflected_direction(Vec3(1, -1, 0), Vec3(0, 1, 0)), expected)

    def test_normal_magnitude_does_not_change_reflection(self):
        a = reflected_direction(Vec3(2, -3, 1), Vec3(0, 1, 0))
        b = reflected_direction(Vec3(2, -3, 1), Vec3(0, 20, 0))
        self.assertVector(a, b)

    def test_zero_normal_rejected(self):
        with self.assertRaises(ValueError):
            reflected_direction(Vec3(1, 0, 0), Vec3(0, 0, 0))

    def test_zero_direction_rejected(self):
        with self.assertRaises(ValueError):
            reflected_direction(Vec3(0, 0, 0), Vec3(0, 1, 0))

    def test_capture_preserves_initial_offset(self):
        entity = CapturableEntity("mote-1", Vec3(2, 1, 0))
        self.assertTrue(entity.begin("left", Vec3(1.5, 1, 0)))
        entity.move("left", Vec3(1.5, 1, 0))
        self.assertVector(entity.position, Vec3(2, 1, 0))
        entity.move("left", Vec3(2.5, 1, 0))
        self.assertVector(entity.position, Vec3(3, 1, 0))

    def test_second_hand_cannot_steal(self):
        entity = CapturableEntity("mote-1", Vec3(0, 0, 0))
        self.assertTrue(entity.begin("left", Vec3(0, 0, 0)))
        self.assertFalse(entity.begin("right", Vec3(0, 0, 0)))
        with self.assertRaises(ValueError):
            entity.move("right", Vec3(5, 0, 0))
        self.assertFalse(entity.release("right", True))
        self.assertEqual(entity.owner, "left")

    def test_invalid_drop_restores_position(self):
        entity = CapturableEntity("mote-1", Vec3(1, 0, 0))
        entity.begin("left", Vec3(1, 0, 0))
        entity.move("left", Vec3(4, 0, 0))
        self.assertFalse(entity.release("left", False))
        self.assertVector(entity.position, Vec3(1, 0, 0))
        self.assertIsNone(entity.owner)
        self.assertFalse(entity.resolved)

    def test_tracking_cancel_is_safe_and_repeatable(self):
        entity = CapturableEntity("mote-1", Vec3(1, 0, 0))
        entity.begin("left", Vec3(1, 0, 0))
        entity.move("left", Vec3(4, 0, 0))
        self.assertTrue(entity.cancel("left"))
        self.assertFalse(entity.cancel("left"))
        self.assertVector(entity.position, Vec3(1, 0, 0))

    def test_valid_resolution_occurs_once(self):
        entity = CapturableEntity("mote-1", Vec3(0, 0, 0))
        entity.begin("left", Vec3(0, 0, 0))
        self.assertTrue(entity.release("left", True))
        self.assertFalse(entity.release("left", True))
        self.assertFalse(entity.begin("left", Vec3(0, 0, 0)))
        self.assertTrue(entity.resolved)

    def test_views_derive_from_one_entity(self):
        entity = CapturableEntity("mote-1", Vec3(1, 2, 3))
        world = ScaleFrame(Vec3(2, 0, 0), 0.6)
        mini = ScaleFrame(Vec3(0, 1, 0), -0.4, 0.1)
        entity.begin("left", Vec3(1, 2, 3))
        entity.move("left", Vec3(3, 2, 1))
        self.assertVector(world.to_local(world.to_world(entity.position)), entity.position)
        self.assertVector(mini.to_local(mini.to_world(entity.position)), entity.position)

    def test_empty_identifiers_rejected(self):
        with self.assertRaises(ValueError):
            CapturableEntity("", Vec3(0, 0, 0))
        entity = CapturableEntity("mote-1", Vec3(0, 0, 0))
        with self.assertRaises(ValueError):
            entity.begin(" ", Vec3(0, 0, 0))

    def test_target_flag_requires_boolean(self):
        entity = CapturableEntity("mote-1", Vec3(0, 0, 0))
        entity.begin("left", Vec3(0, 0, 0))
        with self.assertRaises(TypeError):
            entity.release("left", "false")
        self.assertEqual(entity.owner, "left")


if __name__ == "__main__":
    unittest.main()
