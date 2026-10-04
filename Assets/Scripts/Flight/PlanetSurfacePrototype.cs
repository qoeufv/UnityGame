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
        private Transform dockedShip;
        private Transform oceanSurface;
        private Material oceanMaterial;
        private readonly List<Transform> scanMarkers = new List<Transform>();
        private readonly List<Renderer> scanMarkerRenderers = new List<Renderer>();
        private const float DockInteractionDistance = 9.0f;
        internal static int ReturnTargetIndex = -1;
        internal static bool ReturningFromSurface;

        private void Start()
        {
            if (planet == null) planet = Resources.Load<PlanetData>("Planets/AzureWorld");
            BuildSurface();
            var playerObject = new GameObject("Surface Explorer", typeof(CharacterController), typeof(PlanetSurfaceWalker));
            playerObject.transform.position = new Vector3(0, .72f, -5.0f); player = playerObject.transform;
            viewPivot = new GameObject("First Person View").transform; viewPivot.SetParent(player, false); viewPivot.localPosition = Vector3.up * 1.55f;
            walker = player.GetComponent<PlanetSurfaceWalker>();
            walker.viewPivot = viewPivot;
            walker.platformHalfExtents = new Vector2(14f, 10.5f);
            walker.interactionDistance = 8.5f;
            walker.swimmingEnabled = false;
            walker.ResetLook(0f);
            viewPivot.localRotation = Quaternion.identity;
            var cameraObject = new GameObject("Surface Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(viewPivot, false); cameraObject.transform.localPosition = Vector3.zero;
            view = cameraObject.GetComponent<Camera>();
            view.tag = "MainCamera"; view.fieldOfView = normalFieldOfView; view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(.05f, .22f, .30f);
            // Keep the ship visible from the landing pad while leaving the player a clear walking start.
            dockedShip = BuildDockedShip(new Vector3(-3.6f, .78f, -4.0f));
            BuildHud();
        }

        private void Update()
        {
            AnimateWorld();
            var keys = Keyboard.current;
            if (keys != null && keys.escapeKey.wasPressedThisFrame) ReturnToOrbit();
            if (keys != null && keys.fKey.wasPressedThisFrame)
            {
                if (IsNearDockedShip()) ReturnToOrbit();
                else TryScan();
            }
            if (walker != null && view != null)
            {
                bool nearShip = IsNearDockedShip();
                view.fieldOfView = Mathf.Lerp(view.fieldOfView, normalFieldOfView, Time.deltaTime * 5f);
                view.backgroundColor = new Color(.05f, .22f, .30f);
                RenderSettings.fogColor = new Color(.04f, .16f, .20f);
                RenderSettings.fogDensity = .008f;
                if (hudText != null)
                {
                    string state = nearShip ? "DOCKED SHIP  /  F RETURN TO ORBIT" : "ISLAND SURFACE  /  OCEAN OBSERVATION ONLY";
                    string prompt = Time.unscaledTime < scanMessageUntil ? scanMessage : nearShip ? "F  Return to near orbit" : GetScanPrompt();
                    string mission = explorationComplete ? "MISSION COMPLETE  /  Azure World survey uploaded" : "MISSION  /  Scan all four marked structures";
                    hudText.text = $"{planet.displayName.ToUpperInvariant()}  /  {planet.environment}\n{planet.description}\nWASD  Walk    Mouse  Look around    SPACE  Jump    F  Scan / Return    ESC  Return to orbit\n{state}    Discoveries {scanned.Count}/{scanTargets.Count}\n{mission}\n{prompt}";
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

        private bool IsNearDockedShip()
        {
            if (dockedShip == null || player == null) return false;
            Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
            Vector2 shipXZ = new Vector2(dockedShip.position.x, dockedShip.position.z);
            return Vector2.Distance(playerXZ, shipXZ) <= DockInteractionDistance;
        }

        private void ReturnToOrbit()
        {
            ReturnTargetIndex = planet != null && planet.displayName.ToLowerInvariant().Contains("moon") ? 3 : 1;
            ReturningFromSurface = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("SpaceFlightPrototype");
        }

        private Transform BuildDockedShip(Vector3 position)
        {
            var root = new GameObject("Player Ship / Docked at Azure World").transform;
            root.position = position;
            var hull = Material(new Color(.28f, .38f, .46f));
            var glass = Material(new Color(.03f, .20f, .25f));
            var glow = Material(new Color(.08f, .75f, .9f));
            DockPart("Docked ship hull", PrimitiveType.Capsule, root, Vector3.zero, new Vector3(1.8f, .75f, 1.25f), Quaternion.Euler(90, 0, 0), hull);
            DockPart("Docked ship canopy", PrimitiveType.Sphere, root, new Vector3(0, .34f, .20f), new Vector3(.9f, .38f, .8f), Quaternion.identity, glass);
            DockPart("Docked ship port wing", PrimitiveType.Cube, root, new Vector3(-1.35f, -.12f, -.25f), new Vector3(1.4f, .10f, .65f), Quaternion.Euler(0, -12, 0), hull);
            DockPart("Docked ship starboard wing", PrimitiveType.Cube, root, new Vector3(1.35f, -.12f, -.25f), new Vector3(1.4f, .10f, .65f), Quaternion.Euler(0, 12, 0), hull);
            DockPart("Docked ship beacon", PrimitiveType.Cylinder, root, new Vector3(0, .78f, -.25f), new Vector3(.12f, .08f, .12f), Quaternion.identity, glow);
            return root;
        }

        private GameObject DockPart(string label, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = label;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.transform.localRotation = rotation;
            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>();
            if (collider != null) SafeDestroy(collider);
            return part;
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

            BuildIslandTerrain();

            var water = GameObject.CreatePrimitive(PrimitiveType.Plane); water.name = "Ocean Surface / Azure World Sea"; water.transform.position = new Vector3(0, -.08f, 0); water.transform.localScale = Vector3.one * 160f;
            var waterMaterial = Material(new Color(.015f, .22f, .30f)); waterMaterial.SetFloat("_Metallic", .15f); waterMaterial.SetFloat("_Smoothness", .9f); water.GetComponent<Renderer>().sharedMaterial = waterMaterial;
            oceanSurface = water.transform; oceanMaterial = waterMaterial;
            DestroyWaterCollider(water);

            // Place models upright, scaled to grand structure sizes, resting flush on the platform
            scanTargets.Add(PlaceModel(researchBuilding, new Vector3(-6.8f, .59f, 4.7f), 4.8f, "Research Laboratory").transform);
            scanTargets.Add(PlaceModel(residentialHabitat, new Vector3(6.8f, .59f, 4.7f), 4.8f, "Residential Habitat").transform);
            scanTargets.Add(PlaceModel(communicationTower, new Vector3(0, .59f, 8.7f), 11.5f, "Communication Tower").transform);
            scanTargets.Add(PlaceModel(landingPlatform, new Vector3(0, .59f, -6.4f), 2.2f, "Landing Platform").transform);
            BuildShorelineBand();
            BuildScanMarkers();

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

        private void BuildIslandTerrain()
        {
            // A single irregular island replaces the old cylinder/deck stack. The
            // outer ring is lower than the center so the sea reads as surrounding
            // coastline from the first-person camera.
            const int segments = 20;
            const int rings = 3;
            var vertices = new Vector3[1 + segments * rings + segments];
            var triangles = new List<int>();
            vertices[0] = new Vector3(0f, .78f, 1.5f);
            for (int ring = 0; ring < rings; ring++)
            {
                float radius = ring == 0 ? .58f : ring == 1 ? .82f : 1f;
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float shape = 1f + .10f * Mathf.Sin(i * 2.3f) + .06f * Mathf.Cos(i * 4.1f);
                    float x = Mathf.Cos(angle) * 15.2f * radius * shape;
                    float z = Mathf.Sin(angle) * 11.6f * radius * shape + 1.5f;
                    float height = ring == 0 ? .72f : ring == 1 ? .58f + .08f * Mathf.Sin(i * 1.7f) : .24f + .10f * Mathf.Sin(i * 2.1f);
                    vertices[1 + ring * segments + i] = new Vector3(x, height, z);
                }
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(0); triangles.Add(1 + next); triangles.Add(1 + i);
                for (int ring = 0; ring < rings - 1; ring++)
                {
                    int a = 1 + ring * segments + i;
                    int b = 1 + ring * segments + next;
                    int c = 1 + (ring + 1) * segments + i;
                    int d = 1 + (ring + 1) * segments + next;
                    triangles.Add(a); triangles.Add(b); triangles.Add(c);
                    triangles.Add(b); triangles.Add(d); triangles.Add(c);
                }
                int outer = 1 + (rings - 1) * segments + i;
                int outerNext = 1 + (rings - 1) * segments + next;
                int bottom = 1 + rings * segments + i;
                int bottomNext = 1 + rings * segments + next;
                vertices[bottom] = new Vector3(vertices[outer].x, -.65f, vertices[outer].z);
                vertices[bottomNext] = new Vector3(vertices[outerNext].x, -.65f, vertices[outerNext].z);
                triangles.Add(outer); triangles.Add(bottom); triangles.Add(outerNext);
                triangles.Add(outerNext); triangles.Add(bottom); triangles.Add(bottomNext);
            }
            var mesh = new Mesh { name = "Azure World Island Terrain" };
            mesh.vertices = vertices; mesh.triangles = triangles.ToArray(); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var island = new GameObject("Azure World Island / Uneven Coastline");
            island.AddComponent<MeshFilter>().sharedMesh = mesh;
            island.AddComponent<MeshRenderer>().sharedMaterial = Material(new Color(.18f, .13f, .10f));
            island.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private void BuildShorelineBand()
        {
            const int segments = 20;
            var vertices = new Vector3[segments * 2];
            var triangles = new int[segments * 6];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float shape = 1f + .10f * Mathf.Sin(i * 2.3f) + .06f * Mathf.Cos(i * 4.1f);
                float innerX = Mathf.Cos(angle) * 14.0f * shape;
                float innerZ = Mathf.Sin(angle) * 10.6f * shape + 1.5f;
                float outerX = Mathf.Cos(angle) * 15.8f * shape;
                float outerZ = Mathf.Sin(angle) * 12.1f * shape + 1.5f;
                vertices[i] = new Vector3(innerX, .29f, innerZ);
                vertices[segments + i] = new Vector3(outerX, .18f, outerZ);
                int next = (i + 1) % segments;
                int t = i * 6;
                triangles[t] = i; triangles[t + 1] = segments + i; triangles[t + 2] = next;
                triangles[t + 3] = next; triangles[t + 4] = segments + i; triangles[t + 5] = segments + next;
            }
            var mesh = new Mesh { name = "Azure World Shoreline Band" };
            mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
            var shore = new GameObject("Azure World Shoreline / Low Coast");
            shore.AddComponent<MeshFilter>().sharedMesh = mesh;
            shore.AddComponent<MeshRenderer>().sharedMaterial = Material(new Color(.30f, .22f, .14f));
        }

        private void BuildScanMarkers()
        {
            var markerMaterial = new Color(.16f, .86f, .95f);
            for (int i = 0; i < scanTargets.Count; i++)
            {
                var target = scanTargets[i];
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "Scan Marker / " + target.name;
                marker.transform.position = target.position + Vector3.up * (i == 2 ? 11.8f : 3.0f);
                marker.transform.localScale = new Vector3(.16f, .05f, .16f);
                var col = marker.GetComponent<Collider>(); if (col != null) SafeDestroy(col);
                var renderer = marker.GetComponent<Renderer>(); renderer.sharedMaterial = Material(markerMaterial);
                scanMarkers.Add(marker.transform); scanMarkerRenderers.Add(renderer);
            }
        }

        private void AnimateWorld()
        {
            if (oceanSurface != null)
            {
                float wave = Mathf.Sin(Time.time * .8f) * .012f;
                oceanSurface.position = new Vector3(0f, -.08f + wave, 0f);
                if (oceanMaterial != null) oceanMaterial.SetColor("_BaseColor", Color.Lerp(new Color(.012f, .19f, .27f), new Color(.02f, .28f, .36f), (Mathf.Sin(Time.time * .55f) + 1f) * .5f));
            }
            for (int i = 0; i < scanMarkers.Count; i++)
            {
                float pulse = 1f + .16f * Mathf.Sin(Time.time * 2.2f + i);
                scanMarkers[i].localScale = new Vector3(.16f * pulse, .05f, .16f * pulse);
                if (scanMarkerRenderers[i] != null) scanMarkerRenderers[i].sharedMaterial.SetColor("_BaseColor", Color.Lerp(new Color(.05f, .55f, .65f), new Color(.2f, 1f, 1f), (Mathf.Sin(Time.time * 2.2f + i) + 1f) * .5f));
            }
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

            // Imported building colliders are intentionally removed for this exploration slice.
            // The structures remain visible scan targets, while the island terrain owns the
            // walkable collision so the player is not trapped by invisible model bounds.
            foreach (var col in modelInstance.GetComponentsInChildren<Collider>()) SafeDestroy(col);

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
            hudText.text = $"{planet.displayName.ToUpperInvariant()}  /  {planet.environment}\n{planet.description}\nWASD  Walk    Mouse  Look around    SPACE  Jump    F  Scan / Return    ESC  Return to orbit\nISLAND SURFACE / OCEAN OBSERVATION ONLY";
        }
    }
}
