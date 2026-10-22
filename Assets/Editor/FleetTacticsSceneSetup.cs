using StrategyRPG.Tactics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FleetTacticsSceneSetup
{
    public const string ScenePath = "Assets/Scenes/FleetTacticsPrototype.unity";
    [MenuItem("Tools/Open Fleet Tactics Prototype")]
    public static void Open()
    {
        if (Application.isPlaying) { Debug.LogWarning("Exit Play Mode before opening the prototype."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (System.IO.File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Fleet Tactics Prototype", typeof(FleetBattlePrototype));
        const string materialPath = "Assets/Materials/FleetPrototype.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, materialPath);
        }
        root.GetComponent<FleetBattlePrototype>().materialTemplate = material;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("FLEET_TACTICS_SCENE_READY: " + ScenePath);
    }
}
