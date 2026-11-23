"""Check public control coverage and local Markdown links against the repository source."""

import re
from pathlib import Path


def public_controls(repo):
    """Follow public base classes; include code-behind whose base is declared in XAML."""
    seeds = set("Control ContentControl UserControl Button CheckBox TextBlock TextBox ComboBox ListBox Panel Selector Freezable FrameworkElement ToolTip ContextMenu MenuItem HeaderedItemsControl HeaderedContentControl Border Label ListBoxItem Shape Decorator RepeatButton ScrollViewer TextEditor ItemsControl Image ProgressBar TabControl TabItem ToggleButton Window".split())
    candidates = {}
    for path in (repo / "src/Mosaic.UI.Wpf/Controls").rglob("*.cs"):
        source = path.read_text(encoding="utf-8-sig")
        namespace = re.search(r"namespace\s+([\w.]+)", source)
        for match in re.finditer(r"public\s+(?:(?:abstract|sealed|partial)\s+)*class\s+(\w+)(?:\s*:\s*([\w.]+))?", source):
            name, base = match.groups()
            if path.name.endswith(".xaml.cs"):
                xaml = path.with_suffix("").read_text(encoding="utf-8-sig")
                root = re.search(r"<([\w:]+)\b", re.sub(r"<!--.*?-->", "", xaml, flags=re.S))
                if root:
                    base = root[1].split(":")[-1]
            candidates[name] = (base, path, namespace[1] if namespace else "")
    found = {}
    changed = True
    while changed:
        changed = False
        for name, (base, path, namespace) in candidates.items():
            if name not in found and base and (base.split(".")[-1] in seeds or base.split(".")[-1] in found):
                found[name] = (base, path, namespace)
                changed = True
    return found


def main():
    skill = Path(__file__).resolve().parents[1]
    repo = skill.parents[2]
    inventory = (skill / "SKILL.md").read_text(encoding="utf-8")
    indexed = set(re.findall(r"^\| `([\w]+)`", inventory, re.M))
    controls = public_controls(repo)
    missing = sorted(set(controls) - indexed)
    broken = []
    for page in skill.rglob("*.md"):
        for target in re.findall(r"\]\(([^)]+)\)", page.read_text(encoding="utf-8")):
            if "://" not in target and not target.startswith("#") and not (page.parent / target.split("#")[0]).exists():
                broken.append(f"{page.relative_to(skill)}: {target}")
    print(f"Public control types: {len(controls)}; indexed: {len(set(controls) & indexed)}")
    if missing:
        print("Missing inventory rows: " + ", ".join(missing))
    if broken:
        print("Broken local links:\n" + "\n".join(broken))
    if missing or broken:
        raise SystemExit(1)
    print("Control inventory coverage and local links passed.")


if __name__ == "__main__":
    main()
