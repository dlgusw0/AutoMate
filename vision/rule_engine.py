from typing import Any


def evaluate_detections(detector_result: dict[str, Any]) -> dict[str, Any]:
    """YOLO 결과로 차량 유무를 확인하고 다음 도구 실행을 결정한다."""
    if not detector_result.get("available"):
        return {
            "decision": "GEMINI_REQUIRED",
            "candidate_defects": [],
            "reason": "YOLO detector를 사용할 수 없습니다.",
        }

    counts = detector_result.get("counts", {})
    if counts.get("car", 0) < 1:
        return {
            "decision": "GEMINI_REQUIRED",
            "observed_counts": counts,
            "candidate_defects": [],
            "vehicle_presence_uncertain": True,
            "reason": (
                "YOLO가 차량 본체를 확인하지 못했습니다. 합성 영상이나 미학습 형상일 수 "
                "있으므로 원본 이미지를 Multimodal Vision이 교차 검증합니다."
            ),
        }

    return {
        "decision": "GEMINI_REQUIRED",
        "observed_counts": counts,
        "candidate_defects": [],
        "reason": (
            "YOLO가 차량 영역을 확인했습니다. 추가 학습 전의 부품 검출값은 "
            "누락 근거로 사용하지 않고 Multimodal Vision으로 조립 상태를 확인합니다."
        ),
    }
