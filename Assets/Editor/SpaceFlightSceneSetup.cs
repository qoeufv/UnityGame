using StrategyRPG.Flight;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SpaceFlightSceneSetup
{
    public const string ScenePath="Assets/Scenes/SpaceFlightPrototype.unity";
    [MenuItem("Tools/Open Space Flight Prototype")]
    public static void Open()
    {
        if(Application.isPlaying){Debug.LogWarning("Exit Play Mode before opening the flight prototype.");return;}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        if(System.IO.File.Exists(ScenePath)){EditorSceneManager.OpenScene(ScenePath);return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var prototype=new GameObject("Free Flight Prototype",typeof(SpaceFlightPrototype)).GetComponent<SpaceFlightPrototype>();
        prototype.litTemplate=Template("FlightLit","Universal Render Pipeline/Lit");
        prototype.unlitTemplate=Template("FlightUnlit","Universal Render Pipeline/Unlit");
        prototype.stargatePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Stargate/Prefabs/Stargate.prefab");
        if(prototype.stargatePrefab==null)throw new System.InvalidOperationException("Missing imported stargate prefab.");
        EditorSceneManager.SaveScene(scene,ScenePath);
        Debug.Log("SPACE_FLIGHT_SCENE_READY: "+ScenePath);
    }
    private static Material Template(string name,string shaderName)
    {
        string path="Assets/Materials/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find(shaderName));AssetDatabase.CreateAsset(material,path);}
        return material;
    }
}
