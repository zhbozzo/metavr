#!/usr/bin/env python3
"""Validate foundation files and simple local Markdown links; no Unity test.

Only Markdown links of the form [label](relative/path) are checked. This is
not a complete Markdown parser, security scanner or external-link checker.
"""
from pathlib import Path
import re
import sys
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
REQUIRED = (
    "README.md", "AGENTS.md", "CLAUDE.md",
    "docs/CONTEXTO.md", "docs/PRODUCTO.md", "docs/JUEGO.md",
    "docs/UX_SEGURIDAD.md", "docs/ARQUITECTURA.md", "docs/ENTORNO.md",
    "docs/PLAN.md", "docs/BACKLOG.md", "docs/CONCURSO.md", "docs/PRUEBAS.md",
    "docs/FUENTES.md", "docs/ARTE_AUDIO.md", "docs/DECISIONES.md",
    "docs/PRIVACIDAD_LICENCIAS.md", "docs/ESTADO.md", "docs/ENTREGA.md",
    "prompts/INICIO.md", "prompts/CONTINUAR.md", "infra/README.md",
    "unity/README.md", "submission/DRAFT_EN.md", "submission/JUDGE_INSTRUCTIONS_EN.md",
    "prototypes/reference_model.py", "tests/test_reference_model.py",
)
LINK = re.compile(r"\[[^\]\n]+\]\(([^)\s]+)(?:\s+\"[^\"]*\")?\)")
SKIP_DIRS = {".git", "Library", "Temp", "Obj", "Logs", "node_modules", ".venv"}


def main() -> int:
    errors = []
    for name in REQUIRED:
        path = ROOT / name
        if not path.is_file():
            errors.append(f"Missing required file: {name}")
        elif not path.read_text(encoding="utf-8").strip():
            errors.append(f"Empty required file: {name}")

    checked = 0
    for path in ROOT.rglob("*.md"):
        if SKIP_DIRS.intersection(path.relative_to(ROOT).parts):
            continue
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeError:
            errors.append(f"Not UTF-8: {path.relative_to(ROOT)}")
            continue
        for raw in LINK.findall(text):
            parsed = urlsplit(raw.strip("<>"))
            if parsed.scheme or parsed.netloc or not parsed.path:
                continue
            target = (path.parent / unquote(parsed.path)).resolve()
            if not target.is_relative_to(ROOT):
                errors.append(f"Link outside repository: {path.relative_to(ROOT)} -> {raw}")
            elif not target.exists():
                errors.append(f"Broken local link: {path.relative_to(ROOT)} -> {raw}")
            checked += 1

    if errors:
        for error in errors:
            print(f"FAIL: {error}", file=sys.stderr)
        return 1
    print(f"PASS: {len(REQUIRED)} required files; {checked} simple local links checked.")
    print("Foundation integrity only. Unity, Android, hardware and eligibility are NOT tested.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
