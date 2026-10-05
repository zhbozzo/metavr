"""Real local file operations + mocked process contracts; these are NOT Unity tests."""
import contextlib
import io
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest import mock

from tools.setup_unity_project import (PACKAGE_NAME, SCENE, SetupError, main, prepare_manifest,
                                      read_json, run_stage, setup, validate_receipt, write_manifest)


class ProjectSetupTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.project = self.root / "Project With Spaces"
        self.package = self.root / "Packages" / PACKAGE_NAME
        (self.project / "Packages").mkdir(parents=True)
        self.package.mkdir(parents=True)
        (self.package / "package.json").write_text(json.dumps({"name": PACKAGE_NAME}))
        self.manifest = self.project / "Packages/manifest.json"
        self.document = {"dependencies": {"some.other.package": "2.3.4"}, "scopedRegistries": [{"name": "Keep"}]}
        self.manifest.write_text(json.dumps(self.document))
        self.output = self.root / "logs"; self.output.mkdir()

    def test_relative_package_path_is_from_packages_directory(self):
        result = prepare_manifest(self.document, self.project, self.package)
        self.assertEqual(result["dependencies"][PACKAGE_NAME], "file:../../Packages/" + PACKAGE_NAME)

    def test_preserves_unrelated_fields_and_does_not_mutate_input(self):
        result = prepare_manifest(self.document, self.project, self.package)
        self.assertEqual(result["scopedRegistries"], self.document["scopedRegistries"])
        self.assertEqual(result["dependencies"]["some.other.package"], "2.3.4")
        self.assertNotIn(PACKAGE_NAME, self.document["dependencies"])
        self.assertEqual(result["testables"], [PACKAGE_NAME])

    def test_idempotent_manifest_does_not_rewrite_bytes(self):
        result = prepare_manifest(self.document, self.project, self.package)
        self.assertTrue(write_manifest(self.manifest, self.manifest.read_bytes(), result, self.output))
        before = self.manifest.read_bytes(); stamp = self.manifest.stat().st_mtime_ns
        self.assertFalse(write_manifest(self.manifest, before, prepare_manifest(result, self.project, self.package), self.output))
        self.assertEqual(self.manifest.read_bytes(), before)
        self.assertEqual(self.manifest.stat().st_mtime_ns, stamp)

    def test_conflicting_remote_or_other_local_package_rejected(self):
        for value in ("https://github.com/other/repo.git", "file:../elsewhere", "file://elsewhere"):
            with self.assertRaises(SetupError):
                prepare_manifest({"dependencies": {PACKAGE_NAME: value}}, self.project, self.package)

    def test_embedded_package_shadow_is_rejected(self):
        (self.project / "Packages" / PACKAGE_NAME).mkdir()
        with self.assertRaises(SetupError): prepare_manifest(self.document, self.project, self.package)

    def test_preserves_other_testables_without_duplicating_ours(self):
        doc = {"dependencies": {}, "testables": ["other.tests", PACKAGE_NAME]}
        self.assertEqual(prepare_manifest(doc, self.project, self.package)["testables"], doc["testables"])

    def test_malformed_manifest_fields_are_rejected(self):
        for doc in ([], {}, {"dependencies": []}, {"dependencies": {"x": None}}, {"dependencies": {}, "testables": "wrong"}):
            with self.assertRaises(SetupError): prepare_manifest(doc, self.project, self.package)

    def test_test_framework_requires_an_explicit_exact_pin(self):
        for version in ("latest", "1.*", "../other", "1.2.3-preview.1"):
            with self.assertRaises(SetupError): prepare_manifest(self.document, self.project, self.package, version)
        result = prepare_manifest(self.document, self.project, self.package, "1.4.5")
        self.assertEqual(result["dependencies"]["com.unity.test-framework"], "1.4.5")
        with self.assertRaises(SetupError): prepare_manifest(result, self.project, self.package, "1.4.6")

    def test_no_unrequested_test_framework_install(self):
        self.assertNotIn("com.unity.test-framework", prepare_manifest(self.document, self.project, self.package)["dependencies"])

    def test_duplicate_json_keys_preserve_original(self):
        self.manifest.write_text('{"dependencies":{},"dependencies":{"x":"1"}}')
        before = self.manifest.read_bytes()
        with self.assertRaises(SetupError): read_json(self.manifest)
        self.assertEqual(self.manifest.read_bytes(), before)

    def test_manifest_backup_is_exact(self):
        before = self.manifest.read_bytes()
        write_manifest(self.manifest, before, prepare_manifest(self.document, self.project, self.package), self.output)
        self.assertEqual((self.output / "manifest.before.json").read_bytes(), before)

    def test_changed_manifest_is_not_overwritten(self):
        before = self.manifest.read_bytes(); self.manifest.write_text('{"dependencies":{"new":"1"}}')
        with self.assertRaises(SetupError): write_manifest(self.manifest, before, self.document, self.output)
        self.assertIn("new", self.manifest.read_text())

    def test_symlinked_manifest_is_not_replaced(self):
        actual = self.root / "actual.json"; actual.write_text(self.manifest.read_text()); self.manifest.unlink()
        self.manifest.symlink_to(actual)
        with self.assertRaises(SetupError): write_manifest(self.manifest, actual.read_bytes(), self.document, self.output)
        self.assertTrue(self.manifest.is_symlink())

    def test_zero_exit_without_scene_receipt_cannot_succeed(self):
        with self.assertRaises(SetupError): validate_receipt(self.output / "absent.json", self.project, "6000.0.58f2", "nonce")

    def test_receipt_requires_nonce_version_scene_and_real_metadata(self):
        path = self.output / "receipt.json"
        doc = {"schema": 1, "status": "PREPARED", "unityVersion": "6000.0.58f2", "nonce": "expected",
               "scenePath": SCENE, "rigCount": 1, "cameraCount": 1, "questTested": False}
        path.write_text(json.dumps(doc))
        with self.assertRaises(SetupError): validate_receipt(path, self.project, "6000.0.58f2", "expected")
        scene = self.project / SCENE; scene.parent.mkdir(parents=True); scene.write_text("TEST FIXTURE, NOT UNITY YAML")
        (self.project / (SCENE + ".meta")).write_text("TEST FIXTURE, NOT UNITY METADATA")
        self.assertEqual(validate_receipt(path, self.project, "6000.0.58f2", "expected"), doc)
        for version, nonce in (("6000.0.59f1", "expected"), ("6000.0.58f2", "old")):
            with self.assertRaises(SetupError): validate_receipt(path, self.project, version, nonce)

    def test_process_preserves_spaced_paths_and_uses_no_shell(self):
        with mock.patch("tools.setup_unity_project.subprocess.run", return_value=subprocess.CompletedProcess([], 0)) as launch:
            run_stage(Path("Unity With Spaces"), ["-projectPath", str(self.project)], self.output, "test", 5)
            self.assertEqual(launch.call_args.args[0][0], "Unity With Spaces")
            self.assertIn(str(self.project), launch.call_args.args[0])
            self.assertNotIn("shell", launch.call_args.kwargs)

    def test_nonzero_exit_and_timeout_are_failures(self):
        with mock.patch("tools.setup_unity_project.subprocess.run", return_value=subprocess.CompletedProcess([], 1)):
            with self.assertRaises(SetupError) as caught: run_stage(Path("unused"), [], self.output, "bad", 5)
            self.assertEqual(caught.exception.status, "FAIL")
        with mock.patch("tools.setup_unity_project.subprocess.run", side_effect=subprocess.TimeoutExpired([], 5)):
            with self.assertRaises(SetupError): run_stage(Path("unused"), [], self.output, "timeout", 5)

    def test_missing_editor_reports_blocked_not_prepared(self):
        with mock.patch("tools.setup_unity_project.find_editor", return_value=None), contextlib.redirect_stdout(io.StringIO()) as text:
            self.assertEqual(main(["--project", str(self.project), "--create"]), 2)
        self.assertEqual(json.loads(text.getvalue())["status"], "BLOCKED")

    def test_nonempty_nonproject_never_runs_create_or_deletes(self):
        with mock.patch("tools.setup_unity_project.probe_version", return_value="6000.0.58f2"), mock.patch("tools.setup_unity_project.run_stage") as launch:
            with self.assertRaises(SetupError): setup(self.project, self.package, Path("unused"), True, False, None, self.output, 5)
            launch.assert_not_called()
        self.assertTrue(self.manifest.exists())

    def test_editor_mismatch_never_modifies_manifest(self):
        (self.project / "ProjectSettings").mkdir()
        (self.project / "ProjectSettings/ProjectVersion.txt").write_text("m_EditorVersion: 6000.0.58f2\n")
        before = self.manifest.read_bytes()
        with mock.patch("tools.setup_unity_project.probe_version", return_value="6000.0.59f1"), mock.patch("tools.setup_unity_project.run_stage") as launch:
            with self.assertRaises(SetupError): setup(self.project, self.package, Path("unused"), False, False, None, self.output, 5)
            launch.assert_not_called()
        self.assertEqual(before, self.manifest.read_bytes())

    def test_open_project_never_probes_or_writes(self):
        (self.project / "Temp").mkdir(); (self.project / "Temp/UnityLockfile").touch()
        with mock.patch("tools.setup_unity_project.probe_version") as probe:
            with self.assertRaises(SetupError): setup(self.project, self.package, Path("unused"), False, False, None, self.output, 5)
            probe.assert_not_called()

    def test_source_overlap_is_rejected(self):
        with self.assertRaises(SetupError): setup(self.package, self.package, Path("unused"), True, False, None, self.output, 5)


if __name__ == "__main__":
    unittest.main()
