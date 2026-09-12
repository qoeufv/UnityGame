using System;
using StrategyRPG.Combat;
using StrategyRPG.Map;
using UnityEditor;
using UnityEngine;

/// <summary>Opt-in Play Mode smoke check of the existing arena and delayed skill damage.</summary>
public static class BattleSkillPlaybackValidation
{
    private static BattleManager manager;
    private static SkillData skill;
    private static int initialHealth;
    private static double started;
    private static bool sawAnimation;

    [MenuItem("Tools/Validate Battle Skill Playback %&n")]
    public static void Validate()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        if (skill != null) throw new InvalidOperationException("Validation is already running.");
        manager = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
        if (manager == null || manager.IsBattleActive)
            throw new InvalidOperationException("Start validation from the map, outside a battle.");
        HexMapManager map = UnityEngine.Object.FindAnyObjectByType<HexMapManager>();
        skill = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<SkillData>(
            "Assets/ScriptableObjects/Skills/FlameSunPalm.asset"));
        // Deterministic hit for the integration check; never edits the example asset.
        SerializedObject serialized = new SerializedObject(skill);
        serialized.FindProperty("accuracy").floatValue = 1f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        manager.StartBattle(map.GetTileData(new Vector3Int(0, 1, 0)));
        initialHealth = manager.EnemyStats.CurrentHealth;
        manager.PlayerSkill(skill);
        if (manager.EnemyStats.CurrentHealth != initialHealth)
        {
            Cleanup();
            throw new InvalidOperationException("Damage was applied before animation impact.");
        }
        started = EditorApplication.timeSinceStartup;
        sawAnimation = false;
        EditorApplication.update += Check;
    }

    private static void Check()
    {
        if (!Application.isPlaying) { Cleanup(); return; }
        BattleArenaController arena = UnityEngine.Object.FindAnyObjectByType<BattleArenaController>();
        Animator animator = arena != null ? arena.PlayerVisual?.AnimatorComponent : null;
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_MartialArts"))
            sawAnimation = true;
        if (manager.EnemyStats.CurrentHealth < initialHealth)
        {
            bool passed = sawAnimation && manager.EnemyStats.RemainingTurns(StatusEffectType.Burning) == 3;
            Cleanup();
            if (!passed) throw new InvalidOperationException("Skill animation or Burning integration failed.");
            Debug.Log("BATTLE_PLAYBACK_VALIDATION_PASSED: existing Animator entered Skill_MartialArts; damage waited for impact; Burning applied.");
        }
        else if (EditorApplication.timeSinceStartup - started > 5)
        {
            Cleanup();
            throw new InvalidOperationException("Skill playback produced no damage within 5 seconds.");
        }
    }

    private static void Cleanup()
    {
        EditorApplication.update -= Check;
        if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
        skill = null;
    }
}
