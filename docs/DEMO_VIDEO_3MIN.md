# AutoMate 3분 실제 동작 영상 구성

영상의 중심은 `입력 → Agent 판단 → Tool/API 호출 → 결과 → 작업자 피드백`입니다. 장면 전환 효과나 긴 소개 화면은 사용하지 않습니다. Unity와 Streamlit을 한 화면에 나란히 두고 실제 클릭과 처리 시간을 그대로 촬영합니다.

## 촬영 전 준비

- Unity Game 화면과 Streamlit을 나란히 배치합니다.
- Streamlit 상단 버전이 `2026.10.06-demo-v2`인지 확인합니다.
- Unity Test Case Evaluation이 `5 / 5 PASS`, `100%`인지 확인합니다.
- 브라우저 확대 비율을 조정해 `Agent Execution Trace` 표가 한 화면에 보이게 합니다.
- 알림, 메신저, API 키가 표시된 터미널은 화면에서 제외합니다.

## 0:00–0:15 목표 설명

화면: AutoMate 상단과 6단계 Agent Workflow.

멘트:

> AutoMate는 자동차 조립공정 이미지를 분석하고, 이상 상황의 제조 매뉴얼 검색과 대응 계획, 작업자 조치 후 재검사까지 수행하는 AI Agent입니다.

## 0:15–0:40 실제 입력

화면: Unity에서 `TC02 · WHEEL MISSING` 클릭.

차량이 이동하고 검사 구역에서 멈춘 뒤 카메라 이미지가 자동 저장되는 모습을 보여줍니다. Streamlit에서 `Unity 캡처 목록 새로고침`을 누르고 가장 최신 `unity_wheel_missing_...png`를 선택합니다.

멘트:

> Unity 디지털 트윈의 검사 카메라가 생산 라인의 차량을 촬영해 Python Agent에 전달합니다.

## 0:40–1:25 Agent와 Tool/API 실행

화면: `검사 시작` 클릭 후 진행 상태를 펼친 채 촬영.

화면에 실제로 표시되는 실행 순서를 짧게 읽습니다.

1. OpenCV Quality Gate
2. YOLO Detection
3. Gemini Multimodal Inspection
4. Golden Sample Comparison
5. Manual Search
6. Risk Assessment와 Action Planning
7. SQLite Memory 저장

멘트:

> YOLO가 합성 차량을 충분히 탐지하지 못하면 Rule Engine이 Gemini 교차 검증으로 전환합니다. 같은 카메라의 정상 골든 샘플과 비교해 누락 부품을 확인합니다.

## 1:25–2:05 판단 결과와 물리 제어

화면: Agent Execution Trace, 검사 JSON, 매뉴얼, 대응 계획을 순서대로 보여줍니다.

반드시 보여줄 값:

```text
status: ABNORMAL
defect: wheel_missing
risk_level: CRITICAL
decision_status: ACTION_REQUIRED
SQLite record_id
```

Unity의 빨간 신호와 정지 차량도 함께 보여줍니다.

멘트:

> Agent는 휠 누락을 CRITICAL로 판단하고, 관련 제조 매뉴얼을 검색해 라인 정지와 휠 체결 상태 점검 절차를 제안합니다. 판정과 대응 상태는 SQLite에 저장되고 Unity 라인은 정지 상태를 유지합니다.

## 2:05–2:42 Human-in-the-loop와 재검사

1. Streamlit에서 `작업자 조치 완료 · 재검사 준비` 클릭
2. Unity에서 `TC01 · NORMAL` 선택
3. 새 정상 캡처를 재검사 입력으로 선택
4. `재검사 시작` 클릭
5. `RESOLVED` 결과 표시
6. 작업자 승인 후 Unity 라인 재가동

멘트:

> 작업 완료 후 새 이미지를 다시 검사합니다. 정상 판정이면 케이스를 RESOLVED로 갱신하고, 작업자가 최종 승인해야 라인이 다시 움직입니다.

## 2:42–3:00 평가와 마무리

화면: Unity Test Case Evaluation `5 / 5 PASS`, `100%`, SQLite 검사 이력.

멘트:

> 불확실한 영상은 UNKNOWN으로 처리해 작업자 확인을 요청합니다. AutoMate는 Vision, Planning, Tool Use, Memory, Feedback과 디지털 트윈 제어를 연결한 End-to-End 제조 AI Agent입니다.

## 기술설명서와 맞춰야 할 용어

| 영상 화면 | 기술설명서 용어 |
|---|---|
| OpenCV 품질 검사 | Perception Quality Gate |
| YOLO 탐지 결과 | Object Detection Tool |
| Gemini 판정 JSON | Multimodal Reasoning |
| 골든 샘플 비교 | Reference-based Inspection |
| manuals.json 검색 | Manufacturing Manual Retrieval |
| 대응 계획 | Risk Assessment & Action Planning |
| automate.db 저장 | Agent Memory / State |
| 재검사와 RESOLVED | Feedback Loop |
| Unity 신호와 라인 | Physical AI / Digital Twin Control |

## 촬영 원칙

- 검사 시작 버튼을 누르는 장면과 처리 대기 시간을 자르지 않습니다.
- Agent Execution Trace와 JSON 결과를 읽을 수 있는 크기로 촬영합니다.
- API 키, `.env`, 개인 알림은 노출하지 않습니다.
- 한 영상에서 휠 누락 케이스와 조치 후 정상 재검사까지 연결합니다.
- 다른 4개 케이스는 마지막 평가표로 실제 저장 결과를 증명합니다.
