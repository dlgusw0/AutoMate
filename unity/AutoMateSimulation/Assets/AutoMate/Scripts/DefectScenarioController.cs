using UnityEngine;

namespace AutoMate.Simulation
{
    public enum InspectionScenario
    {
        Normal,
        WheelMissing,
        HeadlightMissing,
        DoorMisaligned,
        Uncertain
    }

    public sealed class DefectScenarioController : MonoBehaviour
    {
        [SerializeField] private VehicleLineController vehicleController;

        private Transform frontLeftWheel;
        private Transform frontLeftHeadlight;
        private Transform leftDoor;
        private GameObject cameraOccluder;
        private Vector3 doorNormalPosition;
        private Quaternion doorNormalRotation;
        private UnityAgentBridge agentBridge;

        public InspectionScenario CurrentScenario { get; private set; } = InspectionScenario.Normal;

        public string CurrentScenarioFileLabel => CurrentScenario switch
        {
            InspectionScenario.WheelMissing => "wheel_missing",
            InspectionScenario.HeadlightMissing => "headlight_missing",
            InspectionScenario.DoorMisaligned => "door_misaligned",
            InspectionScenario.Uncertain => "uncertain",
            _ => "normal"
        };

        private void Start()
        {
            if (vehicleController == null)
            {
                vehicleController = FindFirstObjectByType<VehicleLineController>();
            }
            agentBridge = FindFirstObjectByType<UnityAgentBridge>();

            if (vehicleController == null)
            {
                Debug.LogError("[AutoMate] VehicleLineController was not found.");
                return;
            }

            Transform vehicle = vehicleController.transform;
            frontLeftWheel = vehicle.Find("Front Left Wheel");
            frontLeftHeadlight = vehicle.Find("Front Left Headlight");
            leftDoor = vehicle.Find("Left Door");

            if (leftDoor != null)
            {
                doorNormalPosition = leftDoor.localPosition;
                doorNormalRotation = leftDoor.localRotation;
            }

            BuildCameraOccluder();
            ApplyScenario(InspectionScenario.Normal, false);
        }

        public void SelectNormal() => ApplyScenario(InspectionScenario.Normal, true);
        public void SelectWheelMissing() => ApplyScenario(InspectionScenario.WheelMissing, true);
        public void SelectHeadlightMissing() => ApplyScenario(InspectionScenario.HeadlightMissing, true);
        public void SelectDoorMisaligned() => ApplyScenario(InspectionScenario.DoorMisaligned, true);
        public void SelectUncertain() => ApplyScenario(InspectionScenario.Uncertain, true);

        private void ApplyScenario(InspectionScenario scenario, bool restartLine)
        {
            CurrentScenario = scenario;
            if (agentBridge == null)
            {
                agentBridge = FindFirstObjectByType<UnityAgentBridge>();
            }
            agentBridge?.ResetForNewInspection();

            if (frontLeftWheel != null)
            {
                frontLeftWheel.gameObject.SetActive(scenario != InspectionScenario.WheelMissing);
            }
            if (frontLeftHeadlight != null)
            {
                frontLeftHeadlight.gameObject.SetActive(scenario != InspectionScenario.HeadlightMissing);
            }
            if (leftDoor != null)
            {
                leftDoor.localPosition = doorNormalPosition;
                leftDoor.localRotation = doorNormalRotation;
                if (scenario == InspectionScenario.DoorMisaligned)
                {
                    leftDoor.localPosition += new Vector3(0.18f, 0.18f, -0.38f);
                    leftDoor.localRotation = Quaternion.Euler(0f, 18f, -8f);
                }
            }
            if (cameraOccluder != null)
            {
                cameraOccluder.SetActive(scenario == InspectionScenario.Uncertain);
            }

            if (restartLine)
            {
                vehicleController.ResetVehicle();
                Debug.Log($"[AutoMate] Scenario selected: {CurrentScenarioFileLabel}");
            }
        }

        private void BuildCameraOccluder()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            cameraOccluder = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cameraOccluder.name = "Uncertain Camera Obstruction";
            cameraOccluder.transform.SetParent(camera.transform, false);
            cameraOccluder.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            cameraOccluder.transform.localRotation = Quaternion.identity;
            cameraOccluder.transform.localScale = new Vector3(0.62f, 0.36f, 1f);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color");
            Material material = new Material(shader);
            Color obstructionColor = new Color(0.025f, 0.03f, 0.035f, 1f);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", obstructionColor);
            }
            material.color = obstructionColor;
            cameraOccluder.GetComponent<Renderer>().material = material;
            cameraOccluder.SetActive(false);
        }

        private void OnGUI()
        {
            const float width = 250f;
            const float buttonHeight = 32f;
            float panelHeight = 238f;
            float x = 18f;
            float y = Screen.height - panelHeight - 18f;

            GUI.Box(new Rect(x, y, width, panelHeight), "AutoMate Test Scenario");
            GUI.Label(new Rect(x + 14f, y + 28f, width - 28f, 24f), $"현재: {CurrentScenarioFileLabel}");

            float buttonY = y + 55f;
            if (GUI.Button(new Rect(x + 12f, buttonY, width - 24f, buttonHeight), "TC01 · NORMAL"))
                SelectNormal();
            if (GUI.Button(new Rect(x + 12f, buttonY + 36f, width - 24f, buttonHeight), "TC02 · WHEEL MISSING"))
                SelectWheelMissing();
            if (GUI.Button(new Rect(x + 12f, buttonY + 72f, width - 24f, buttonHeight), "TC03 · HEADLIGHT MISSING"))
                SelectHeadlightMissing();
            if (GUI.Button(new Rect(x + 12f, buttonY + 108f, width - 24f, buttonHeight), "TC04 · DOOR MISALIGNED"))
                SelectDoorMisaligned();
            if (GUI.Button(new Rect(x + 12f, buttonY + 144f, width - 24f, buttonHeight), "TC05 · UNCERTAIN CAMERA"))
                SelectUncertain();
        }
    }
}
