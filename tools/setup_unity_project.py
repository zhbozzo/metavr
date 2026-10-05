#!/usr/bin/env python3
"""Prepare a real Unity desktop project; optional EditMode + PlayMode verification.

Requires an already installed, activated editor. Never installs Unity or Meta SDKs,
accepts licenses, builds Android, uploads logs, or writes fabricated Unity metadata.
"""
from __future__ import annotations

import argparse
from copy import deepcopy
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile
from typing import Sequence
import uuid

try:
    from .run_unity_checks import editor_version, execute, find_editor, probe_version
except ImportError:
    from run_unity_checks import editor_version, execute, find_editor, probe_version

ROOT = Path(__file__).resolve().parents[1]
PACKAGE_NAME = "com.zh.room-breakers"
SCENE = "Assets/RoomBreakersGenerated/DesktopEncounter.unity"
METHOD = "RoomBreakers.ScaleLab.Editor.DesktopProjectSetup.Prepare"


class SetupError(Exception):
    def __init__(self, detail: str, status: str = "BLOCKED"):
        super().__init__(detail)
        self.status = status


def object_pairs(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise SetupError("Duplicate JSON keys; original manifest was not changed")
        result[key] = value
    return result


def read_json(path: Path):
    try:
        with path.open("rb") as source:
            data = source.read(1_000_001)
        if len(data) > 1_000_000:
            raise SetupError("Unexpectedly large JSON file")
        return json.loads(data.decode("utf-8-sig"), object_pairs_hook=object_pairs)
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise SetupError("Missing, unreadable or malformed JSON; existing files were preserved") from error


def prepare_manifest(document: dict, project: Path, package: Path, test_version: str | None = None) -> dict:
    """Preserve unrelated dependencies/registries and reject conflicting package installations."""
    if not isinstance(document, dict) or not isinstance(document.get("dependencies"), dict):
        raise SetupError("Manifest needs a dependencies object")
    dependencies = document["dependencies"]
    if any(not isinstance(k, str) or not isinstance(v, str) for k, v in dependencies.items()):
        raise SetupError("Manifest dependencies must be strings")
    testables = document.get("testables", [])
    if not isinstance(testables, list) or any(not isinstance(x, str) for x in testables):
        raise SetupError("Manifest testables must be a string array")
    reference = "file:" + Path(os.path.relpath(package, project / "Packages")).as_posix()
    prior = dependencies.get(PACKAGE_NAME)
    if prior is not None:
        same = prior.startswith("file:") and not prior.startswith("file://")
        if same:
            same = ((project / "Packages" / prior[5:]).resolve() == package.resolve())
        if not same:
            raise SetupError("Another ROOMBREAKERS dependency is already present; reconcile it explicitly")
    if (project / "Packages" / PACKAGE_NAME).exists():
        raise SetupError("An embedded ROOMBREAKERS package would shadow the repository package")
    if test_version is not None:
        if re.fullmatch(r"[0-9]+\.[0-9]+\.[0-9]+", test_version) is None:
            raise SetupError("Specify an exact stable Test Framework version, not latest or a range")
        prior_test = dependencies.get("com.unity.test-framework")
        if prior_test not in (None, test_version):
            raise SetupError("Test Framework is already pinned differently; no implicit upgrade")
    result = deepcopy(document)
    result["dependencies"][PACKAGE_NAME] = reference
    if test_version is not None:
        result["dependencies"]["com.unity.test-framework"] = test_version
    result["testables"] = list(testables)
    if PACKAGE_NAME not in result["testables"]:
        result["testables"].append(PACKAGE_NAME)
    return result


def write_manifest(path: Path, before: bytes, document: dict, output: Path) -> bool:
    if path.is_symlink():
        raise SetupError("Refusing to replace a symlinked manifest")
    if path.read_bytes() != before:
        raise SetupError("Manifest changed during preparation; retry after closing other writers")
    desired = (json.dumps(document, indent=2, ensure_ascii=False) + "\n").encode("utf-8")
    # Semantic no-op leaves formatting, mtime and package resolution untouched.
    if read_json(path) == document:
        return False
    (output / "manifest.before.json").write_bytes(before)
    temporary = None
    try:
        with tempfile.NamedTemporaryFile(dir=path.parent, prefix=".rb-manifest-", delete=False) as stream:
            temporary = Path(stream.name)
            stream.write(desired)
            stream.flush()
            os.fsync(stream.fileno())
        if path.read_bytes() != before:
            raise SetupError("Manifest changed before commit; original file was not replaced")
        os.replace(temporary, path)
        return True
    finally:
        if temporary is not None:
            temporary.unlink(missing_ok=True)


def run_stage(editor: Path, arguments: list[str], output: Path, label: str, timeout: int):
    command = [str(editor), "-batchmode", "-nographics", "-quit"] + arguments + ["-logFile", str(output / f"{label}.log")]
    try:
        with (output / f"{label}-process.log").open("w", encoding="utf-8") as log:
            result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=timeout, check=False)
    except subprocess.TimeoutExpired as error:
        raise SetupError(f"Unity {label} timed out; inspect local logs before retrying", "FAIL") from error
    except OSError as error:
        raise SetupError("Unity could not be launched") from error
    if result.returncode != 0:
        raise SetupError(f"Unity {label} exited with code {result.returncode}; inspect local logs", "FAIL")


