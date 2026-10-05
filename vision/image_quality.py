from typing import Any

import cv2
import numpy as np
from PIL import Image


def analyze_image_quality(
    image: Image.Image,
    min_brightness: float = 65.0,
    max_brightness: float = 245.0,
    min_blur_score: float = 45.0,
) -> dict[str, Any]:
    """밝기와 Laplacian 분산으로 API 호출 전 이미지 품질을 검사한다."""
    rgb = np.asarray(image.convert("RGB"))
    gray = cv2.cvtColor(rgb, cv2.COLOR_RGB2GRAY)
    brightness = float(np.mean(gray))
    blur_score = float(cv2.Laplacian(gray, cv2.CV_64F).var())

    reasons: list[str] = []
    if brightness < min_brightness:
        reasons.append("TOO_DARK")
    elif brightness > max_brightness:
        reasons.append("TOO_BRIGHT")
    if blur_score < min_blur_score:
        reasons.append("TOO_BLURRY")

    return {
        "passed": not reasons,
        "brightness": round(brightness, 2),
        "blur_score": round(blur_score, 2),
        "reasons": reasons,
        "thresholds": {
            "min_brightness": min_brightness,
            "max_brightness": max_brightness,
            "min_blur_score": min_blur_score,
        },
    }

