using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

namespace StrategyRPG.Flight
{
    /// <summary>Small data-driven landing scene. Replace generated primitives with authored content later.</summary>
    public sealed class PlanetSurfacePrototype : MonoBehaviour
    {
        [System.Serializable] public sealed class SurfaceModelAsset { public GameObject model; public Texture2D baseColor, normal, metallic, roughness; }
        public PlanetData planet;
        public SurfaceModelAsset researchBuilding = new SurfaceModelAsset();
        public SurfaceModelAsset residentialHabitat = new SurfaceModelAsset();
        public SurfaceModelAsset communicationTower = new SurfaceModelAsset();
        public SurfaceModelAsset landingPlatform = new SurfaceModelAsset();
        [Header("Camera & Immersion")]
        public float normalFieldOfView = 44f;
        public float underwaterFieldOfView = 52f;
        private Camera view;
        private Transform player;
        private Transform viewPivot;
        private float yaw;
        private PlanetSurfaceWalker walker;
        private UnityEngine.UI.Text hudText;
        private readonly List<Transform> scanTargets = new List<Transform>();
        private readonly HashSet<int> scanned = new HashSet<int>();
        private string scanMessage = "Explore the platform and scan a structure.";
        private float scanMessageUntil;
        private bool explorationComplete;

        private void Start()
        {
            if (planet == null) planet = Resources.Load<PlanetData>("Planets/AzureWorld");
            BuildSurface();
            var playerObject = new GameObject("Surface Explorer", typeof(CharacterController), typeof(PlanetSurfaceWalker));
            playerObject.transform.position = new Vector3(0, .72f, -2.5f); player = playerObject.transform;
            viewPivot = new GameObject("First Person View").transform; viewPivot.SetParent(player, false); viewPivot.localPosition = Vector3.up * 1.55f;
            walker = player.GetComponent<PlanetSurfaceWalker>();
            walker.viewPivot = viewPivot;
            walker.platformHalfExtents = new Vector2(18f, 16f);
            walker.interactionDistance = 8.5f;
            walker.ResetLook(0f);
            viewPivot.localRotation = Quaternion.identity;
            var cameraObject = new GameObject("Surface Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(viewPivot, false); cameraObject.transform.localPosition = Vector3.zero;
            view = cameraObject.GetComponent<Camera>();
            view.tag = "MainCamera"; view.fieldOfView = normalFieldOfView; view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(.05f, .22f, .30f);
            BuildHud();
        }

        private void Update()
        {
            var keys = Keyboard.current;
            if (keys != null && keys.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; SceneManager.LoadScene("SpaceFlightPrototype"); }
            if (keys != null && keys.fKey.wasPressedThisFrame) TryScan();
            if (walker != null && view != null)
            {
                bool underwater = walker.IsUnderwater;
                view.fieldOfView = Mathf.Lerp(view.fieldOfView, underwater ? underwaterFieldOfView : normalFieldOfView, Time.deltaTime * 5f);
                view.backgroundColor = underwater ? new Color(.005f, .06f, .09f) : new Color(.05f, .22f, .30f);
                RenderSettings.fogColor = underwater ? new Color(.005f, .12f, .16f) : new Color(.04f, .16f, .20f);
                RenderSettings.fogDensity = underwater ? .08f : .008f;
                if (hudText != null)
                {
                    string state = underwater ? $"UNDERWATER  DEPTH {walker.DepthBelowWater:0.0}m  SPACE ASCEND  CTRL DIVE" : "PLATFORM / SHALLOW WATER";
                    string prompt = Time.unscaledTime < scanMessageUntil ? scanMessage : GetScanPrompt();
                    string mission = explorationComplete ? "MISSION COMPLETE  /  Azure World survey uploaded" : "MISSION  /  Scan all four marked structures";
                    hudText.text = $"{planet.displayName.ToUpperInvariant()}  /  {planet.environment}\n{planet.description}\nWASD  Walk / Swim    Mouse  Look around    SPACE  Swim up    CTRL  Dive    F  Scan    ESC  Return to orbit\n{state}    Discoveries {scanned.Count}/{scanTargets.Count}\n{mission}\n{prompt}";
                }
            }
        }

        private string GetScanPrompt()
        {
            if (walker == null) return string.Empty;
            for (int i = 0; i < scanTargets.Count; i++)
            {
                if (scanned.Contains(i) || !walker.IsWithinInteractionDistance(scanTargets[i])) continue;
                if (HitsTarget(scanTargets[i])) return $"F  Scan {scanTargets[i].name}";
            }
            return "Walk near a structure and look at it to scan.";
        }

        private void TryScan()
        {
            if (walker == null) return;
            for (int i = 0; i < scanTargets.Count; i++)
            {
                if (scanned.Contains(i) || !walker.IsWithinInteractionDistance(scanTargets[i])) continue;
                if (!HitsTarget(scanTargets[i])) continue;
                scanned.Add(i);
                scanMessage = i == 0 ? "Research laboratory scanned: oceanic climate research is active." : i == 1 ? "Residential habitat scanned: 36 crew signatures recorded." : i == 2 ? "Communication tower scanned: an unknown signal is repeating below the sea." : "Landing platform scanned: orbital approach path is clear.";
                if (scanned.Count == scanTargets.Count)
                {
                    explorationComplete = true;
                    scanMessage = "Azure World exploration complete: all four structures have been surveyed.";
                }
                scanMessageUntil = Time.unscaledTime + 6f;
                return;
            }
            scanMessage = "Move closer and look directly at a structure before scanning."; scanMessageUntil = Time.unscaledTime + 4f;
        }

        private bool HitsTarget(Transform target)
        {
            if (walker == null || target == null || !walker.IsWithinInteractionDistance(target)) return false;
            var hits = Physics.RaycastAll(walker.GetLookRay(), walker.interactionDistance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].transform;
                if (hitTransform == target || hitTransform.IsChildOf(target)) return true;
            }
            return false;
        }