def validate_receipt(path: Path, project: Path, version: str, nonce: str) -> dict:
    receipt = read_json(path)
    expected = {"schema": 1, "status": "PREPARED", "unityVersion": version,
                "nonce": nonce, "scenePath": SCENE, "rigCount": 1, "cameraCount": 1,
                "questTested": False}
    if not isinstance(receipt, dict) or any(receipt.get(k) != v for k, v in expected.items()):
        raise SetupError("Unity did not produce the expected fresh scene receipt", "FAIL")
    for relative in (SCENE, SCENE + ".meta"):
        candidate = project / relative
        if not candidate.is_file() or candidate.stat().st_size == 0:
            raise SetupError("Unity scene or its editor-generated metadata is absent", "FAIL")
    return receipt


def setup(project: Path, package: Path, editor: Path, create: bool, verify: bool,
          test_version: str | None, output: Path, timeout: int) -> dict:
    if project == package or project in package.parents or package in project.parents:
        raise SetupError("Project destination overlaps source code/package directories")
    if (project / "Temp/UnityLockfile").exists():
        raise SetupError("Project is open in Unity; close it before preparation")
    metadata = read_json(package / "package.json")
    if not isinstance(metadata, dict) or metadata.get("name") != PACKAGE_NAME:
        raise SetupError("The repository package is missing or has an unexpected identity")
    actual = probe_version(editor)
    if actual is None or not actual.startswith("6000."):
        raise SetupError("Select an installed Unity 6 editor; its exact version could not be verified")
    declared = editor_version(project)
    if declared is None:
        if not create:
            raise SetupError("Project is missing; use --create to let Unity create it")
        if project.exists() and (not project.is_dir() or any(project.iterdir())):
            raise SetupError("Destination is nonempty but is not a valid Unity project; nothing was deleted")
        run_stage(editor, ["-createProject", str(project)], output, "create", timeout)
        declared = editor_version(project)
    if declared != actual:
        raise SetupError("Project/editor versions differ or Unity did not create ProjectVersion; no upgrade attempted")
    manifest = project / "Packages/manifest.json"
    if manifest.is_symlink():
        raise SetupError("Refusing to replace a symlinked manifest")
    before = manifest.read_bytes()
    document = prepare_manifest(read_json(manifest), project, package, test_version)
    if verify and "com.unity.test-framework" not in document["dependencies"]:
        raise SetupError("Add Test Framework with Package Manager, or pass --test-framework-version with your chosen exact version")
    changed = write_manifest(manifest, before, document, output)
    nonce = uuid.uuid4().hex
    receipt_path = output / "scene-receipt.json"
    if receipt_path.exists():
        raise SetupError("Receipt path is not fresh; existing evidence was preserved", "FAIL")
    run_stage(editor, ["-projectPath", str(project), "-executeMethod", METHOD,
                      "-rbSetupReceipt", str(receipt_path), "-rbSetupNonce", nonce], output, "prepare", timeout)
    validate_receipt(receipt_path, project, actual, nonce)
    results = {}
    if verify:
        for platform in ("EditMode", "PlayMode"):
            destination = output / platform
            destination.mkdir()
            result = execute(editor, project, destination, timeout, platform)
            results[platform] = {"status": result.status, "cases": result.cases, "detail": result.detail}
            if result.status != "PASS":
                raise SetupError(f"{platform}: {result.detail}", result.status)
    return {"status": "PASS" if verify else "PREPARED", "declared_editor": actual,
            "manifest_changed": changed, "scene": SCENE, "checks": results,
            "detail": "Desktop integration prepared" + (" and required engine tests passed" if verify else "; engine tests were not run"),
            "apk_tested": False, "quest_tested": False, "visual_quality_verified": False}


def main(argv: Sequence[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, default=ROOT / "unity/RoomBreakers")
    parser.add_argument("--unity", help="Installed editor executable (or UNITY_EDITOR environment variable)")
    parser.add_argument("--create", action="store_true", help="Allow Unity to create a missing project in an empty destination")
    parser.add_argument("--verify", action="store_true", help="Run all required EditMode and PlayMode integration cases")
    parser.add_argument("--test-framework-version", help="Optional explicit stable version; never upgrades an existing different pin")
    parser.add_argument("--timeout", type=int, default=600)
    args = parser.parse_args(argv)
    if args.timeout <= 0:
        parser.error("--timeout must be positive")
    output = None
    try:
        project = args.project.expanduser().resolve()
        editor = find_editor(editor_version(project) or "", args.unity)
        if editor is None:
            raise SetupError("No editor found. Install/activate Unity locally, then set UNITY_EDITOR or --unity; no Unity step ran")
        base = ROOT / ".validation-local"
        base.mkdir(exist_ok=True)
        output = Path(tempfile.mkdtemp(prefix="setup-", dir=base))
        report = setup(project, ROOT / "unity/Packages" / PACKAGE_NAME, editor.resolve(),
                       args.create, args.verify, args.test_framework_version, output, args.timeout)
    except SetupError as error:
        report = {"status": error.status, "detail": str(error), "apk_tested": False, "quest_tested": False}
    except (OSError, ValueError) as error:
        report = {"status": "BLOCKED", "detail": "Local files could not be accessed; no success is claimed", "apk_tested": False, "quest_tested": False}
    if output is not None:
        report["local_report_directory"] = str(output)
        (output / "summary.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 0 if report["status"] in ("PASS", "PREPARED") else 1 if report["status"] == "FAIL" else 2


if __name__ == "__main__":
    raise SystemExit(main())
