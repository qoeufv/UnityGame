using System;
using System.Collections;
using Random = UnityEngine.Random;
using StrategyRPG.Map;
using StrategyRPG.UI;
using UnityEngine;

namespace StrategyRPG.Combat
{
    public enum BattleState
    {
        Inactive,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    /// <summary>
    /// The possible results of the prototype's basic attack roll.
    /// This can later be replaced by a configurable outcome pool.
    /// </summary>
    public enum AttackOutcome
    {
        Miss,
        NormalHit,
        CriticalHit
    }

    /// <summary>
    /// Owns combat state, turn order, action resolution, and battle outcomes.
    /// UI presentation is delegated to BattleUIController and 3D environment to BattleArenaController.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("Map Integration")]
        [SerializeField] private HexMapInput mapInput;

        [Header("Battle UI & 3D Arena")]
        [SerializeField] private BattleUIController battleUI;
        [SerializeField] private BattleArenaController battleArena;

        [Header("Prototype Balance")]
        [SerializeField, Min(0f)] private float enemyTurnDelay = 0.65f;
        [SerializeField, Min(0f)] private float battleEndDelay = 1.25f;

        [Header("Optional Data-Driven Combat")]
        [SerializeField] private CharacterData playerData;
        [SerializeField] private CharacterData enemyData;
        [SerializeField] private SkillData defaultSkill;
        [Tooltip("Cancel a skill if its animation event never arrives; no damage is applied.")]
        [SerializeField, Min(0.1f)] private float animationEventTimeout = 5f;

        private CombatantStats player = new CombatantStats("Player", 100);
        private CombatantStats enemy = new CombatantStats("Enemy", 60);

        private BattleState state = BattleState.Inactive;
        private HexTileData activeBattleTile;
        private bool playerIsDefending;

        public CombatantStats PlayerStats => player;
        public CombatantStats EnemyStats => enemy;
        public BattleState State => state;
        public bool IsBattleActive => state != BattleState.Inactive;

        private void Awake()
        {
            if (mapInput == null)
            {
                mapInput = FindAnyObjectByType<HexMapInput>();
            }

            if (battleUI == null)
            {
                battleUI = GetComponent<BattleUIController>();
            }

            if (battleArena == null)
            {
                battleArena = FindAnyObjectByType<BattleArenaController>(FindObjectsInactive.Include);
            }
        }

        private void Start()
        {
            if (battleUI == null)
            {
                Debug.LogError("BattleManager: A BattleUIController is required.", this);
                return;
            }

            battleUI.Initialize(this);
            battleUI.Hide();
        }

        public void StartBattle(HexTileData battleTile)
        {
            if (IsBattleActive || battleTile == null || battleTile.isCleared)
            {
                return;
            }

            // Fresh runtime copies also discard equipment/status changes from the previous battle.
            player = playerData != null ? new CombatantStats(playerData) : new CombatantStats("Player", 100);
            enemy = enemyData != null ? new CombatantStats(enemyData) : new CombatantStats("Enemy", 60);
            activeBattleTile = battleTile;
            player.ResetToFullHealth();
            enemy.ResetToFullHealth();
            playerIsDefending = false;
            state = BattleState.PlayerTurn;

            if (mapInput != null)
            {
                mapInput.SetMapInputEnabled(false);
            }

            // Activate 3D Battle Arena with terrain-specific environment
            if (battleArena != null)
            {
                battleArena.ShowBattle(battleTile.terrainType);
                if (playerData != null) battleArena.PlayerVisual?.EquipWeapon(player.Weapon);
                if (enemyData != null) battleArena.EnemyVisual?.EquipWeapon(enemy.Weapon);
            }

            battleUI.Show();
            battleUI.ClearLog();
            WriteLog($"◆ 江湖对决开始于【{battleTile.terrainType}】地界 ({battleTile.gridPosition.x}, {battleTile.gridPosition.y})");
            WriteLog("【少侠回合】请选择应对招式。");
            RefreshUI();
        }

        public void PlayerAttack()
        {
            if (!BeginPlayerAction()) return;

            if (battleArena != null && battleArena.PlayerVisual != null)
            {
                battleArena.PlayerVisual.PlayAttack();
            }

            AttackOutcome outcome = RollBasicAttackOutcome();
            switch (outcome)
            {
                case AttackOutcome.Miss:
                    WriteLog("少侠招式落空，被对手轻巧避过！");
                    if (battleArena != null && battleArena.EnemyVisual != null)
                    {
                        battleArena.EnemyVisual.PlayDodge();
                    }
                    break;

                case AttackOutcome.NormalHit:
                    int normalDamage = enemy.TakeDamage(Random.Range(10, 15));
                    WriteLog($"少侠出招击中对手，造成 {normalDamage} 点气血伤害。");
                    if (battleArena != null && battleArena.EnemyVisual != null)
                    {
                        battleArena.EnemyVisual.PlayHit();
                    }
                    break;

                case AttackOutcome.CriticalHit:
                    int criticalDamage = enemy.TakeDamage(Random.Range(20, 29));
                    WriteLog("【会心一击！】少侠招招制敌，命中破绽！");
                    WriteLog($"对手受到 {criticalDamage} 点重创伤害！");
                    if (battleArena != null && battleArena.EnemyVisual != null)
                    {
                        battleArena.EnemyVisual.PlayHit();
                    }
                    break;
            }

            FinishPlayerAction();
        }

