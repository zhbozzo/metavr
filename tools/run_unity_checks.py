#!/usr/bin/env python3
"""Run actual Unity EditMode checks; absent tools and missing results never pass.

Sources: Unity Test Framework command-line reference (checked 2026-10-04).
This does not install Unity, accept a license, create a project, or test a Quest.
"""
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


@dataclass(frozen=True)
class Outcome:
    status: str
    detail: str
    cases: int = 0


def inspect_results(path: Path) -> Outcome:
    """Check real NUnit output, not just the editor exit status."""
    try:
        if path.stat().st_size > 10_000_000:
            return Outcome("FAIL", "Unexpectedly large NUnit result file")
        data = path.read_bytes()
        if b"<!DOCTYPE" in data or b"<!ENTITY" in data:
            return Outcome("FAIL", "DTD/entity declarations are not allowed")
        root = ET.fromstring(data)
    except (OSError, ET.ParseError):
        return Outcome("FAIL", "Missing, unreadable or malformed NUnit results")
    if root.tag != "test-run" or root.get("result") != "Passed":
        return Outcome("FAIL", "NUnit run did not report Passed")
    cases = list(root.iter("test-case"))
    if not cases or any(case.get("result") != "Passed" for case in cases):
        return Outcome("FAIL", "Zero, failed, skipped or inconclusive test cases", len(cases))
    names = [case.get("fullname", "") for case in cases]
    if len(set(names)) != len(names) or not all(names):
        return Outcome("FAIL", "Missing or duplicate test identifiers", len(cases))
    matched = [name for name in names if name.startswith(PREFIX)]
    frames = [name for name in matched if name.startswith(PREFIX + "NumericFrameMatchesActualUnityTransform(")]
    hand = PREFIX + "EnlargedHandPreservesRoomPoseAcrossTwoRotatedFrames"
    if len(frames) < 3 or hand not in matched:
        return Outcome("FAIL", "Required ROOMBREAKERS transform cases were not all executed", len(cases))
    return Outcome("PASS", "Unity EditMode cases passed; this is NOT a device validation", len(cases))


def editor_version(project: Path) -> str | None:
    try:
        text = (project / "ProjectSettings" / "ProjectVersion.txt").read_text(encoding="utf-8")
    except OSError:
        return None
    match = re.search(r"^m_EditorVersion:\s*([0-9]+\.[0-9]+\.[0-9]+[abfp][0-9]+)\s*$", text, re.MULTILINE)
    return match.group(1) if match else None


def find_editor(version: str, explicit: str | None) -> Path | None:
    override = explicit or os.environ.get("UNITY_EDITOR")
    if override:
        candidate = Path(override).expanduser()
        return candidate if candidate.is_file() and os.access(candidate, os.X_OK) else None
    # Only the version declared by the project; never choose a newer editor implicitly.
    candidates = [
        Path("/Applications/Unity/Hub/Editor") / version / "Unity.app/Contents/MacOS/Unity",
        Path.home() / "Unity/Hub/Editor" / version / "Editor/Unity",
        Path(os.environ.get("PROGRAMFILES", "C:/Program Files")) / "Unity/Hub/Editor" / version / "Editor/Unity.exe",
    ]
    return next((p for p in candidates if p.is_file() and os.access(p, os.X_OK)), None)


def command(editor: Path, project: Path, output: Path) -> list[str]:
    # -quit can stop tests before completion; the test runner controls shutdown.
    # A list of arguments avoids shell parsing of paths with spaces.
    return [str(editor), "-batchmode", "-nographics", "-runTests", "-projectPath", str(project),
            "-testPlatform", "EditMode", "-assemblyNames", ASSEMBLY,
            "-testResults", str(output / "results.xml"), "-logFile", str(output / "editor.log")]


def execute(editor: Path, project: Path, output: Path, timeout: int) -> Outcome:
    try:
        # Each call receives a fresh output folder from main: stale XML cannot pass.
        with (output / "process.log").open("w", encoding="utf-8") as log:
            result = subprocess.run(command(editor, project, output), stdout=log,
                                    stderr=subprocess.STDOUT, timeout=timeout, check=False)
    except subprocess.TimeoutExpired:
        return Outcome("FAIL", "Editor timed out; inspect the local logs")
    except OSError:
        return Outcome("BLOCKED", "Editor could not be launched; inspect installation and permissions")
    if result.returncode != 0:
        return Outcome("FAIL", f"Editor exited with code {result.returncode}; inspect local logs")
    return inspect_results(output / "results.xml")


def main(argv: Sequence[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, default=ROOT / "unity/RoomBreakers")
    parser.add_argument("--unity", help="Explicit editor executable; must match the project's version")
    parser.add_argument("--timeout", type=int, default=600)
    args = parser.parse_args(argv)
    if args.timeout <= 0:
        parser.error("--timeout must be positive")
    project = args.project.expanduser().resolve()
    version = editor_version(project)
    output = None
    if version is None or not (project / "Packages/manifest.json").is_file():
        outcome = Outcome("BLOCKED", "A real Unity project with ProjectVersion and package manifest is required")
    else:
        editor = find_editor(version, args.unity)
        if editor is None:
            outcome = Outcome("BLOCKED", f"Unity {version} was not found; no editor test has run")
        elif (project / "Temp/UnityLockfile").exists():
            outcome = Outcome("BLOCKED", "Project appears open in Unity; close it before batch tests")
        else:
            # Logs may contain machine paths. Store locally, never publish automatically.
            base = ROOT / ".validation-local"
            base.mkdir(exist_ok=True)
            output = Path(tempfile.mkdtemp(prefix="unity-", dir=base))
            outcome = execute(editor, project, output, args.timeout)
    report = {"timestamp_utc": datetime.now(timezone.utc).isoformat(), **asdict(outcome),
              "declared_editor": version, "apk_tested": False, "quest_tested": False}
    if output is not None:
        (output / "summary.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        report["local_report_directory"] = str(output)
    print(json.dumps(report, indent=2))
    return {"PASS": 0, "FAIL": 1, "BLOCKED": 2}[outcome.status]


if __name__ == "__main__":
    raise SystemExit(main())
