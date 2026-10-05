import argparse
from collections import Counter
from pathlib import Path

import yaml
from PIL import Image, ImageDraw


IMAGE_SUFFIXES = {".jpg", ".jpeg", ".png"}
COLORS = ["#00ff66", "#00bfff", "#ffcc00", "#ff4dff"]


def load_config(config_path: Path) -> tuple[Path, dict[int, str]]:
    config = yaml.safe_load(config_path.read_text(encoding="utf-8"))
    dataset_root = Path(config["path"])
    if not dataset_root.is_absolute():
        dataset_root = Path.cwd() / dataset_root
    names = {int(class_id): str(name) for class_id, name in config["names"].items()}
    return dataset_root, names


def parse_label(
    label_path: Path,
    names: dict[int, str],
) -> list[tuple[int, float, float, float, float]]:
    boxes = []
    for line_number, raw_line in enumerate(
        label_path.read_text(encoding="utf-8").splitlines(), start=1
    ):
        line = raw_line.strip()
        if not line:
            continue
        parts = line.split()
        if len(parts) != 5:
            raise ValueError(f"{label_path}:{line_number} 항목이 5개가 아닙니다.")
        class_id = int(parts[0])
        cx, cy, width, height = map(float, parts[1:])
        if class_id not in names:
            raise ValueError(f"{label_path}:{line_number} 알 수 없는 class {class_id}")
        if not all(0 <= value <= 1 for value in (cx, cy, width, height)):
            raise ValueError(f"{label_path}:{line_number} 좌표가 0~1 범위를 벗어났습니다.")
        if width <= 0 or height <= 0:
            raise ValueError(f"{label_path}:{line_number} 박스 크기가 0 이하입니다.")
        if cx - width / 2 < 0 or cx + width / 2 > 1:
            raise ValueError(f"{label_path}:{line_number} x축 박스가 이미지를 벗어났습니다.")
        if cy - height / 2 < 0 or cy + height / 2 > 1:
            raise ValueError(f"{label_path}:{line_number} y축 박스가 이미지를 벗어났습니다.")
        boxes.append((class_id, cx, cy, width, height))
    return boxes


def save_preview(
    image_path: Path,
    boxes: list[tuple[int, float, float, float, float]],
    names: dict[int, str],
    output_path: Path,
) -> None:
    with Image.open(image_path).convert("RGB") as image:
        draw = ImageDraw.Draw(image)
        image_width, image_height = image.size
        for class_id, cx, cy, width, height in boxes:
            x1 = (cx - width / 2) * image_width
            y1 = (cy - height / 2) * image_height
            x2 = (cx + width / 2) * image_width
            y2 = (cy + height / 2) * image_height
            color = COLORS[class_id % len(COLORS)]
            draw.rectangle((x1, y1, x2, y2), outline=color, width=4)
            draw.text((x1 + 4, y1 + 4), names[class_id], fill=color)
        output_path.parent.mkdir(parents=True, exist_ok=True)
        image.save(output_path, quality=95)


def validate(config_path: Path, preview_dir: Path | None) -> None:
    dataset_root, names = load_config(config_path)
    class_counts: Counter[str] = Counter()
    image_count = 0

    for split in ("train", "val", "test"):
        image_dir = dataset_root / "images" / split
        label_dir = dataset_root / "labels" / split
        images = sorted(
            path for path in image_dir.iterdir() if path.suffix.lower() in IMAGE_SUFFIXES
        )
        print(f"{split}: {len(images)} images")
        for image_path in images:
            label_path = label_dir / f"{image_path.stem}.txt"
            if not label_path.exists():
                raise ValueError(f"라벨 파일이 없습니다: {label_path}")
            boxes = parse_label(label_path, names)
            if not boxes:
                raise ValueError(f"라벨이 비어 있습니다: {label_path}")
            image_count += 1
            class_counts.update(names[class_id] for class_id, *_ in boxes)
            if preview_dir:
                save_preview(
                    image_path,
                    boxes,
                    names,
                    preview_dir / split / f"{image_path.stem}.jpg",
                )

    if image_count == 0:
        raise ValueError("검사할 이미지가 없습니다.")
    print(f"total: {image_count} images")
    print("objects:", dict(class_counts))
    print("validation: PASS")


def main() -> None:
    parser = argparse.ArgumentParser(description="AutoMate YOLO 데이터셋 검증")
    parser.add_argument(
        "--data",
        type=Path,
        default=Path("datasets/automate/data.yaml"),
    )
    parser.add_argument("--preview-dir", type=Path)
    args = parser.parse_args()
    validate(args.data, args.preview_dir)


if __name__ == "__main__":
    main()
