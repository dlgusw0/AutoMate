# AutoMate 5분 시연 가이드

## 시연 전 준비

1. Unity에서 `AssemblyLine` 씬의 Game 화면을 실행합니다.
2. Streamlit을 실행하고 `Workflow · 2026.10.06-demo-v2`를 확인합니다.
3. Streamlit의 Unity Test Case Evaluation이 `5 / 5`, `100%`인지 확인합니다.
4. Unity와 Streamlit 창을 나란히 배치합니다.

## 0:00–0:40 문제와 목표

> AutoMate는 자동차 조립공정 카메라 이미지를 분석하고, 이상 상황에서는 제조 매뉴얼을 검색해 위험도와 대응 절차를 제안하는 AI Agent입니다. 작업자 조치 후 재검사와 생산 라인 제어까지 하나의 흐름으로 연결했습니다.

화면 상단의 6단계 Agent Workflow를 보여줍니다.

## 0:40–1:30 정상 차량

1. Unity에서 `TC01 · NORMAL`을 선택합니다.
2. 차량이 검사 위치에 멈추고 자동 촬영되는 모습을 보여줍니다.
3. Streamlit에서 최신 `unity_normal_...png`를 선택하고 검사합니다.
4. `NORMAL · LOW`와 SQLite 저장 결과를 보여줍니다.
5. Unity가 초록 신호에서 작업자 승인을 기다리는 점을 설명합니다.
6. `작업자 승인 · Unity 라인 재가동`을 눌러 차량을 이동시킵니다.

핵심 설명:

> 정상 판정도 작업자가 승인해야 라인이 재가동되는 Human-in-the-loop 구조입니다.

## 1:30–3:10 휠 누락 이상

1. Unity에서 `TC02 · WHEEL MISSING`을 선택합니다.
2. 새 캡처를 Streamlit에서 검사합니다.
3. 다음 결과를 순서대로 보여줍니다.
   - OpenCV Quality Gate
   - YOLO Detection 및 Gemini 교차 검증
   - `ABNORMAL · wheel_missing · CRITICAL`
   - 검색된 휠 조립 제조 매뉴얼
   - 가능 원인, 즉시 안전조치, 확인 절차
   - Unity 빨간 신호와 정지 유지

핵심 설명:

> Agent는 이미지를 분류하는 데서 끝나지 않고 매뉴얼을 도구로 검색해 위험도와 대응 순서를 생성하며, 결과와 상태를 SQLite에 기억합니다.

## 3:10–4:10 조치와 재검사

1. Streamlit에서 `작업자 조치 완료 · 재검사 준비`를 누릅니다.
2. Unity에서 `TC01 · NORMAL`을 선택해 조치 후 이미지를 생성합니다.
3. 새 정상 이미지를 재검사에 사용합니다.
4. `RESOLVED` 상태와 Unity 라인 재가동 승인 버튼을 보여줍니다.

핵심 설명:

> 재검사에서 이상이 계속되면 대응 계획을 다시 생성하고, 정상일 때만 RESOLVED로 변경합니다.

## 4:10–4:45 안전한 불확실성 처리

1. Unity에서 `TC05 · UNCERTAIN CAMERA` 결과를 보여줍니다.
2. `UNKNOWN`, 작업자 확인 요청, 라인 정지를 설명합니다.

> 근거가 부족할 때 AI가 결함을 임의 확정하지 않고 작업자 확인을 요청합니다.

## 4:45–5:00 평가 결과

Unity Test Case Evaluation의 `5 / 5 PASS`, `100%`와 SQLite 최근 검사 이력을 보여줍니다.

마지막 문장:

> AutoMate는 Vision, Tool Use, Planning, Memory, Human Feedback과 디지털 트윈 제어를 결합한 End-to-End 제조 AI Agent입니다.

## 시연 장애 대응

- Gemini 요청 제한이 발생하면 SQLite에 저장된 5개 평가 결과와 이전 검사 결과를 보여줍니다.
- Unity 캡처가 바로 나타나지 않으면 `Unity 캡처 목록 새로고침`을 누릅니다.
- Unity 신호가 바뀌지 않으면 Play 상태와 `unity_bridge/agent_state.json` 생성 여부를 확인합니다.
- 잘못된 예전 이미지를 선택하지 않도록 가장 최신 타임스탬프를 확인합니다.