        private void BuildSurface()
        {
            RenderSettings.ambientLight = Color.Lerp(planet.secondaryColor, Color.white, .16f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.04f, .16f, .20f); RenderSettings.fogDensity = .008f;
            var sun = new GameObject("Ocean World Sun", typeof(Light)); sun.transform.rotation = Quaternion.Euler(42, -28, 0); var light = sun.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.15f; light.color = new Color(.78f, .9f, 1f);

            // Expansive floating research platform deck
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder); ground.name = "Ocean Research Platform / Main Deck";
            ground.transform.position = new Vector3(0, .15f, 2.0f); ground.transform.localScale = new Vector3(36f, .5f, 30f);
            ground.GetComponent<Renderer>().sharedMaterial = Material(new Color(.12f, .22f, .27f));

            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube); deck.name = "Ocean Research Platform / Rear Campus Deck";
            deck.transform.position = new Vector3(0, .48f, 8.5f); deck.transform.localScale = new Vector3(28f, .16f, 13f);
            deck.GetComponent<Renderer>().sharedMaterial = Material(new Color(.18f, .30f, .34f));

            var padDeck = GameObject.CreatePrimitive(PrimitiveType.Cylinder); padDeck.name = "Ocean Research Platform / South Landing Deck";
            padDeck.transform.position = new Vector3(0, .48f, -7.5f); padDeck.transform.localScale = new Vector3(11f, .16f, 11f);
            padDeck.GetComponent<Renderer>().sharedMaterial = Material(new Color(.16f, .26f, .30f));

            var water = GameObject.CreatePrimitive(PrimitiveType.Plane); water.name = "Ocean Surface / Walkable Water Boundary"; water.transform.position = new Vector3(0, -.08f, 0); water.transform.localScale = Vector3.one * 160f;
            var waterMaterial = Material(new Color(.015f, .22f, .30f)); waterMaterial.SetFloat("_Metallic", .15f); waterMaterial.SetFloat("_Smoothness", .9f); water.GetComponent<Renderer>().sharedMaterial = waterMaterial;
            DestroyWaterCollider(water);

            AddWaterDetail(new Vector3(0, -.045f, 2.0f), 45f, new Color(.03f, .40f, .47f));
            AddWaterDetail(new Vector3(0, -.035f, 2.0f), 28f, new Color(.02f, .28f, .38f));

