using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace AutoMate.Simulation
{
    public sealed class UnityCaptureBridge : MonoBehaviour
    {
        [SerializeField] private VehicleLineController vehicle;
        [SerializeField] private Camera inspectionCamera;
        [SerializeField] private DefectScenarioController scenarioController;
        [SerializeField] private int captureWidth = 1280;
        [SerializeField] private int captureHeight = 720;

        private bool capturedAtCurrentStop;
        private bool captureInProgress;
        private string lastCapturePath = "아직 촬영되지 않음";

        private void Start()
        {
            if (vehicle == null)
            {
                vehicle = FindFirstObjectByType<VehicleLineController>();
            }
            if (inspectionCamera == null)
            {
                inspectionCamera = Camera.main;
            }
            if (scenarioController == null)
            {
                scenarioController = FindFirstObjectByType<DefectScenarioController>();
            }
        }

        private void Update()
        {
            if (vehicle == null)
            {
                return;
            }

            if (vehicle.WaitingForInspection && !capturedAtCurrentStop && !captureInProgress)
            {
                StartCoroutine(CaptureInspectionImage());
            }

            if (!vehicle.WaitingForInspection)
            {
                capturedAtCurrentStop = false;
            }
        }

        private IEnumerator CaptureInspectionImage()
        {
            captureInProgress = true;
            yield return new WaitForEndOfFrame();

            string outputDirectory = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "..", "unity_captures")
            );
            Directory.CreateDirectory(outputDirectory);

            string timestamp = System.DateTime.Now.ToString(
                "yyyyMMdd_HHmmss_fff",
                CultureInfo.InvariantCulture
            );
            string scenarioLabel = scenarioController == null
                ? "unlabeled"
                : scenarioController.CurrentScenarioFileLabel;
            string imagePath = Path.Combine(
                outputDirectory,
                $"unity_{scenarioLabel}_{timestamp}.png"
            );

            if (inspectionCamera == null)
            {
                lastCapturePath = "검사 카메라를 찾을 수 없음";
                captureInProgress = false;
                Debug.LogError("[AutoMate] Main inspection camera was not found.");
                yield break;
            }

            RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            Texture2D capturedImage = new Texture2D(
                captureWidth,
                captureHeight,
                TextureFormat.RGB24,
                false
            );
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = inspectionCamera.targetTexture;

            inspectionCamera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            inspectionCamera.Render();
            capturedImage.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            capturedImage.Apply();

            inspectionCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            File.WriteAllBytes(imagePath, capturedImage.EncodeToPNG());

            Destroy(renderTexture);
            Destroy(capturedImage);

            lastCapturePath = imagePath;
            capturedAtCurrentStop = true;
            Debug.Log($"[AutoMate] Inspection image saved: {imagePath}");

            captureInProgress = false;
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(18f, 18f, 470f, 78f), "AutoMate · Unity Inspection Camera");
            GUI.Label(
                new Rect(34f, 47f, 440f, 42f),
                vehicle != null && vehicle.WaitingForInspection
                    ? $"검사 위치 정지 · 캡처: {Path.GetFileName(lastCapturePath)}"
                    : "차량이 AI 검사 구역으로 이동 중입니다."
            );
        }
    }
}
