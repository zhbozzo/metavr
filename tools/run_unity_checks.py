#!/usr/bin/env python3
"""Run real Unity tests with complete suite evidence. Never substitute .NET or mocks."""
from __future__ import annotations

import argparse
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile
from typing import Sequence
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
ASSEMBLY = "RoomBreakers.Core.UnityTests"
PREFIX = "RoomBreakers.Tests.SpatialFrameUnityTests."
INPUT_PREFIX = "RoomBreakers.Tests.UnityInputFrameTests."
INPUT_CASES = (
    "MissingFrameRejected", "UniformHierarchyMatchesTransform", "NonUniformAncestorRejected",
    "MirroredAncestorRejected", "ZeroScaleRejected", "PhysicalRoomRejectsScaledTransform",
    "OutOfRangeScaleRejected", "DriverUsesUnityWorldPoseWithoutDoubleTransform",
)
PLAY_ASSEMBLY = "RoomBreakers.Integration.PlayTests"
PLAY_PREFIX = "RoomBreakers.Tests.EncounterIntegrationTests."
PLAY_CASES = (
    "BootPinchLoadsTheRealRig", "CaptureSynchronizesBothRenderedScales",
    "FullEncounterWinsAndRestartsThroughSceneInput", "DisableAndInvalidationStopTheEncounter",
)
VERSION_PATTERN = r"[0-9]+\.[0-9]+\.[0-9]+[abfp][0-9]+"


@dataclass(frozen=True)
class Outcome:
    status: str
    detail: str
    cases: int = 0


def inspect_results(path: Path, platform: str = "EditMode") -> Outcome:
    """NUnit XML must contain every required case, not just a successful exit code."""
    if platform not in ("EditMode", "PlayMode"):
        return Outcome("FAIL", "Unknown test platform")
    try:
        with path.open("rb") as source:
            data = source.read(10_000_001)
        if len(data) > 10_000_000:
            return Outcome("FAIL", "Unexpectedly large NUnit result file")
        text = data.decode("utf-8-sig")
        if "<!DOCTYPE" in text.upper() or "<!ENTITY" in text.upper():
            return Outcome("FAIL", "DTD/entity declarations are not allowed")
        root = ET.fromstring(text)
    except (OSError, UnicodeError, ET.ParseError):
        return Outcome("FAIL", "Missing, unreadable or malformed NUnit results")
    if root.tag != "test-run" or root.get("result") != "Passed":
        return Outcome("FAIL", "NUnit run did not report Passed")
    cases = list(root.iter("test-case"))
    if not cases or any(case.get("result") != "Passed" for case in cases):
        return Outcome("FAIL", "Zero, failed, skipped or inconclusive test cases", len(cases))
    if any(s.get("result") not in (None, "Passed") for s in root.iter("test-suite")):
        return Outcome("FAIL", "A child test suite did not pass", len(cases))
    names = [case.get("fullname", "") for case in cases]
    if len(set(names)) != len(names) or not all(names):
        return Outcome("FAIL", "Missing or duplicate test identifiers", len(cases))
    if platform == "EditMode":
        frames = [name for name in names if name.startswith(PREFIX + "NumericFrameMatchesActualUnityTransform(")]
        required = {PREFIX + "EnlargedHandPreservesRoomPoseAcrossTwoRotatedFrames"}
        required.update(INPUT_PREFIX + name for name in INPUT_CASES)
        complete = len(frames) >= 3 and required.issubset(names)
    else:
        complete = {PLAY_PREFIX + name for name in PLAY_CASES}.issubset(names)
    if not complete:
        return Outcome("FAIL", "Required ROOMBREAKERS cases were not all executed", len(cases))
    return Outcome("PASS", f"Unity {platform} suite passed; not a Quest or visual-quality validation", len(cases))


