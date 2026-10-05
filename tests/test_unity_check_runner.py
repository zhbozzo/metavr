"""Synthetic report/runner tests. They do not execute or impersonate Unity."""
import contextlib
import io
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest import mock
import xml.etree.ElementTree as ET

from tools.run_unity_checks import (command, editor_version, execute, inspect_results, main, PREFIX,
                                    INPUT_PREFIX, INPUT_CASES, PLAY_PREFIX, PLAY_CASES, probe_version)


class UnityCheckRunnerTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        self.xml = self.root / "results.xml"

    def fixture(self, result="Passed", count=3, hand=True, inputs=True):
        root = ET.Element("test-run", result=result)
        for i in range(count):
            ET.SubElement(root, "test-case", fullname=PREFIX + f"NumericFrameMatchesActualUnityTransform({i})", result="Passed")
        if hand:
            ET.SubElement(root, "test-case", fullname=PREFIX + "EnlargedHandPreservesRoomPoseAcrossTwoRotatedFrames", result="Passed")
        if inputs:
            for name in INPUT_CASES:
                ET.SubElement(root, "test-case", fullname=INPUT_PREFIX + name, result="Passed")
        ET.ElementTree(root).write(self.xml, encoding="utf-8")
        return root

    def test_valid_complete_report(self):
        self.fixture()
        self.assertEqual(inspect_results(self.xml).status, "PASS")
        self.assertEqual(inspect_results(self.xml).cases, 12)

    def test_missing_report_fails(self):
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_malformed_report_fails(self):
        self.xml.write_text("<broken")
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_zero_tests_fail(self):
        self.fixture(count=0, hand=False, inputs=False)
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_failed_root_fails(self):
        self.fixture(result="Failed")
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_skipped_case_fails(self):
        root = self.fixture(); root[0].set("result", "Skipped")
        ET.ElementTree(root).write(self.xml)
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_missing_case_fails(self):
        self.fixture(count=2)
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_wrong_suite_does_not_pass(self):
        self.xml.write_text('<test-run result="Passed"><test-case fullname="Other.Test" result="Passed"/></test-run>')
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_duplicates_do_not_pass(self):
        root = self.fixture(); root[1].set("fullname", root[0].get("fullname"))
        ET.ElementTree(root).write(self.xml)
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_dtd_rejected(self):
        self.xml.write_text('<!DOCTYPE test-run><test-run result="Passed"/>')
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_exit_zero_without_results_fails(self):
        with mock.patch("tools.run_unity_checks.subprocess.run", return_value=subprocess.CompletedProcess([], 0)):
            self.assertEqual(execute(Path("unused"), self.root, self.root, 5).status, "FAIL")

    def test_exit_failure_even_with_results_fails(self):
        self.fixture()
        with mock.patch("tools.run_unity_checks.subprocess.run", return_value=subprocess.CompletedProcess([], 1)):
            self.assertEqual(execute(Path("unused"), self.root, self.root, 5).status, "FAIL")

    def test_timeout_fails(self):
        with mock.patch("tools.run_unity_checks.subprocess.run", side_effect=subprocess.TimeoutExpired([], 5)):
            self.assertEqual(execute(Path("unused"), self.root, self.root, 5).status, "FAIL")

    def test_command_has_no_premature_quit(self):
        cmd = command(Path("path with spaces/Unity"), self.root, self.root)
        self.assertNotIn("-quit", cmd)
        self.assertIn("-assemblyNames", cmd)
        self.assertEqual(cmd[0], "path with spaces/Unity")

    def test_project_absent_is_blocked(self):
        with contextlib.redirect_stdout(io.StringIO()) as capture:
            self.assertEqual(main(["--project", str(self.root)]), 2)
        self.assertIn('"status": "BLOCKED"', capture.getvalue())
        self.assertIn('"quest_tested": false', capture.getvalue())

    def test_version_is_read_not_invented(self):
        self.assertIsNone(editor_version(self.root))
        (self.root / "ProjectSettings").mkdir()
        version = self.root / "ProjectSettings/ProjectVersion.txt"
        version.write_text("m_EditorVersion: 6000.0.58f2\n")
        self.assertEqual(editor_version(self.root), "6000.0.58f2")
        version.write_text("m_EditorVersion: ../../unknown\n")
        self.assertIsNone(editor_version(self.root))

    def test_previous_four_case_report_no_longer_passes(self):
        self.fixture(inputs=False)
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_each_missing_input_case_fails(self):
        for missing in range(len(INPUT_CASES)):
            root = self.fixture(); root.remove(root[4 + missing]); ET.ElementTree(root).write(self.xml)
            self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_playmode_needs_exact_integration_suite(self):
        self.fixture()
        self.assertEqual(inspect_results(self.xml, "PlayMode").status, "FAIL")
        root = ET.Element("test-run", result="Passed")
        for name in PLAY_CASES:
            ET.SubElement(root, "test-case", fullname=PLAY_PREFIX + name, result="Passed")
        ET.ElementTree(root).write(self.xml)
        self.assertEqual(inspect_results(self.xml, "PlayMode").status, "PASS")
        root.remove(root[0]); ET.ElementTree(root).write(self.xml)
        self.assertEqual(inspect_results(self.xml, "PlayMode").status, "FAIL")

    def test_playmode_retains_graphics_and_never_quits_early(self):
        cmd = command(Path("Unity"), self.root, self.root, "PlayMode")
        self.assertNotIn("-nographics", cmd)
        self.assertNotIn("-quit", cmd)
        self.assertIn("RoomBreakers.Integration.PlayTests", cmd)

    def test_stale_xml_is_rejected_before_launch(self):
        self.fixture()
        with mock.patch("tools.run_unity_checks.subprocess.run") as launch:
            self.assertEqual(execute(Path("unused"), self.root, self.root, 5).status, "FAIL")
            launch.assert_not_called()

    def test_failed_child_suite_is_rejected(self):
        root = self.fixture(); ET.SubElement(root, "test-suite", result="Failed")
        ET.ElementTree(root).write(self.xml)
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_utf16_entity_report_is_rejected(self):
        self.xml.write_bytes('<!DOCTYPE test-run><test-run result="Passed"/>'.encode("utf-16"))
        self.assertEqual(inspect_results(self.xml).status, "FAIL")

    def test_actual_version_is_probed_not_inferred_from_path(self):
        with mock.patch("tools.run_unity_checks.subprocess.run", return_value=subprocess.CompletedProcess([], 0, "6000.0.58f2\n", "")) as launch:
            self.assertEqual(probe_version(Path("Unity")), "6000.0.58f2")
            self.assertEqual(launch.call_args.args[0], ["Unity", "-version"])

    def test_unknown_ambiguous_or_failed_version_is_blocked(self):
        for code, text in [(0, "unknown"), (0, "6000.0.1f1 6000.0.2f1"), (1, "6000.0.1f1")]:
            with mock.patch("tools.run_unity_checks.subprocess.run", return_value=subprocess.CompletedProcess([], code, text, "")):
                self.assertIsNone(probe_version(Path("Unity")))


if __name__ == "__main__":
    unittest.main()