            // Central boulevard and campus walkways connecting all sectors
            AddWalkway(new Vector3(0, .58f, 0.5f), new Vector3(3.4f, .12f, 10.0f));
            AddWalkway(new Vector3(-4.5f, .59f, 6.5f), new Vector3(6.5f, .12f, 2.8f));
            AddWalkway(new Vector3(4.5f, .59f, 6.5f), new Vector3(6.5f, .12f, 2.8f));
            AddWalkway(new Vector3(0, .59f, 10.5f), new Vector3(2.8f, .12f, 4.5f));

            // Accessible ramp leading from central walkway up onto the landing platform
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube); ramp.name = "Landing Platform Access Ramp";
            ramp.transform.position = new Vector3(0, 1.55f, -5.0f); ramp.transform.localScale = new Vector3(2.8f, .12f, 2.8f);
            ramp.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            ramp.GetComponent<Renderer>().sharedMaterial = Material(new Color(.14f, .20f, .24f));

            // Place models upright, scaled to grand structure sizes, resting flush on the platform
            scanTargets.Add(PlaceModel(researchBuilding, new Vector3(-9.0f, .59f, 6.5f), 4.8f, "Research Laboratory").transform);
            scanTargets.Add(PlaceModel(residentialHabitat, new Vector3(9.0f, .59f, 6.5f), 4.8f, "Residential Habitat").transform);
            scanTargets.Add(PlaceModel(communicationTower, new Vector3(0, .59f, 13.0f), 11.5f, "Communication Tower").transform);
            scanTargets.Add(PlaceModel(landingPlatform, new Vector3(0, .59f, -7.5f), 2.2f, "Landing Platform").transform);

            AddBeacon(new Vector3(0, .72f, 2.0f));
            AddRailings();
            AddSupportPiers();
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private void DestroyWaterCollider(GameObject water)
        {
            var col = water.GetComponent<Collider>();
            if (col != null) SafeDestroy(col);
        }

        private void AddWalkway(Vector3 position, Vector3 scale)
        {
            var walkway = GameObject.CreatePrimitive(PrimitiveType.Cube); walkway.name = "Base Walkway"; walkway.transform.position = position; walkway.transform.localScale = scale;
            walkway.GetComponent<Renderer>().sharedMaterial = Material(new Color(.12f, .18f, .22f));
        }

        private void AddWaterDetail(Vector3 position, float diameter, Color color)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder); ring.name = "Ocean Surface Wake";
            ring.transform.position = position; ring.transform.localScale = new Vector3(diameter, .006f, diameter);
            ring.GetComponent<Renderer>().sharedMaterial = Material(color);
            var col = ring.GetComponent<Collider>();
            if (col != null) SafeDestroy(col);
        }

        private void AddBeacon(Vector3 position)
        {
            var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder); beacon.name = "Research Beacon / Arrival Marker";
            beacon.transform.position = position; beacon.transform.localScale = new Vector3(.22f, .45f, .22f);
            beacon.GetComponent<Renderer>().sharedMaterial = Material(new Color(.2f, .8f, .9f));
            var lamp = beacon.AddComponent<Light>(); lamp.type = LightType.Point; lamp.range = 7f; lamp.intensity = 2.2f; lamp.color = new Color(.1f, .8f, 1f);
        }

        private void AddRailings()
        {
            var railMaterial = Material(new Color(.35f, .62f, .66f));
            // North perimeter behind the structures
            AddRailSegment(new Vector3(-7.5f, .98f, 15.0f), new Vector3(14.0f, 1.0f, .12f), railMaterial);
            AddRailSegment(new Vector3(7.5f, .98f, 15.0f), new Vector3(14.0f, 1.0f, .12f), railMaterial);
            // West perimeter
            AddRailSegment(new Vector3(-15.5f, .98f, 6.0f), new Vector3(.12f, 1.0f, 16.0f), railMaterial);
            // East perimeter
            AddRailSegment(new Vector3(15.5f, .98f, 6.0f), new Vector3(.12f, 1.0f, 16.0f), railMaterial);
            // South perimeter corners (leaving center and dock open for swimming down into ocean)
            AddRailSegment(new Vector3(-9.5f, .98f, -5.5f), new Vector3(8.0f, 1.0f, .12f), railMaterial);
            AddRailSegment(new Vector3(9.5f, .98f, -5.5f), new Vector3(8.0f, 1.0f, .12f), railMaterial);
        }

        private void AddRailSegment(Vector3 position, Vector3 scale, Material material)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube); rail.name = "Platform Safety Railing";
            rail.transform.position = position; rail.transform.localScale = scale; rail.GetComponent<Renderer>().sharedMaterial = material;
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube); top.name = "Platform Safety Handrail";
            top.transform.position = position + Vector3.up * .42f; top.transform.localScale = new Vector3(scale.x + (scale.x < .2f ? .12f : 0), .08f, scale.z + (scale.z < .2f ? .12f : 0)); top.GetComponent<Renderer>().sharedMaterial = material;
        }

        private void AddSupportPiers()
        {
            var supportMaterial = Material(new Color(.06f, .12f, .15f));
            foreach (var x in new[] { -13.0f, -6.0f, 0f, 6.0f, 13.0f })
                foreach (var z in new[] { -7.5f, 2.0f, 10.0f })
                {
                    var pier = GameObject.CreatePrimitive(PrimitiveType.Cylinder); pier.name = "Floating Platform Support";
                    pier.transform.position = new Vector3(x, -1.1f, z); pier.transform.localScale = new Vector3(.55f, 2.8f, .55f); pier.GetComponent<Renderer>().sharedMaterial = supportMaterial;
                }
        }

        private GameObject PlaceModel(SurfaceModelAsset asset, Vector3 position, float targetHeight, string label, float yaw = 0f)
        {
            if (asset == null || asset.model == null) return new GameObject(label + " (Missing)");

            var root = new GameObject(label);
            root.transform.position = position;
            root.transform.rotation = Quaternion.identity;

            // Preserve natural upright orientation (FBX Z-up -> Unity Y-up) without tilting
            Quaternion uprightRot = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(270f, 0f, 0f);
            var modelInstance = Instantiate(asset.model, position, uprightRot, root.transform);
            modelInstance.name = label + "_Visual";

            var renderers = modelInstance.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(modelInstance.transform.position, Vector3.zero);
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            // Scale uniformly so the model achieves the target height (at least human height or larger)
            if (bounds.size.y > 0.01f)
            {
                float factor = targetHeight / bounds.size.y;
                modelInstance.transform.localScale *= factor;
            }

            // Recalculate bounds after scaling
            bounds = new Bounds(modelInstance.transform.position, Vector3.zero);
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            // Flush base of model perfectly onto the platform deck surface (position.y)
            float deltaY = position.y - bounds.min.y;
            modelInstance.transform.position += Vector3.up * deltaY;

            // Recalculate final bounds for collider placement
            bounds = new Bounds(modelInstance.transform.position, Vector3.zero);
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            // Apply URP Lit material with imported PBR textures
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (asset.baseColor != null) material.SetTexture("_BaseMap", asset.baseColor);
            if (asset.normal != null) { material.SetTexture("_BumpMap", asset.normal); material.EnableKeyword("_NORMALMAP"); }
            if (asset.metallic != null) { material.SetTexture("_MetallicGlossMap", asset.metallic); material.EnableKeyword("_METALLICSPECGLOSSMAP"); }
            material.SetFloat("_Smoothness", .48f);
            foreach (var r in renderers) r.sharedMaterial = material;

            // Remove any messy colliders on the FBX export and add a clean, aligned BoxCollider on root
            foreach (var col in modelInstance.GetComponentsInChildren<Collider>()) SafeDestroy(col);
            var box = root.AddComponent<BoxCollider>();
            box.center = root.transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size;

            return root;
        }

        private Material Material(Color color)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = color; return m;
        }

        private void BuildHud()
        {
            var canvas = new GameObject("Surface HUD", typeof(Canvas), typeof(CanvasScaler));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900);
            var textObject = new GameObject("Planet Info", typeof(RectTransform), typeof(UnityEngine.UI.Text)); textObject.transform.SetParent(canvas.transform, false);
            var rect = textObject.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.04f, .74f); rect.anchorMax = new Vector2(.96f, .97f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            hudText = textObject.GetComponent<UnityEngine.UI.Text>(); hudText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); hudText.fontSize = 22; hudText.color = Color.white;
            hudText.text = $"{planet.displayName.ToUpperInvariant()}  /  {planet.environment}\n{planet.description}\nWASD  Walk / Swim    Mouse  Look around    SPACE  Swim up    CTRL  Dive    ESC  Return to orbit\nPLATFORM / SHALLOW WATER";
        }
    }
}