        public void PlayerSkill(SkillCategory category = SkillCategory.MartialArt)
        {
            if (defaultSkill != null && defaultSkill.Category == category)
            {
                PlayerSkill(defaultSkill);
                return;
            }
            if (!BeginPlayerAction()) return;

            if (battleArena != null && battleArena.PlayerVisual != null)
            {
                battleArena.PlayerVisual.PlaySkill(category);
            }

            if (Random.value < 0.15f)
            {
                WriteLog("内劲运转未稳，招式未能完全命中！");
                if (battleArena != null && battleArena.EnemyVisual != null)
                {
                    battleArena.EnemyVisual.PlayDodge();
                }
            }
            else
            {
                int damage = enemy.TakeDamage(Random.Range(18, 27));
                WriteLog($"武学绝技威力惊人！对敌人造成 {damage} 点绝学伤害！");
                if (battleArena != null && battleArena.EnemyVisual != null)
                {
                    battleArena.EnemyVisual.PlayHit();
                }
            }

            FinishPlayerAction();
        }

        /// <summary>Opt-in authored skill action. Resolution remains here, outside animation playback.</summary>
        public void PlayerSkill(SkillData skill)
        {
            if (state != BattleState.PlayerTurn || skill == null) return;
            if (!skill.SupportsWeapon(player.Weapon))
            {
                WriteLog($"{skill.DisplayName} requires {skill.WeaponRequirement}.");
                return;
            }
            BattleCharacterController controller = battleArena != null ? battleArena.PlayerController : null;
            if (controller == null || !controller.isActiveAndEnabled)
            {
                WriteLog("No active battle character controller is available.");
                return;
            }
            if (!BeginPlayerAction()) return;
            StartCoroutine(PlayerSkillRoutine(skill, controller));
        }

