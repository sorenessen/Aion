#!/usr/bin/env python3
"""Create a source-only recovery snapshot of the current Est working tree."""
from pathlib import Path
import subprocess
import zipfile

root = Path(__file__).resolve().parents[1]

def git(*args, text=False):
    return subprocess.check_output(["git", *args], cwd=root, text=text)

names = git("ls-files", "-z", "--cached", "--others", "--exclude-standard").split(b"\0")
files = sorted({name.decode("utf-8", "surrogateescape") for name in names if name})
branch = git("branch", "--show-current", text=True).strip()
commit = git("log", "-1", "--oneline", text=True).strip()
status = git("status", "--short", text=True)
destination = root.parent / "Est-recovery.zip"

with zipfile.ZipFile(destination, "w", zipfile.ZIP_DEFLATED) as archive:
    for name in files:
        path = root / name
        if path.is_file() and not path.is_symlink():
            archive.write(path, arcname=f"Est/{name}")
    archive.writestr("Est/RECOVERY_STATUS.txt", f"Branch: {branch}\nCommit: {commit}\n\nGit status:\n{status}")

print(f"Created: {destination}")
print(f"Files: {len(files)}")
print(f"Branch: {branch}")
print(f"Commit: {commit}")
print(status or "Working tree clean.")
