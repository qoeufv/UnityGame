using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace StrategyRPG.Flight
{
    /// <summary>Independent flight sample: environment, replaceable ship silhouette and navigation HUD.</summary>
    public class SpaceFlightPrototype : MonoBehaviour
    {
        public Material litTemplate, unlitTemplate;
        public GameObject stargatePrefab;
        private SpaceFlightEnvironment environment;
        private SpaceFlightController pilot;
        private SpaceFollowCamera follow;
        private Camera view;
        private Text heading, telemetry, targetLabel, feedback, instructions;
        private RectTransform marker;
        private Text markerText;
        private Transform target;
        private string targetName;
        private int targetIndex;
        private readonly HashSet<int> discovered = new HashSet<int>();
        private readonly List<Object> resources = new List<Object>();
        private float messageUntil;
        private string message = "Pilot the ship to the stargate. Approach slowly, then press F.";

        private void Start()
        {
            environment = gameObject.AddComponent<SpaceFlightEnvironment>();
            environment.litTemplate=litTemplate; environment.unlitTemplate=unlitTemplate; environment.stargatePrefab=stargatePrefab;
            environment.Build();
            var ship=new GameObject("Player Starfighter"); ship.transform.SetParent(transform);
            pilot=ship.AddComponent<SpaceFlightController>();
            BuildShip(ship.transform);
            var cameraObject=new GameObject("Flight Camera",typeof(Camera),typeof(AudioListener));cameraObject.transform.SetParent(transform);
            view=cameraObject.GetComponent<Camera>();view.tag="MainCamera";view.nearClipPlane=.2f;view.farClipPlane=2000;view.fieldOfView=60;
            view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.007f,.012f,.027f);
            follow=cameraObject.AddComponent<SpaceFollowCamera>();follow.target=ship.transform;
            BuildHUD();
            int startTarget = PlanetSurfacePrototype.ReturningFromSurface ? PlanetSurfacePrototype.ReturnTargetIndex : 0;
            ChooseTarget(startTarget >= 0 && startTarget < 4 ? startTarget : 0);
            if (PlanetSurfacePrototype.ReturningFromSurface)
            {
                Vector3 returnPosition = target.position + Vector3.back * 90f + Vector3.up * 12f;
                pilot.ResetFlight(returnPosition, Quaternion.LookRotation(target.position - returnPosition));
                Notify("Returned to near orbit: " + targetName + ".");
                PlanetSurfacePrototype.ReturningFromSurface = false;
                PlanetSurfacePrototype.ReturnTargetIndex = -1;
            }
            else pilot.ResetFlight(Vector3.zero,Quaternion.identity);
            follow.ResetView(); messageUntil=Time.unscaledTime+9;
        }
        private Material Material(Material template,Color color)
        {
            var material=new Material(template);material.SetColor("_BaseColor",color);resources.Add(material);return material;
        }
        private void BuildShip(Transform root)
        {
            var hull=Material(litTemplate,new Color(.38f,.48f,.57f));hull.SetFloat("_Metallic",.45f);hull.SetFloat("_Smoothness",.6f);
            var glass=Material(litTemplate,new Color(.025f,.17f,.22f));glass.SetFloat("_Smoothness",.85f);
            var exhaust=Material(unlitTemplate,new Color(.1f,.8f,1));
            Part("Hull",PrimitiveType.Capsule,root,new Vector3(0,0,0),new Vector3(.7f,1.15f,.6f),Quaternion.Euler(90,0,0),hull);
            Part("Port wing",PrimitiveType.Cube,root,new Vector3(-.65f,-.13f,-.35f),new Vector3(1.05f,.10f,.7f),Quaternion.Euler(0,-25,0),hull);
            Part("Starboard wing",PrimitiveType.Cube,root,new Vector3(.65f,-.13f,-.35f),new Vector3(1.05f,.10f,.7f),Quaternion.Euler(0,25,0),hull);
            Part("Canopy",PrimitiveType.Sphere,root,new Vector3(0,.25f,.45f),new Vector3(.47f,.28f,.85f),Quaternion.identity,glass);
            foreach(float x in new[]{-.34f,.34f})
            {
                var engine=Part("Ion engine",PrimitiveType.Sphere,root,new Vector3(x,-.03f,-.96f),new Vector3(.18f,.18f,.24f),Quaternion.identity,exhaust);
                var trail=engine.AddComponent<TrailRenderer>();trail.sharedMaterial=exhaust;trail.time=.25f;trail.startWidth=.10f;trail.endWidth=0;trail.minVertexDistance=.3f;
                trail.startColor=new Color(.15f,.75f,1,.7f);trail.endColor=new Color(.08f,.3f,1,0);
            }
        }
        private GameObject Part(string name,PrimitiveType type,Transform parent,Vector3 position,Vector3 scale,Quaternion rotation,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.transform.localRotation=rotation;
            Destroy(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        private void Update()
        {
            if(pilot==null || target==null)return;
            var keys=Keyboard.current;
            if(keys!=null)
            {
                if(keys.digit1Key.wasPressedThisFrame)ChooseTarget(0);
                if(keys.digit2Key.wasPressedThisFrame)ChooseTarget(1);
                if(keys.digit3Key.wasPressedThisFrame)ChooseTarget(2);
                if(keys.digit4Key.wasPressedThisFrame)ChooseTarget(3);
                if(keys.tabKey.wasPressedThisFrame)ChooseTarget((targetIndex+1)%4);
                if(keys.rKey.wasPressedThisFrame){pilot.ResetFlight(Vector3.zero,Quaternion.identity);foreach(var trail in pilot.GetComponentsInChildren<TrailRenderer>())trail.Clear();follow.ResetView();Notify("Returned to departure point.");}
            }
            float distance=Vector3.Distance(pilot.transform.position,target.position);
            float approach=targetIndex==1?75:targetIndex==3?35:targetIndex==2?16:22;
            bool near=distance<=approach;
            if(keys!=null && keys.fKey.wasPressedThisFrame)
            {
                if(!near)Notify("Approach the selected destination first.");
                else if(Mathf.Abs(pilot.Speed)>5)Notify("Hold SPACE to brake below 5 m/s before interacting.");
                else
                {
                    discovered.Add(targetIndex);
                    if (targetIndex == 1) { Notify("Entering Azure World orbit..."); SceneManager.LoadScene("Planet_AzureWorld"); }
                    else if (targetIndex == 3) { Notify("Entering Pale Moon orbit..."); SceneManager.LoadScene("Planet_PaleMoon"); }
                    else Notify(targetIndex==0?"Stargate reached. Flight sample complete; jump travel is not enabled yet.":targetName+" scanned. Choose another destination to explore.");
                }
            }
            telemetry.text=$"{Mathf.Abs(pilot.Speed):0.0} m/s   {(pilot.Boosting?"BOOST":"CRUISE")}\nDiscoveries {discovered.Count} / 4";
            string action = targetIndex == 1 || targetIndex == 3 ? "[ F ] LAND" : "[ F ] INTERACT";
            targetLabel.text=$"DESTINATION  /  {targetName}\n{distance:0} m to centre   {(near?action:"APPROACH TARGET")}";
            feedback.text=pilot.AtBoundary?"EDGE OF TEST SECTOR — turn back toward the destinations":Time.unscaledTime<messageUntil?message:near?"Slow down with SPACE, then press F to land / inspect.":"W thrust   A / D turn   Up / Down pitch   Shift boost";
        }
        private void LateUpdate()
        {
            if(view==null || target==null || marker==null)return;
            Vector3 screen=view.WorldToViewportPoint(target.position);
            bool behind=screen.z<0;
            if(behind){screen.x=1-screen.x;screen.y=1-screen.y;}
            marker.anchorMin=marker.anchorMax=new Vector2(Mathf.Clamp(screen.x,.07f,.93f),Mathf.Clamp(screen.y,.20f,.77f));
            marker.anchoredPosition=Vector2.zero;markerText.text=behind?"TURN TOWARD\n"+targetName:"◇ "+targetName;
        }
        private void ChooseTarget(int index)
        {
            targetIndex=index;
            target=index==0?environment.Gate:index==1?environment.Planet:index==2?environment.Station:environment.Moon;
            targetName=index==0?"ANCIENT GATE":index==1?"AZURE WORLD":index==2?"RELAY STATION":"PALE MOON";
        }
        private void Notify(string value){message=value;messageUntil=Time.unscaledTime+6;}
        private void BuildHUD()
        {
            var canvasObject=new GameObject("Flight HUD",typeof(Canvas),typeof(CanvasScaler));canvasObject.transform.SetParent(transform);
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
            heading=Label(canvasObject.transform,"Heading",new Vector2(.03f,.90f),new Vector2(.75f,.97f),28);heading.text="FRONTIER  /  FREE FLIGHT";
            telemetry=Label(canvasObject.transform,"Speed",new Vector2(.03f,.77f),new Vector2(.35f,.88f),25);
            targetLabel=Label(canvasObject.transform,"Target",new Vector2(.68f,.79f),new Vector2(.98f,.95f),23);
            feedback=Label(canvasObject.transform,"Feedback",new Vector2(.03f,.12f),new Vector2(.97f,.19f),22);
            instructions=Label(canvasObject.transform,"Controls",new Vector2(.03f,.025f),new Vector2(.97f,.12f),19);
            instructions.text="W / S  Thrust / reverse    A / D  Turn    ↑ / ↓  Pitch    Q / E  Roll    SHIFT  Boost    SPACE  Brake\nRight mouse  Look around    Wheel  Zoom    1–4 / TAB  Destination    F  Land / Inspect    R  Reset";
            markerText=Label(canvasObject.transform,"Navigation",new Vector2(.5f,.5f),new Vector2(.5f,.5f),20);
            marker=markerText.rectTransform;marker.sizeDelta=new Vector2(250,60);markerText.alignment=TextAnchor.MiddleCenter;markerText.color=new Color(.3f,.85f,1);
        }
        private Text Label(Transform parent,string name,Vector2 min,Vector2 max,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.color=new Color(.82f,.9f,.96f);text.raycastTarget=false;return text;
        }
        private void OnDestroy(){foreach(var resource in resources)if(resource!=null)Destroy(resource);}
    }
}