        private IEnumerator PlayerSkillRoutine(SkillData skill, BattleCharacterController controller)
        {
            bool impacted = false;
            int actionId = -1;
            Action<int, SkillData> onImpact = (id, playedSkill) =>
            {
                if (id == actionId && playedSkill == skill) impacted = true;
            };
            controller.Impact += onImpact;
            try
            {
                controller.Visual.EquipWeapon(player.Weapon);
                controller.PlaySkill(skill);
                actionId = controller.CurrentActionId;
                float elapsed = 0f;
                float timeout = skill.ImpactMode == SkillImpactMode.Timed
                    ? skill.ImpactTime + 1f : Mathf.Max(0.1f, animationEventTimeout);
                while (!impacted && controller != null && controller.CurrentActionId == actionId &&
                    controller.HasPendingImpact && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (impacted)
                {
                    // Unity Random.value can return 1; resolver takes rolls in [0, 1).
                    SkillResult result = SkillResolver.Resolve(player, enemy, skill,
                        Mathf.Min(Random.value, 0.999999f), Mathf.Min(Random.value, 0.999999f));
                    WriteLog(result.Hit ? $"{skill.DisplayName}: {result.Damage} damage" +
                        (result.Critical ? " (critical)." : ".") : $"{skill.DisplayName} missed.");
                    if (result.Hit) battleArena.EnemyController?.PlayHit();
                    else battleArena.EnemyVisual?.PlayDodge();
                    // VFX is a presentation side effect; the resolver only returns gameplay results.
                    if (result.Hit && skill.VfxPrefab != null)
                        battleArena.EnemyVisual?.SpawnEffect(skill.VfxPrefab);
                }
                else
                {
                    if (controller != null && controller.CurrentActionId == actionId) controller.CancelAction();
                    WriteLog($"{skill.DisplayName} cancelled before impact (interrupted or missing animation event).");
                }
            }
            finally
            {
                if (controller != null)
                {
                    controller.Impact -= onImpact;
                    if (controller.CurrentActionId == actionId && controller.HasPendingImpact) controller.CancelAction();
                }
            }
            FinishPlayerAction();
        }

        public void PlayerHeal()
        {
            if (!BeginPlayerAction()) return;

            if (battleArena != null && battleArena.PlayerVisual != null)
            {
                battleArena.PlayerVisual.PlaySkill(SkillCategory.InnerSkill);
            }

            int healedAmount = player.Heal(Random.Range(15, 26));
            WriteLog($"调息运气，少侠恢复了 {healedAmount} 点气血。");
            FinishPlayerAction();
        }

        public void PlayerDefend()
        {
            if (!BeginPlayerAction()) return;

            playerIsDefending = true;
            if (battleArena != null && battleArena.PlayerVisual != null)
            {
                battleArena.PlayerController?.PlayDefend();
            }

            WriteLog("少侠运起招架防守身法，下一次受击伤害大幅减轻。");
            FinishPlayerAction();
        }

        public void PlayerQinggong()
        {
            if (!BeginPlayerAction()) return;

            playerIsDefending = true;
            if (battleArena != null && battleArena.PlayerVisual != null)
            {
                battleArena.PlayerVisual.PlaySkill(SkillCategory.Qinggong);
            }

            WriteLog("少侠施展轻功身法，身如飞燕腾挪闪避！");
            FinishPlayerAction();
        }

        private bool BeginPlayerAction()
        {
            if (state != BattleState.PlayerTurn)
            {
                return false;
            }

            state = BattleState.EnemyTurn;
            battleUI.SetActionButtonsInteractable(false);
            return true;
        }

        private void FinishPlayerAction()
        {
            int statusDamage = player.EndTurn();
            if (statusDamage > 0) WriteLog($"Burning: player takes {statusDamage} damage.");
            RefreshUI();
            if (player.IsDefeated)
            {
                HandleDefeat();
                return;
            }

            if (enemy.IsDefeated)
            {
                HandleVictory();
                return;
            }

            StartCoroutine(EnemyTurnRoutine());
        }

        private IEnumerator EnemyTurnRoutine()
        {
            WriteLog("【对手回合】敌人出招！");

            if (battleArena != null && battleArena.EnemyVisual != null)
            {
                battleArena.EnemyVisual.PlayAttack();
            }

            yield return new WaitForSeconds(enemyTurnDelay);

            int rolledDamage = Random.Range(8, 15);
            if (playerIsDefending)
            {
                rolledDamage = Mathf.CeilToInt(rolledDamage * 0.5f);
                playerIsDefending = false;
                WriteLog("少侠成功招架，化解了大半劲力！");
            }

            int actualDamage = player.TakeDamage(rolledDamage);
            WriteLog($"对手击中少侠，造成 {actualDamage} 点伤害。");

            if (battleArena != null && battleArena.PlayerVisual != null)
            {
                battleArena.PlayerVisual.PlayHit();
            }

            RefreshUI();

            if (player.IsDefeated)
            {
                HandleDefeat();
                yield break;
            }

            int statusDamage = enemy.EndTurn();
            if (statusDamage > 0) WriteLog($"Burning: enemy takes {statusDamage} damage.");
            if (enemy.IsDefeated)
            {
                HandleVictory();
                yield break;
            }

            state = BattleState.PlayerTurn;
            WriteLog("【少侠回合】请选择应对招式。");
            RefreshUI();
        }

        private AttackOutcome RollBasicAttackOutcome()
        {
            float roll = Random.value;

            if (roll < 0.05f) return AttackOutcome.Miss;
            if (roll < 0.05f + 0.95f * player.Stats.CritRate) return AttackOutcome.CriticalHit;
            return AttackOutcome.NormalHit;
        }

        private void HandleVictory()
        {
            state = BattleState.Victory;
            activeBattleTile.isCleared = true;
            WriteLog("【大获全胜】强敌败退，江湖名望提升！");

            if (battleArena != null)
            {
                if (battleArena.PlayerVisual != null) battleArena.PlayerVisual.PlayVictory();
                if (battleArena.EnemyVisual != null) battleArena.EnemyController?.PlayDeath();
            }

            RefreshUI();
            StartCoroutine(EndBattleRoutine(returnToTown: false));
        }

        private void HandleDefeat()
        {
            state = BattleState.Defeat;
            WriteLog("【少侠败阵】气血不支，暂回城镇休养生息...");

            if (battleArena != null)
            {
                if (battleArena.PlayerVisual != null) battleArena.PlayerController?.PlayDeath();
                if (battleArena.EnemyVisual != null) battleArena.EnemyVisual.PlayVictory();
            }

            RefreshUI();
            StartCoroutine(EndBattleRoutine(returnToTown: true));
        }

        private IEnumerator EndBattleRoutine(bool returnToTown)
        {
            yield return new WaitForSeconds(battleEndDelay);

            if (returnToTown && mapInput != null)
            {
                mapInput.ReturnPlayerToTown();
            }

            activeBattleTile = null;
            playerIsDefending = false;
            state = BattleState.Inactive;
            battleUI.Hide();

            if (battleArena != null)
            {
                battleArena.HideBattle();
            }

            if (mapInput != null)
            {
                mapInput.SetMapInputEnabled(true);
            }
        }

        private void RefreshUI()
        {
            if (battleUI == null) return;

            battleUI.RefreshHealth(
                player.CurrentHealth,
                player.MaxHealth,
                enemy.CurrentHealth,
                enemy.MaxHealth);
            battleUI.SetActionButtonsInteractable(state == BattleState.PlayerTurn);
        }

        private void WriteLog(string message)
        {
            Debug.Log(message, this);

            if (battleUI != null)
            {
                battleUI.AddLog(message);
            }
        }
    }
}

