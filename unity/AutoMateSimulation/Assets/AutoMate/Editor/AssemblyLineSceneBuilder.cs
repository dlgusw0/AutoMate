using System.IO;
using AutoMate.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoMate.Editor
{
    public static class AssemblyLineSceneBuilder
    {
        private const string ScenePath = "Assets/AutoMate/Scenes/AssemblyLine.unity";
        private const string MaterialFolder = "Assets/AutoMate/Materials/Generated";

        [MenuItem("AutoMate/Build Assembly Line Demo")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/AutoMate/Materials", "Generated");

            Material floorMaterial = CreateMaterial("FactoryFloor", new Color(0.13f, 0.16f, 0.19f), 0.15f, 0.42f);
            Material beltMaterial = CreateMaterial("ConveyorBelt", new Color(0.055f, 0.07f, 0.085f), 0.72f, 0.35f);
            Material metalMaterial = CreateMaterial("IndustrialMetal", new Color(0.28f, 0.33f, 0.38f), 0.8f, 0.55f);
            Material safetyMaterial = CreateMaterial("SafetyYellow", new Color(1f, 0.56f, 0.04f), 0.25f, 0.48f);
            Material cyanMaterial = CreateMaterial("ScannerCyan", new Color(0.02f, 0.62f, 0.88f), 0.3f, 0.62f, true);
            Material carMaterial = CreateMaterial("VehicleBlue", new Color(0.025f, 0.19f, 0.38f), 0.82f, 0.72f);
            Material glassMaterial = CreateMaterial("VehicleGlass", new Color(0.035f, 0.08f, 0.12f), 0.65f, 0.88f);
            Material tireMaterial = CreateMaterial("Tire", new Color(0.018f, 0.02f, 0.024f), 0.05f, 0.18f);
            Material headlightMaterial = CreateMaterial("Headlight", new Color(0.72f, 0.9f, 1f), 0.2f, 0.92f, true);
            Material redMaterial = CreateMaterial("StatusRed", new Color(0.82f, 0.035f, 0.025f), 0.1f, 0.55f, true);
            Material greenMaterial = CreateMaterial("StatusGreen", new Color(0.02f, 0.72f, 0.2f), 0.1f, 0.55f, true);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "AssemblyLine";

            GameObject environment = new GameObject("Environment");
            CreateCube("Factory Floor", environment.transform, new Vector3(0f, -0.2f, 0f), new Vector3(32f, 0.4f, 20f), floorMaterial);
            CreateCube("Back Wall", environment.transform, new Vector3(0f, 4f, 8.5f), new Vector3(32f, 8f, 0.35f), metalMaterial);

            GameObject conveyor = new GameObject("Conveyor Line");
            CreateCube("Belt", conveyor.transform, new Vector3(0f, 0.58f, 0f), new Vector3(22f, 0.28f, 4.2f), beltMaterial);
            CreateCube("Left Safety Rail", conveyor.transform, new Vector3(0f, 1.05f, 2.18f), new Vector3(22f, 0.18f, 0.18f), safetyMaterial);
            CreateCube("Right Safety Rail", conveyor.transform, new Vector3(0f, 0.32f, -2.18f), new Vector3(22f, 0.18f, 0.18f), safetyMaterial);

            for (int index = -10; index <= 10; index++)
            {
                GameObject roller = CreateCylinder(
                    $"Roller {index + 11:00}",
                    conveyor.transform,
                    new Vector3(index, 0.78f, 0f),
                    new Vector3(0.22f, 1.92f, 0.22f),
                    metalMaterial
                );
                roller.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }

            for (int index = -9; index <= 9; index += 3)
            {
                CreateCube($"Support L {index}", conveyor.transform, new Vector3(index, 0.15f, 1.65f), new Vector3(0.28f, 1.3f, 0.28f), metalMaterial);
                CreateCube($"Support R {index}", conveyor.transform, new Vector3(index, 0.15f, -1.65f), new Vector3(0.28f, 1.3f, 0.28f), metalMaterial);
            }

            GameObject inspectionStation = new GameObject("AI Inspection Station");
            CreateCube("Inspection Zone", inspectionStation.transform, new Vector3(0f, 0.79f, 0f), new Vector3(4.6f, 0.05f, 4f), cyanMaterial);
            CreateCube("Gate Left", inspectionStation.transform, new Vector3(0f, 3.2f, 2.65f), new Vector3(0.45f, 5.2f, 0.45f), safetyMaterial);
            CreateCube("Gate Right", inspectionStation.transform, new Vector3(0f, 3.2f, -2.65f), new Vector3(0.45f, 5.2f, 0.45f), safetyMaterial);
            CreateCube("Gate Top", inspectionStation.transform, new Vector3(0f, 5.7f, 0f), new Vector3(0.5f, 0.5f, 5.75f), safetyMaterial);
            CreateCube("Scanner Left", inspectionStation.transform, new Vector3(0f, 3.25f, 2.38f), new Vector3(0.18f, 2.4f, 0.12f), cyanMaterial);
            CreateCube("Scanner Right", inspectionStation.transform, new Vector3(0f, 3.25f, -2.38f), new Vector3(0.18f, 2.4f, 0.12f), cyanMaterial);

            GameObject statusTower = new GameObject("Status Tower");
            statusTower.transform.SetParent(inspectionStation.transform);
            CreateCube("Tower Pole", statusTower.transform, new Vector3(0.7f, 4.7f, -3.05f), new Vector3(0.18f, 2.4f, 0.18f), metalMaterial);
            CreateSphere("Green Light", statusTower.transform, new Vector3(0.7f, 5.45f, -3.05f), new Vector3(0.42f, 0.42f, 0.42f), greenMaterial);
            CreateSphere("Red Light", statusTower.transform, new Vector3(0.7f, 6.0f, -3.05f), new Vector3(0.42f, 0.42f, 0.42f), redMaterial);

            GameObject vehicle = BuildVehicle(carMaterial, glassMaterial, tireMaterial, metalMaterial, headlightMaterial);
            vehicle.transform.position = new Vector3(-8f, 1.2f, 0f);
            VehicleLineController vehicleController = vehicle.AddComponent<VehicleLineController>();

            BuildLighting();
            BuildCamera();

            GameObject bridgeObject = new GameObject("AutoMate Capture Bridge");
            DefectScenarioController scenarioController = bridgeObject.AddComponent<DefectScenarioController>();
            UnityCaptureBridge captureBridge = bridgeObject.AddComponent<UnityCaptureBridge>();
            UnityAgentBridge agentBridge = bridgeObject.AddComponent<UnityAgentBridge>();
            SerializedObject serializedScenario = new SerializedObject(scenarioController);
            serializedScenario.FindProperty("vehicleController").objectReferenceValue = vehicleController;
            serializedScenario.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject serializedBridge = new SerializedObject(captureBridge);
            serializedBridge.FindProperty("vehicle").objectReferenceValue = vehicleController;
            serializedBridge.FindProperty("scenarioController").objectReferenceValue = scenarioController;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject serializedAgentBridge = new SerializedObject(agentBridge);
            serializedAgentBridge.FindProperty("vehicle").objectReferenceValue = vehicleController;
            serializedAgentBridge.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = inspectionStation;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AutoMate] AssemblyLine scene created successfully.");
        }

        private static GameObject BuildVehicle(Material body, Material glass, Material tire, Material metal, Material headlight)
        {
            GameObject vehicle = new GameObject("Demo Vehicle");
            CreateCube("Lower Body", vehicle.transform, new Vector3(0f, 0f, 0f), new Vector3(4.9f, 0.85f, 2.3f), body);
            CreateCube("Upper Body", vehicle.transform, new Vector3(-0.35f, 0.7f, 0f), new Vector3(2.75f, 0.72f, 2.05f), glass);
            CreateCube("Front Hood", vehicle.transform, new Vector3(1.65f, 0.52f, 0f), new Vector3(1.55f, 0.18f, 2.15f), body);
            CreateCube("Front Bumper", vehicle.transform, new Vector3(2.55f, -0.12f, 0f), new Vector3(0.22f, 0.45f, 2.25f), metal);
            CreateCube("Left Door", vehicle.transform, new Vector3(-0.35f, 0.2f, -1.18f), new Vector3(1.62f, 0.88f, 0.12f), body);
            CreateCube("Right Door", vehicle.transform, new Vector3(-0.35f, 0.2f, 1.18f), new Vector3(1.62f, 0.88f, 0.12f), body);
            CreateCube("Front Left Headlight Housing", vehicle.transform, new Vector3(2.69f, 0.12f, -0.72f), new Vector3(0.14f, 0.46f, 0.72f), tire);
            CreateCube("Front Right Headlight Housing", vehicle.transform, new Vector3(2.69f, 0.12f, 0.72f), new Vector3(0.14f, 0.46f, 0.72f), tire);
            CreateCube("Front Left Headlight", vehicle.transform, new Vector3(2.78f, 0.12f, -0.72f), new Vector3(0.1f, 0.3f, 0.5f), headlight);
            CreateCube("Front Right Headlight", vehicle.transform, new Vector3(2.78f, 0.12f, 0.72f), new Vector3(0.1f, 0.3f, 0.5f), headlight);

            float[] wheelX = { -1.55f, 1.55f };
            float[] wheelZ = { -1.18f, 1.18f };
            foreach (float x in wheelX)
            {
                foreach (float z in wheelZ)
                {
                    string longitudinal = x > 0f ? "Front" : "Rear";
                    string side = z < 0f ? "Left" : "Right";
                    BuildWheel(
                        $"{longitudinal} {side} Wheel",
                        vehicle.transform,
                        new Vector3(x, -0.35f, z),
                        z < 0f,
                        tire,
                        metal
                    );
                    GameObject axleHub = CreateCylinder(
                        $"{longitudinal} {side} Axle Hub",
                        vehicle.transform,
                        new Vector3(x, -0.35f, z),
                        new Vector3(0.3f, 0.12f, 0.3f),
                        metal
                    );
                    axleHub.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }

            return vehicle;
        }

        private static void BuildLighting()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light directional = lightObject.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = 0.82f;
            directional.color = new Color(0.88f, 0.94f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(42f, -34f, 0f);

            for (int index = -2; index <= 2; index++)
            {
                GameObject areaObject = new GameObject($"Factory Light {index + 3}");
                Light point = areaObject.AddComponent<Light>();
                point.type = LightType.Point;
                point.range = 13f;
                point.intensity = 120f;
                point.color = new Color(0.72f, 0.88f, 1f);
                areaObject.transform.position = new Vector3(index * 4.5f, 6.5f, -1f);
            }
        }

        private static void BuildCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.fieldOfView = 32f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.transform.position = new Vector3(6.3f, 2.75f, -5.8f);
            cameraObject.transform.LookAt(new Vector3(0.35f, 1.12f, 0f));
        }

        private static GameObject BuildWheel(
            string name,
            Transform parent,
            Vector3 position,
            bool cameraFacing,
            Material tire,
            Material metal
        )
        {
            GameObject assembly = new GameObject(name);
            assembly.transform.SetParent(parent);
            assembly.transform.position = position;

            GameObject tireObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tireObject.name = "Tire";
            tireObject.transform.SetParent(assembly.transform, false);
            tireObject.transform.localScale = new Vector3(0.72f, 0.3f, 0.72f);
            tireObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tireObject.GetComponent<Renderer>().sharedMaterial = tire;

            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Silver Hub";
            hub.transform.SetParent(assembly.transform, false);
            hub.transform.localPosition = new Vector3(0f, 0f, cameraFacing ? -0.31f : 0.31f);
            hub.transform.localScale = new Vector3(0.34f, 0.08f, 0.34f);
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hub.GetComponent<Renderer>().sharedMaterial = metal;
            return assembly;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
            return gameObject;
        }

        private static GameObject CreateCylinder(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
            return gameObject;
        }

        private static GameObject CreateSphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
            return gameObject;
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness, bool emission = false)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            material.color = color;
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string parent, string name)
        {
            string fullPath = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
