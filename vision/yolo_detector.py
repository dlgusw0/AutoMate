from collections import Counter
from pathlib import Path
from typing import Any

import numpy as np
from PIL import Image


DEFAULT_CLASSES = ["car", "wheel", "headlight", "car door"]


class YoloWorldDetector:
    """텍스트로 지정한 자동차 부품을 탐지하는 YOLO-World 도구."""

    def __init__(
        self,
        model_path: str = "yolov8s-worldv2.pt",
        classes: list[str] | None = None,
    ) -> None:
        from ultralytics import YOLO

        self.model_path = model_path
        self.classes = classes or DEFAULT_CLASSES
        self.model = YOLO(model_path)
        self.model.set_classes(self.classes)

    def detect(
        self,
        image: Image.Image,
        confidence: float = 0.15,
        image_size: int = 640,
    ) -> tuple[dict[str, Any], Image.Image]:
        rgb = np.asarray(image.convert("RGB"))
        results = self.model.predict(
            source=rgb,
            conf=confidence,
            imgsz=image_size,
            verbose=False,
            device="cpu",
        )
        result = results[0]
        detections: list[dict[str, Any]] = []
        if result.boxes is not None:
            for box in result.boxes:
                class_id = int(box.cls.item())
                class_name = str(result.names[class_id])
                coordinates = [round(float(value), 1) for value in box.xyxy[0].tolist()]
                detections.append(
                    {
                        "class_name": class_name,
                        "confidence": round(float(box.conf.item()), 3),
                        "bbox_xyxy": coordinates,
                    }
                )

        counts = dict(Counter(item["class_name"] for item in detections))
        plotted_bgr = result.plot()
        plotted_rgb = plotted_bgr[:, :, ::-1]
        annotated = Image.fromarray(plotted_rgb)
        return (
            {
                "available": True,
                "source": "YOLO_WORLD",
                "model": Path(self.model_path).name,
                "classes": self.classes,
                "counts": counts,
                "detections": detections,
            },
            annotated,
        )


def unavailable_result(
    reason: str,
    model_path: str | None = None,
    classes: list[str] | None = None,
) -> dict[str, Any]:
    """모델을 사용할 수 없을 때도 성공 결과와 같은 dict 구조를 반환한다."""
    return {
        "available": False,
        "source": "YOLO_WORLD",
        "model": Path(model_path).name if model_path else None,
        "classes": classes or DEFAULT_CLASSES,
        "counts": {},
        "detections": [],
        "reason": reason,
    }