def editor_version(project: Path) -> str | None:
    try:
        text = (project / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8")
    except (OSError, UnicodeError):
        return None
    match = re.search(r"^m_EditorVersion:\s*(" + VERSION_PATTERN + r")\s*$", text, re.MULTILINE)
    return match.group(1) if match else None


def find_editor(version: str, explicit: str | None) -> Path | None:
    override = explicit or os.environ.get("UNITY_EDITOR")
    if override:
        candidate = Path(override).expanduser()
        return candidate if candidate.is_file() and os.access(candidate, os.X_OK) else None
    candidates = [
        Path("/Applications/Unity/Hub/Editor") / version / "Unity.app/Contents/MacOS/Unity",
        Path.home() / "Unity/Hub/Editor" / version / "Editor/Unity",
        Path(os.environ.get("PROGRAMFILES", "C:/Program Files")) / "Unity/Hub/Editor" / version / "Editor/Unity.exe",
    ]
    return next((p for p in candidates if p.is_file() and os.access(p, os.X_OK)), None)


def probe_version(editor: Path) -> str | None:
    # Official -version prints without opening a project. Never upgrade by guessing from a path.
    try:
        result = subprocess.run([str(editor), "-version"], capture_output=True, text=True,
                                timeout=30, check=False)
    except (OSError, subprocess.TimeoutExpired, UnicodeError):
        return None
    if result.returncode != 0:
        return None
    versions = set(re.findall(r"(?<![0-9.])" + VERSION_PATTERN + r"(?![0-9a-z])",
                              result.stdout + "\n" + result.stderr))
    return next(iter(versions)) if len(versions) == 1 else None


def command(editor: Path, project: Path, output: Path, platform: str = "EditMode") -> list[str]:
    if platform not in ("EditMode", "PlayMode"):
        raise ValueError("Unknown test platform")
    # -quit aborts asynchronous tests. PlayMode retains a graphics device on a workstation.
    args = [str(editor), "-batchmode"]
    if platform == "EditMode":
        args.append("-nographics")
    return args + ["-runTests", "-projectPath", str(project), "-testPlatform", platform,
                   "-assemblyNames", ASSEMBLY if platform == "EditMode" else PLAY_ASSEMBLY,
                   "-testResults", str(output / "results.xml"), "-logFile", str(output / "editor.log")]


def execute(editor: Path, project: Path, output: Path, timeout: int, platform: str = "EditMode") -> Outcome:
    # Even a caller bypassing main must not pass by reusing a previous successful report.
    if (output / "results.xml").exists():
        return Outcome("FAIL", "Result path is not fresh; existing evidence was left untouched")
    try:
        with (output / "process.log").open("w", encoding="utf-8") as log:
            result = subprocess.run(command(editor, project, output, platform), stdout=log,
                                    stderr=subprocess.STDOUT, timeout=timeout, check=False)
    except subprocess.TimeoutExpired:
        return Outcome("FAIL", "Editor timed out; inspect local logs and close remaining Unity processes")
    except OSError:
        return Outcome("BLOCKED", "Editor could not be launched")
    if result.returncode != 0:
        return Outcome("FAIL", f"Editor exited with code {result.returncode}; inspect local logs")
    return inspect_results(output / "results.xml", platform)


def main(argv: Sequence[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, default=ROOT / "unity/RoomBreakers")
    parser.add_argument("--unity", help="Exact editor executable; must match ProjectVersion")
    parser.add_argument("--platform", choices=("EditMode", "PlayMode"), default="EditMode")
    parser.add_argument("--timeout", type=int, default=600)
    args = parser.parse_args(argv)
    if args.timeout <= 0:
        parser.error("--timeout must be positive")
    project = args.project.expanduser().resolve()
    version = editor_version(project)
    output = None
    if version is None or not (project / "Packages/manifest.json").is_file():
        outcome = Outcome("BLOCKED", "A real Unity project with ProjectVersion and manifest is required")
    else:
        editor = find_editor(version, args.unity)
        if editor is None:
            outcome = Outcome("BLOCKED", f"Unity {version} was not found; no editor test has run")
        elif (project / "Temp/UnityLockfile").exists():
            outcome = Outcome("BLOCKED", "Project is open in Unity; close it before batch tests")
        elif probe_version(editor) != version:
            outcome = Outcome("BLOCKED", "Selected editor version is unknown or differs from ProjectVersion")
        else:
            base = ROOT / ".validation-local"
            base.mkdir(exist_ok=True)
            output = Path(tempfile.mkdtemp(prefix="unity-", dir=base))
            outcome = execute(editor, project, output, args.timeout, args.platform)
    report = {"timestamp_utc": datetime.now(timezone.utc).isoformat(), **asdict(outcome),
              "platform": args.platform, "declared_editor": version, "apk_tested": False, "quest_tested": False}
    if output is not None:
        (output / "summary.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        report["local_report_directory"] = str(output)
    print(json.dumps(report, indent=2))
    return {"PASS": 0, "FAIL": 1, "BLOCKED": 2}[outcome.status]


if __name__ == "__main__":
    raise SystemExit(main())
