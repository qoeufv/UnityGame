using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace StrategyRPG.Tactics
{
    /// <summary>Replaceable presentation/input adapter for the isolated tactical prototype.</summary>
    public class FleetBattlePrototype : MonoBehaviour
    {
        public Material materialTemplate;
        [Header("Enemy AI")]
        public bool useStochasticMcts = true;
        [Range(.25f, 3f)] public float enemyTurnThinkingSeconds = 1.5f;
        public FleetBattleState State { get; private set; }
        private FleetUnit selected;
        private Camera view;
        private bool busy;
        private bool started, integrated, bossEncounter, resultReturned;
        private int[] initialHull;
        private int enemyTurnSequence;
        private System.Action<bool, int[]> encounterFinished;

        /// <summary>Configure before Start. Hull order matches the three allied units in FleetBattleState.</summary>
        public void ConfigureEncounter(int[] hull, bool boss, System.Action<bool, int[]> onFinished)
        {
            if (started || integrated) throw new System.InvalidOperationException("Configure an encounter once, before Start.");
            if (hull == null) throw new System.ArgumentNullException(nameof(hull));
            if (onFinished == null) throw new System.ArgumentNullException(nameof(onFinished));
            var allies = new FleetBattleState().Units.Where(unit => !unit.enemy).ToArray();
            if (hull.Length != allies.Length) throw new System.ArgumentException("Supply exactly three allied hull values.", nameof(hull));
            for (int i = 0; i < hull.Length; i++)
                if (hull[i] < 0 || hull[i] > allies[i].maxHP)
                    throw new System.ArgumentOutOfRangeException(nameof(hull), "Hull must be between zero and the corresponding ship's maximum.");
            if (!hull.Any(value => value > 0)) throw new System.ArgumentException("An encounter needs at least one surviving allied ship.", nameof(hull));
            initialHull = (int[])hull.Clone();
            bossEncounter = boss;
            encounterFinished = onFinished;
            integrated = true;
        }
        private Transform world;
        private readonly Dictionary<Vector2Int, Renderer> tiles = new Dictionary<Vector2Int, Renderer>();
        private readonly Dictionary<FleetUnit, Transform> ships = new Dictionary<FleetUnit, Transform>();
        private readonly Dictionary<FleetUnit, TextMesh> labels = new Dictionary<FleetUnit, TextMesh>();
        private readonly List<Object> generated = new List<Object>();
        private Text title, details, log;
        private Button endTurn, repair, restart;
        private Material tileMaterial, lineMaterial, friendlyMaterial, enemyMaterial, rockMaterial;
        private Font font;
        private GameObject hud;
        private readonly Color cyan = new Color(.2f,.78f,.9f), coral = new Color(.95f,.38f,.3f);

        private void Start()
        {
            started = true;
            tileMaterial = MakeMaterial(new Color(.055f,.095f,.14f));
            lineMaterial = MakeMaterial(Color.white);
            friendlyMaterial = MakeMaterial(cyan); enemyMaterial = MakeMaterial(coral);
            rockMaterial = MakeMaterial(new Color(.24f,.28f,.32f));
            var cameraObject = new GameObject("Tactical Camera", typeof(Camera)); cameraObject.transform.SetParent(transform);
            view = cameraObject.GetComponent<Camera>(); view.tag = "MainCamera";
            view.orthographic = true; view.orthographicSize = 8.7f; view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(.015f,.023f,.04f);
            var center = World(new Vector2Int(3, 2)) + new Vector3(1.7f,0,.6f);
            view.transform.position = center + new Vector3(0,18,-11);
            view.transform.LookAt(center);
            BuildHUD(); ResetEncounter();
        }
        public static Vector3 World(Vector2Int c) => new Vector3(Mathf.Sqrt(3) * (c.x + c.y * .5f), 0, 1.5f * c.y);
        private Material MakeMaterial(Color color)
        {
            var material = materialTemplate != null ? new Material(materialTemplate) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", color); material.SetFloat("_Cull",0); generated.Add(material); return material;
        }
        public void ResetEncounter()
        {
            // Integrated runs own their fleet state; restarting would erase exploration damage.
            if (!started || (integrated && State != null)) return;
            StopAllCoroutines(); busy = false;
            if (world != null) Destroy(world.gameObject);
            foreach (var mesh in generated.OfType<Mesh>().ToArray()) { Destroy(mesh); generated.Remove(mesh); }
            tiles.Clear(); ships.Clear(); labels.Clear();
            world = new GameObject("Encounter Visuals").transform; world.SetParent(transform);
            State = new FleetBattleState();
            if (integrated)
            {
                var allies = State.Units.Where(unit => !unit.enemy).ToArray();
                for (int i = 0; i < allies.Length; i++) allies[i].hp = initialHull[i];
                if (bossEncounter)
                    foreach (var enemy in State.Units.Where(unit => unit.enemy))
                    {
                        enemy.maxHP = Mathf.RoundToInt(enemy.maxHP * 1.5f);
                        enemy.hp = enemy.maxHP;
                        enemy.attack += 3;
                    }
            }
            selected = State.Units.FirstOrDefault(unit => !unit.enemy && unit.Alive);
            Mesh hex = HexMesh();
            for (int q = 0; q < FleetBattleState.Width; q++) for (int r = 0; r < FleetBattleState.Height; r++)
            {
                var cell = new Vector2Int(q,r);
                var tile = new GameObject($"Hex {q},{r}", typeof(MeshFilter), typeof(MeshRenderer));
                tile.transform.SetParent(world); tile.transform.position = World(cell);
                tile.GetComponent<MeshFilter>().sharedMesh = hex;
                var renderer = tile.GetComponent<Renderer>(); renderer.sharedMaterial = tileMaterial; tiles[cell] = renderer;
                if (State.Obstacles.Contains(cell))
                {
                    var rock = Primitive("Asteroid", PrimitiveType.Sphere, tile.transform, rockMaterial);
                    rock.localPosition = new Vector3(0,.38f,0); rock.localScale = new Vector3(1.1f,.8f,.9f);
                }
            }
            var beacon = Primitive("Repair Relay", PrimitiveType.Cylinder, world, friendlyMaterial);
            beacon.position = World(State.RepairCell) + Vector3.up*.08f;
            beacon.localScale = new Vector3(.32f,.08f,.32f);
            foreach (var unit in State.Units)
            {
                var root = new GameObject(unit.name).transform; root.SetParent(world); root.position = World(unit.cell) + Vector3.up*.32f;
                var hull = Primitive("Hull", PrimitiveType.Capsule, root, unit.enemy ? enemyMaterial : friendlyMaterial);
                hull.localRotation = Quaternion.Euler(0,0,90); hull.localScale = new Vector3(.32f,.48f,.32f);
                var wings = Primitive("Wings", PrimitiveType.Cube, root, unit.enemy ? enemyMaterial : friendlyMaterial);
                wings.localScale = new Vector3(.28f,.10f,.85f);
                var nose = Primitive("Bow", PrimitiveType.Sphere, root, unit.enemy ? enemyMaterial : friendlyMaterial);
                nose.localPosition = new Vector3(unit.enemy ? -.55f : .55f,0,0); nose.localScale = Vector3.one*.20f;
                var labelObject = new GameObject("Status", typeof(TextMesh)); labelObject.transform.SetParent(root);
                labelObject.transform.localPosition = new Vector3(0,.65f,0);
                labelObject.transform.rotation = view.transform.rotation;
                var text = labelObject.GetComponent<TextMesh>(); text.fontSize = 42; text.characterSize = .06f; text.anchor = TextAnchor.MiddleCenter;
                ships[unit] = root; labels[unit] = text;
            }
            Say("Select a cyan ship. Click a blue hex to move, or a red target to fire."); Refresh();
        }
        private Transform Primitive(string name, PrimitiveType type, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent,false);
            Destroy(go.GetComponent<Collider>()); go.GetComponent<Renderer>().sharedMaterial = material; return go.transform;
        }
        private Mesh HexMesh()
        {
            var vertices = new Vector3[7]; var indices = new int[18];
            for (int i=0;i<6;i++)
            {
                float angle = (30+i*60)*Mathf.Deg2Rad;
                vertices[i+1] = new Vector3(Mathf.Cos(angle)*.94f,0,Mathf.Sin(angle)*.94f);
                indices[i*3]=0; indices[i*3+1]=i+1; indices[i*3+2]=(i+1)%6+1;
            }
            var mesh = new Mesh { vertices=vertices, triangles=indices }; mesh.RecalculateNormals(); generated.Add(mesh); return mesh;
        }
        private void Update()
        {
            if (State == null || busy || State.Phase != FleetPhase.Player || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Ray ray = view.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!new Plane(Vector3.up, Vector3.zero).Raycast(ray,out float distance)) return;
            Vector3 p = ray.GetPoint(distance);
            var cell = FleetBattleState.RoundAxial(new Vector2(p.x / Mathf.Sqrt(3) - p.z/3, p.z*2/3));
            if (!State.Inside(cell)) return;
            var target = State.At(cell);
            if (target != null && !target.enemy) { selected=target; Refresh(); return; }
            if (target != null && State.CanAttack(selected,target)) { StartCoroutine(Fire(selected,target)); return; }
            if (State.CanMove(selected,cell)) { StartCoroutine(Move(selected,cell)); return; }
            Say("Unavailable: check AP, range, occupied hexes and asteroid cover.");
        }
        private IEnumerator Move(FleetUnit unit, Vector2Int cell)
        {
            var path = State.Path(unit,cell); if (!State.Move(unit,cell)) yield break;
            busy=true; Refresh();
            foreach (var step in path.Skip(1))
            {
                Vector3 from=ships[unit].position, to=World(step)+Vector3.up*.32f;
                for(float t=0;t<1;t+=Time.deltaTime*5) { ships[unit].position=Vector3.Lerp(from,to,t); yield return null; }
                ships[unit].position=to;
            }
            Say(unit.name+" moved. 1 AP spent."); busy=false; Refresh();
        }
        private IEnumerator Fire(FleetUnit unit, FleetUnit target)
        {
            if (!State.Attack(unit,target)) yield break;
            FleetAttackResult result=State.LastAttackResult;
            busy=true; Refresh();
            // Keep the struck hull visible until the impact presentation finishes.
            ships[target].gameObject.SetActive(true);
            var go=new GameObject("Weapon Beam",typeof(LineRenderer)); go.transform.SetParent(world);
            var line=go.GetComponent<LineRenderer>(); line.sharedMaterial=lineMaterial; line.positionCount=2;
            Color beamColor=result.Critical?new Color(1f,.78f,.18f):result.Hit?(unit.enemy?coral:cyan):new Color(.55f,.60f,.66f);
            line.startWidth=result.Critical?.13f:.065f; line.endWidth=result.Critical?.07f:.025f; line.startColor=line.endColor=beamColor;
            line.SetPosition(0,ships[unit].position); line.SetPosition(1,ships[target].position+(result.Hit?Vector3.zero:new Vector3(.7f,.35f,.5f)));
            yield return new WaitForSeconds(.25f); Destroy(go);
            Say(!result.Hit?target.name+" DODGED "+unit.name+"'s attack.":result.Critical?"CRITICAL HIT! "+unit.name+" dealt "+result.damage+" damage to "+target.name+".":unit.name+" hit "+target.name+" for "+result.damage+" damage."); busy=false; Refresh();
        }
        private void EndTurn()
        {
            if(busy || State.Phase!=FleetPhase.Player) return;
            State.EndPhase(); StartCoroutine(EnemyTurn());
        }
        private IEnumerator EnemyTurn()
        {
            Refresh();
            if (!useStochasticMcts)
            {
                foreach(var unit in State.Units.Where(u=>u.enemy && u.Alive).ToArray())
                {
                    while(State.CanAct(unit))
                    {
                        var target=State.Units.Where(u=>State.CanAttack(unit,u)).OrderBy(u=>u.hp).FirstOrDefault();
                        if(target!=null) yield return Fire(unit,target);
                        else
                        {
                            var cell=State.Approach(unit); if(!cell.HasValue) break;
                            yield return Move(unit,cell.Value);
                        }
                        yield return new WaitForSeconds(.25f);
                    }
                    if(State.Finished) break;
                }
            }
            else
            {
                float turnDeadline = Time.realtimeSinceStartup + enemyTurnThinkingSeconds;
                int actions = 0;
                while (!State.Finished && State.Phase == FleetPhase.Enemy && State.Units.Any(State.CanAct))
                {
                    var planner = new FleetMctsPlanner(State, true, 440 + State.Round * 101 + enemyTurnSequence++);
                    float actionDeadline = Mathf.Min(turnDeadline, Time.realtimeSinceStartup + .28f);
                    do
                    {
                        planner.RunIterations(1);
                        yield return null;
                    }
                    while ((Time.realtimeSinceStartup < actionDeadline || planner.Simulations < 8) && planner.Simulations < 128);

                    FleetBattleAction action = planner.BestAction();
                    if (action.type == FleetActionType.EndPhase) break;
                    if (action.actor < 0 || action.actor >= State.Units.Count) break;
                    FleetUnit actor = State.Units[action.actor];
                    Say($"MCTS considered {planner.Simulations} simulations. {actor.name}: {action.type}.");
                    Refresh();
                    yield return new WaitForSeconds(.12f);
                    if (action.type == FleetActionType.Attack && action.target >= 0 && action.target < State.Units.Count)
                        yield return Fire(actor, State.Units[action.target]);
                    else if (action.type == FleetActionType.Move)
                        yield return Move(actor, action.cell);
                    else if (action.type == FleetActionType.FieldRepair)
                        yield return FieldRepair(actor);
                    else break;
                    actions++;
                    if (actions >= 12) break;
                }
            }
            if(!State.Finished) { State.EndPhase(); Say("Your fleet is ready. Each surviving ship has 2 AP."); }
            if(selected==null || !selected.Alive) selected=State.Units.FirstOrDefault(u=>!u.enemy && u.Alive);
            Refresh();
        }
        private IEnumerator FieldRepair(FleetUnit unit)
        {
            if (!State.FieldRepair(unit)) yield break;
            busy = true; Refresh();
            yield return new WaitForSeconds(.3f);
            Say(unit.name + " restored 12 hull with its field repair charge.");
            busy = false; Refresh();
        }
        private void Refresh()
        {
            if (selected != null && !selected.Alive) selected = State.Units.FirstOrDefault(u => !u.enemy && u.Alive);
            title.text=State.Finished ? (State.Phase==FleetPhase.Victory?(integrated ? "VICTORY  /  ENCOUNTER CLEARED" : "VICTORY  /  SECTOR SECURED"):"DEFEAT  /  FLEET LOST") : $"FLEET TACTICS   /   ROUND {State.Round}   /   {(State.Phase==FleetPhase.Player?"YOUR TURN":"HOSTILE TURN")}";
            foreach(var pair in tiles)
            {
                Color color=new Color(.055f,.095f,.14f);
                if(State.Obstacles.Contains(pair.Key)) color=new Color(.1f,.105f,.12f);
                else if(!busy && State.CanMove(selected,pair.Key)) color=new Color(.08f,.23f,.32f);
                if(!busy && State.CanAttack(selected,State.At(pair.Key))) color=new Color(.43f,.12f,.13f);
                if(selected!=null && selected.cell==pair.Key) color=new Color(.16f,.43f,.48f);
                var block=new MaterialPropertyBlock(); block.SetColor("_BaseColor",color); pair.Value.SetPropertyBlock(block);
            }
            foreach(var unit in State.Units)
            { ships[unit].gameObject.SetActive(unit.Alive); labels[unit].text=$"{unit.name}\n{unit.hp}/{unit.maxHP}  |  {unit.ap} AP"; }
            details.text=selected==null?"No ship selected":$"{selected.name.ToUpper()}\n\nHULL  {selected.hp} / {selected.maxHP}\nACTION POINTS  {selected.ap} / 2\n\nMOVE  {selected.move} hexes\nRANGE  {selected.range} hexes\nDAMAGE  {selected.attack}\nHIT  {selected.accuracy*100f:0}%\nCRIT  {selected.critRate*100f:0}%  /  {selected.criticalMultiplier:0.0}x\n\nMove / fire: 1 AP each\nA miss is shown as DODGED.\nAsteroids block movement\nand line of fire.\n\nRepair relay: +20 hull\nField repair: +12 hull\n{selected.fieldRepairs} field charge left.";
            endTurn.interactable=!busy && State.Phase==FleetPhase.Player;
            repair.interactable=!busy && (State.CanRepair(selected) || State.CanFieldRepair(selected));
            repair.GetComponentInChildren<Text>().text=State.CanRepair(selected)?"RELAY +20":selected!=null && selected.fieldRepairs>0?"FIELD REPAIR +12":"REPAIR USED";
            restart.interactable = !busy && !resultReturned && (integrated ? State.Finished : State.Phase != FleetPhase.Enemy);
            restart.GetComponentInChildren<Text>().text = integrated ? "RETURN TO ROUTE" : "RESTART";
            if (State.Finished)
            {
                string outcome = State.Phase == FleetPhase.Victory ? "All hostiles destroyed. " : "All allied ships lost. ";
                log.text = outcome + (integrated ? "Return to the route to resolve this encounter." : "Restart to try another approach.");
            }
        }
        private void ReturnOrRestart()
        {
            if (!integrated) { ResetEncounter(); return; }
            if (State == null || !State.Finished || busy || resultReturned) return;
            resultReturned = true;
            restart.interactable = false;
            var hull = State.Units.Where(unit => !unit.enemy).Select(unit => unit.hp).ToArray();
            var callback = encounterFinished;
            encounterFinished = null;
            callback?.Invoke(State.Phase == FleetPhase.Victory, hull);
        }
        private void Say(string message) { log.text=message; }
        private void BuildHUD()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hud=new GameObject("Tactical HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); hud.transform.SetParent(transform);
            hud.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=hud.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
            if(EventSystem.current==null) { var events=new GameObject("Tactical EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)); events.transform.SetParent(transform); }
            title=Text("Heading",new Vector2(.025f,.90f),new Vector2(.97f,.98f),30);
            details=Text("Ship details",new Vector2(.80f,.28f),new Vector2(.99f,.87f),21);
            log=Text("Action log",new Vector2(.025f,.03f),new Vector2(.72f,.09f),23);
            var help=Text("Help",new Vector2(.025f,.84f),new Vector2(.77f,.9f),20); help.text="SELECT SHIP  >  MOVE OR FIRE  >  END TURN     |     HOSTILE AI: STOCHASTIC MCTS";
            endTurn=Button("END TURN",new Vector2(.80f,.14f),new Vector2(.98f,.21f),EndTurn);
            repair=Button("REPAIR +20",new Vector2(.80f,.23f),new Vector2(.98f,.30f),()=>
            {
                if (busy) return;
                if (State.Repair(selected)) Say("Repair relay consumed. +20 hull.");
                else if (State.FieldRepair(selected)) Say(selected.name + " used its field repair charge. +12 hull.");
                Refresh();
            });
            restart=Button("RESTART",new Vector2(.80f,.045f),new Vector2(.98f,.11f),ReturnOrRestart);
        }
        private Text Text(string name,Vector2 min,Vector2 max,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(hud.transform,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var text=go.GetComponent<Text>(); text.font=font; text.fontSize=size; text.color=new Color(.82f,.91f,.94f); text.raycastTarget=false; return text;
        }
        private Button Button(string label,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
        {
            var text=Text(label,min,max,23); text.text=label; text.alignment=TextAnchor.MiddleCenter;
            var parent=new GameObject(label+" button",typeof(RectTransform),typeof(Image),typeof(Button)); parent.transform.SetParent(hud.transform,false);
            var rect=parent.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            parent.GetComponent<Image>().color=new Color(.08f,.24f,.30f);
            text.transform.SetParent(parent.transform,false); text.rectTransform.anchorMin=Vector2.zero; text.rectTransform.anchorMax=Vector2.one;
            var button=parent.GetComponent<Button>(); button.targetGraphic=parent.GetComponent<Image>(); button.onClick.AddListener(action); return button;
        }
        private void OnDisable() { StopAllCoroutines(); busy=false; }
        private void OnEnable() { if(State!=null && !integrated) ResetEncounter(); }
        private void OnDestroy() { foreach(var item in generated) if(item!=null) Destroy(item); }
    }
}
