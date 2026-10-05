using System;
using System.IO;
using UnityEngine;

namespace AutoMate.Simulation
{
    public sealed class UnityAgentBridge : MonoBehaviour
    {
        [Serializable]
        private sealed class AgentState
        {
            public string case_id;
            public int record_id;
            public string status;
            public string defect;
            public float confidence;
            public string risk_level;
            public string decision_status;
        }

        [Serializable]
        private sealed class OperatorCommand
        {
            public string command_id;
            public string case_id;
            public string action;
        }

        [SerializeField] private VehicleLineController vehicle;
        [SerializeField] private float pollingIntervalSeconds = 0.25f;

        private string statePath;
        private string commandPath;
        private DateTime lastStateWriteUtc = DateTime.MinValue;
        private DateTime lastCommandWriteUtc = DateTime.MinValue;
        private string lastCommandId = string.Empty;
        private string currentCaseId = string.Empty;
        private string currentStatus = "PENDING";
        private string currentDefect = "none";
        private string bridgeMessage = "검사 이미지 대기 중";
        private float nextPollAt;
        private Renderer greenLight;
        private Renderer redLight;

        private void Awake()
        {
            string bridgeDirectory = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "..", "unity_bridge")
            );
            Directory.CreateDirectory(bridgeDirectory);
            statePath = Path.Combine(bridgeDirectory, "agent_state.json");
            commandPath = Path.Combine(bridgeDirectory, "operator_command.json");

            if (vehicle == null)
            {
                vehicle = FindFirstObjectByType<VehicleLineController>();
            }

            GameObject greenObject = GameObject.Find("Green Light");
            GameObject redObject = GameObject.Find("Red Light");
            greenLight = greenObject == null ? null : greenObject.GetComponent<Renderer>();
            redLight = redObject == null ? null : redObject.GetComponent<Renderer>();

            if (File.Exists(commandPath))
            {
                lastCommandWriteUtc = File.GetLastWriteTimeUtc(commandPath);
                OperatorCommand existing = ReadJson<OperatorCommand>(commandPath);
                lastCommandId = existing == null ? string.Empty : existing.command_id;
            }
            SetIndicator(false, false);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextPollAt)
            {
                return;
            }
            nextPollAt = Time.unscaledTime + pollingIntervalSeconds;
            ReadAgentStateIfChanged();
            ReadOperatorCommandIfChanged();
        }

        public void ResetForNewInspection()
        {
            currentCaseId = string.Empty;
            currentStatus = "PENDING";
            currentDefect = "none";
            bridgeMessage = "새 검사 결과 대기 중";
            SetIndicator(false, false);

            if (File.Exists(statePath))
            {
                lastStateWriteUtc = File.GetLastWriteTimeUtc(statePath);
            }
            if (File.Exists(commandPath))
            {
                lastCommandWriteUtc = File.GetLastWriteTimeUtc(commandPath);
                OperatorCommand existing = ReadJson<OperatorCommand>(commandPath);
                lastCommandId = existing == null ? lastCommandId : existing.command_id;
            }
        }

        private void ReadAgentStateIfChanged()
        {
            if (!File.Exists(statePath))
            {
                return;
            }

            DateTime writeTime = File.GetLastWriteTimeUtc(statePath);
            if (writeTime <= lastStateWriteUtc)
            {
                return;
            }
            lastStateWriteUtc = writeTime;

            AgentState state = ReadJson<AgentState>(statePath);
            if (state == null || string.IsNullOrWhiteSpace(state.status))
            {
                bridgeMessage = "Agent 상태 파일 읽기 실패";
                return;
            }

            currentCaseId = state.case_id;
            currentStatus = state.status;
            currentDefect = state.defect;
            vehicle?.StopLine();

            if (state.status == "NORMAL")
            {
                SetIndicator(true, false);
                bridgeMessage = "정상 판정 · 작업자 승인 대기";
            }
            else
            {
                SetIndicator(false, true);
                bridgeMessage = $"라인 정지 · {state.status} / {state.risk_level}";
            }

            Debug.Log(
                $"[AutoMate] Agent result received: {state.status}, " +
                $"defect={state.defect}, confidence={state.confidence:0.00}"
            );
        }

        private void ReadOperatorCommandIfChanged()
        {
            if (!File.Exists(commandPath))
            {
                return;
            }

            DateTime writeTime = File.GetLastWriteTimeUtc(commandPath);
            if (writeTime <= lastCommandWriteUtc)
            {
                return;
            }
            lastCommandWriteUtc = writeTime;

            OperatorCommand command = ReadJson<OperatorCommand>(commandPath);
            if (command == null || string.IsNullOrWhiteSpace(command.command_id))
            {
                return;
            }
            if (command.command_id == lastCommandId)
            {
                return;
            }
            lastCommandId = command.command_id;

            bool caseMatches = !string.IsNullOrWhiteSpace(currentCaseId)
                && command.case_id == currentCaseId;
            if (command.action == "CONTINUE" && caseMatches && currentStatus == "NORMAL")
            {
                vehicle?.ContinueLine();
                SetIndicator(true, false);
                bridgeMessage = "작업자 승인 완료 · 라인 이동 중";
                Debug.Log("[AutoMate] Operator approved line continuation.");
            }
            else
            {
                Debug.LogWarning("[AutoMate] Rejected stale or unsafe operator command.");
            }
        }

        private static T ReadJson<T>(string path) where T : class
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[AutoMate] Bridge JSON read failed: {exception.Message}");
                return null;
            }
        }

        private void SetIndicator(bool green, bool red)
        {
            if (greenLight != null)
            {
                greenLight.enabled = green;
            }
            if (redLight != null)
            {
                redLight.enabled = red;
            }
        }

        private void OnGUI()
        {
            float width = 390f;
            float x = Screen.width - width - 18f;
            GUI.Box(new Rect(x, 18f, width, 105f), "Python Agent → Unity Control");
            GUI.Label(new Rect(x + 16f, 48f, width - 32f, 24f), bridgeMessage);
            GUI.Label(
                new Rect(x + 16f, 74f, width - 32f, 42f),
                $"Status: {currentStatus} · Defect: {currentDefect}"
            );
        }
    }
}
