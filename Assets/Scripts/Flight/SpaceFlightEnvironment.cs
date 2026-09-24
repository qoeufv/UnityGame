using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace StrategyRPG.Flight
{
    /// <summary>Small, deliberately compressed exploration space. Build is explicitly invoked by the scene bootstrap.</summary>
    public sealed class SpaceFlightEnvironment : MonoBehaviour
    {
        public Material litTemplate;
        public Material unlitTemplate;
        public GameObject stargatePrefab;
        public Transform Planet { get; private set; }
        public Transform Moon { get; private set; }
        public Transform Gate { get; private set; }
        public Transform Station { get; private set; }

        private readonly List<Object> ownedAssets = new List<Object>();
        private bool built;

        public void Build()
        {
            if (built) return;
            built = true;
            Material ocean = MakeMaterial("Ocean blue", new Color(.12f, .33f, .47f), false, .24f);
            Material moonRock = MakeMaterial("Moon stone", new Color(.48f, .43f, .34f), false, .1f);
            Material hull = MakeMaterial("Station hull", new Color(.24f, .29f, .35f), false, .35f);
            Material panels = MakeMaterial("Solar panels", new Color(.055f, .16f, .24f), false, .5f);
            Material cyan = MakeMaterial("Navigation cyan", new Color(.19f, .66f, .73f), true);
            Material gold = MakeMaterial("Dust ring gold", new Color(.37f, .32f, .22f), true);
            Planet = Sphere("Pelagic / main planet", new Vector3(80, 15, 150), 45, ocean);
            Moon = Sphere("Aster / moon", new Vector3(-55, 35, 230), 14, moonRock);
            MakeRing(Planet, 57, 65, gold, Quaternion.Euler(22, 0, 12));
            MakeRing(Planet, 67, 69, gold, Quaternion.Euler(22, 0, 12));
            // Fine latitude bands suggest atmosphere and make the sphere's curvature legible.
            Material atmosphere = MakeMaterial("Atmosphere rim detail", new Color(.17f, .46f, .56f), false, .05f);
            for (int i = -2; i <= 2; i++)
            {
                float y = i * 11f;
                Circle(Planet, new Vector3(0, y, 0), Mathf.Sqrt(45.05f * 45.05f - y * y), .2f, atmosphere);
            }

            Gate = NewRoot("Departure Stargate", new Vector3(0, 0, 240));
            if (stargatePrefab != null)
            {
                GameObject visual = Instantiate(stargatePrefab, Gate);
                visual.name = "Stargate visual";
                // Imported prefab is bottom-pivoted. Keep the open passage centered on the flight plane.
                visual.transform.localPosition = new Vector3(0, -12, 0);
                visual.transform.localRotation = Quaternion.Euler(0, 180, 0);
                visual.transform.localScale = Vector3.one * 24;
                foreach (Collider collider in visual.GetComponentsInChildren<Collider>()) collider.enabled = false;
            }
            else
            {
                var fallback = Circle(Gate, Vector3.zero, 10, .9f, hull);
                fallback.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
            var gateLight = Circle(Gate, new Vector3(0, 0, -.2f), 8.7f, .10f, cyan);
            gateLight.transform.localRotation = Quaternion.Euler(90, 0, 0);
            // Rim-only collision: never put a solid collider across the passage.
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2 / 16;
                Transform rim = new GameObject("Gate rim collision").transform;
                rim.SetParent(Gate, false);
                rim.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 10.2f;
                rim.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                rim.gameObject.AddComponent<BoxCollider>().size = new Vector3(2, 4, 2);
            }

            Station = NewRoot("Survey Station", new Vector3(-70, -5, 100));
            Primitive("Habitat", PrimitiveType.Cylinder, Station, Vector3.zero, new Vector3(4, 3, 4), hull);
            Primitive("Port array", PrimitiveType.Cube, Station, new Vector3(-3.5f, 0, 0), new Vector3(3, .25f, 4), panels);
            Primitive("Starboard array", PrimitiveType.Cube, Station, new Vector3(3.5f, 0, 0), new Vector3(3, .25f, 4), panels);
            Primitive("Array spine", PrimitiveType.Cube, Station, Vector3.zero, new Vector3(8, .35f, .4f), hull);
            Primitive("Beacon", PrimitiveType.Sphere, Station, new Vector3(0, 3.3f, 0), Vector3.one * .65f, cyan);
            Circle(Station, new Vector3(0, -2.8f, 0), 3, .08f, cyan);
            BuildStars();

            var sun = new GameObject("Distant warm sun").AddComponent<Light>();
            sun.transform.SetParent(transform, false);
            sun.transform.rotation = Quaternion.Euler(28, -35, 0);
            sun.type = LightType.Directional;
            sun.color = new Color(1f, .88f, .73f);
            sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.11f, .15f, .23f);
            RenderSettings.fog = false;
        }

        private Transform NewRoot(string label, Vector3 position)
        {
            Transform root = new GameObject(label).transform;
            root.SetParent(transform, false);
            root.position = position;
            return root;
        }

        private Material MakeMaterial(string label, Color color, bool unlit, float smoothness = 0)
        {
            Material template = unlit ? unlitTemplate : litTemplate;
            Shader shader = template != null ? template.shader : Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find(unlit ? "Unlit/Color" : "Standard");
            Material material = template != null ? new Material(template) : new Material(shader);
            material.name = label;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", null);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            ownedAssets.Add(material);
            return material;
        }

        private GameObject Primitive(string label, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = label;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        private Transform Sphere(string label, Vector3 position, float radius, Material material)
        {
            Transform root = NewRoot(label, position);
            const int segments = 96, rows = 48;
            var vertices = new Vector3[(segments + 1) * (rows + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new List<int>(segments * rows * 6);
            for (int y = 0; y <= rows; y++)
            for (int x = 0; x <= segments; x++)
            {
                float latitude = Mathf.PI * y / rows, longitude = 2 * Mathf.PI * x / segments;
                int index = y * (segments + 1) + x;
                vertices[index] = new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Sin(longitude)) * radius;
                uv[index] = new Vector2((float)x / segments, 1f - (float)y / rows);
                if (x == segments || y == rows) continue;
                int next = index + segments + 1;
                triangles.AddRange(new[] { index, index + 1, next, index + 1, next + 1, next });
            }
            Mesh mesh = new Mesh { name = label + " smooth sphere", vertices = vertices, uv = uv, triangles = triangles.ToArray() };
            var normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = vertices[i].normalized;
            mesh.normals = normals;
            mesh.RecalculateBounds();
            ownedAssets.Add(mesh);
            root.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            root.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            root.gameObject.AddComponent<SphereCollider>().radius = radius;
            return root;
        }

        private LineRenderer Circle(Transform parent, Vector3 center, float radius, float width, Material material)
        {
            var line = new GameObject("Orbital detail").AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false);
            line.transform.localPosition = center;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 128;
            line.widthMultiplier = width;
            line.sharedMaterial = material;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius);
            }
            return line;
        }

        private void MakeRing(Transform parent, float inner, float outer, Material material, Quaternion rotation)
        {
            const int segments = 192;
            var vertices = new Vector3[segments * 2];
            var triangles = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                Vector3 direction = new Vector3(Mathf.Cos(i * Mathf.PI * 2 / segments), 0, Mathf.Sin(i * Mathf.PI * 2 / segments));
                vertices[i * 2] = direction * inner;
                vertices[i * 2 + 1] = direction * outer;
                int a = i * 2, b = ((i + 1) % segments) * 2;
                triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1, a + 1, b, a, b + 1, b, a + 1 });
            }
            var mesh = new Mesh { name = "Planet dust annulus", vertices = vertices, triangles = triangles.ToArray() };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); ownedAssets.Add(mesh);
            var ring = new GameObject("Dust ring");
            ring.transform.SetParent(parent, false);
            ring.transform.localRotation = rotation;
            ring.AddComponent<MeshFilter>().sharedMesh = mesh;
            ring.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void BuildStars()
        {
            var random = new System.Random(6124);
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                float y = (float)random.NextDouble() * 2 - 1;
                float angle = (float)random.NextDouble() * Mathf.PI * 2;
                var normal = new Vector3(Mathf.Sqrt(1 - y * y) * Mathf.Cos(angle), y, Mathf.Sqrt(1 - y * y) * Mathf.Sin(angle));
                Vector3 center = normal * (1100 + (float)random.NextDouble() * 200) + new Vector3(0, 0, 120);
                Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
                Vector3 up = Vector3.Cross(right, normal);
                float size = .12f + (float)random.NextDouble() * .55f;
                int start = vertices.Count;
                vertices.Add(center - right * size - up * size); vertices.Add(center + right * size - up * size);
                vertices.Add(center + right * size + up * size); vertices.Add(center - right * size + up * size);
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3, start + 2, start + 1, start, start + 3, start + 2, start });
            }
            var mesh = new Mesh { name = "Fixed star field", vertices = vertices.ToArray(), triangles = triangles.ToArray() };
            mesh.RecalculateBounds(); ownedAssets.Add(mesh);
            Transform stars = NewRoot("Distant stars", Vector3.zero);
            stars.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = stars.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = MakeMaterial("Soft stars", new Color(.55f, .65f, .76f), true);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void OnDestroy()
        {
            foreach (Object asset in ownedAssets)
            {
                if (asset == null) continue;
                if (Application.isPlaying) Destroy(asset); else DestroyImmediate(asset);
            }
        }
    }
}
