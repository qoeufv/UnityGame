using System;
using System.Collections.Generic;
using StrategyRPG.Combat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace StrategyRPG.UI
{
    /// <summary>
    /// Creates and controls the Chinese Wuxia-themed Turn-Based RPG battle interface.
    /// Handles visual hierarchy, sub-skill drawers, equipment slot preview, and status effect indicators.
    /// Pure presentation layer; BattleManager handles all combat mechanics and rolls.
    /// </summary>
    public class BattleUIController : MonoBehaviour
    {
        private const int MaxLogLines = 8;

        private readonly Queue<string> logLines = new Queue<string>();

        private GameObject canvasObject;
        private BattleManager boundBattleManager;
        private Font uiFont;

        // Top Status Elements
        private Text playerNameText;
        private Text playerHpText;
        private Image playerHpBarFill;
        private Image playerMpBarFill;
        private Text playerMpText;
        private Transform playerStatusTray;

        private Text enemyNameText;
        private Text enemyHpText;
        private Image enemyHpBarFill;
        private Transform enemyStatusTray;

        // Command Buttons (Main 6 + Equip)
        private Button basicAttackButton;
        private Button martialArtsButton;
        private Button innerSkillButton;
        private Button qinggongButton;
        private Button defendButton;
        private Button inventoryButton;
        private Button equipmentPreviewButton;

        // Sub-Action Drawer
        private GameObject subDrawerObject;
        private Text subDrawerTitleText;
        private Transform subSkillListParent;
        private Button subDrawerCloseButton;

        // Equipment Preview Modal
        private GameObject equipDrawerObject;

        // Combat Log
        private Text combatLogText;

        private void Awake()
        {
            BuildWuxiaUI();
            Hide();
        }

        public void Initialize(BattleManager battleManager)
        {
            boundBattleManager = battleManager;

            // Wire Primary Action Buttons
            basicAttackButton.onClick.RemoveAllListeners();
            martialArtsButton.onClick.RemoveAllListeners();
            innerSkillButton.onClick.RemoveAllListeners();
            qinggongButton.onClick.RemoveAllListeners();
            defendButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.RemoveAllListeners();
            equipmentPreviewButton.onClick.RemoveAllListeners();

            basicAttackButton.onClick.AddListener(() =>
            {
                CloseSubDrawers();
                battleManager.PlayerAttack();
            });

            martialArtsButton.onClick.AddListener(() => OpenSubSkillDrawer("武学招式 · Martial Arts", GetMartialArtsSkills()));
            innerSkillButton.onClick.AddListener(() => OpenSubSkillDrawer("内功心法 · Inner Qi", GetInnerSkills()));
            qinggongButton.onClick.AddListener(() => OpenSubSkillDrawer("轻功身法 · Qinggong", GetQinggongSkills()));
            inventoryButton.onClick.AddListener(() => OpenSubSkillDrawer("行囊丹药 · Inventory", GetInventoryItems()));

            defendButton.onClick.AddListener(() =>
            {
                CloseSubDrawers();
                battleManager.PlayerDefend();
            });

            equipmentPreviewButton.onClick.AddListener(ToggleEquipmentPreview);
        }

        public void Show()
        {
            if (canvasObject != null)
            {
                canvasObject.SetActive(true);
                CloseSubDrawers();
            }
        }

        public void Hide()
        {
            if (canvasObject != null)
            {
                canvasObject.SetActive(false);
                CloseSubDrawers();
            }
        }

        public void RefreshHealth(int playerHealth, int playerMaxHealth, int enemyHealth, int enemyMaxHealth)
        {
            if (playerHpText != null)
            {
                playerHpText.text = $"气血 (HP): {playerHealth} / {playerMaxHealth}";
            }

            if (playerHpBarFill != null)
            {
                playerHpBarFill.fillAmount = Mathf.Clamp01((float)playerHealth / Mathf.Max(1, playerMaxHealth));
            }

            if (enemyHpText != null)
            {
                enemyHpText.text = $"气血 (HP): {enemyHealth} / {enemyMaxHealth}";
            }

            if (enemyHpBarFill != null)
            {
                enemyHpBarFill.fillAmount = Mathf.Clamp01((float)enemyHealth / Mathf.Max(1, enemyMaxHealth));
            }
        }

        public void SetActionButtonsInteractable(bool interactable)
        {
            basicAttackButton.interactable = interactable;
            martialArtsButton.interactable = interactable;
            innerSkillButton.interactable = interactable;
            qinggongButton.interactable = interactable;
            defendButton.interactable = interactable;
            inventoryButton.interactable = interactable;
            equipmentPreviewButton.interactable = interactable;

            if (!interactable)
            {
                CloseSubDrawers();
            }
        }

        public void ClearLog()
        {
            logLines.Clear();
            if (combatLogText != null) combatLogText.text = string.Empty;
        }

        public void AddLog(string message)
        {
            logLines.Enqueue(message);

            while (logLines.Count > MaxLogLines)
            {
                logLines.Dequeue();
            }

            if (combatLogText != null)
            {
                combatLogText.text = string.Join("\n", logLines);
            }
        }

        #region Sub-Drawer Management
        private struct SubSkillEntry
        {
            public string Name;
            public string Description;
            public string Cost;
            public Action OnSelect;
        }

        private List<SubSkillEntry> GetMartialArtsSkills()
        {
            return new List<SubSkillEntry>
            {
                new SubSkillEntry
                {
                    Name = "烈阳掌 (Flame Sun Palm)",
                    Description = "至阳掌力，附带烈火劲气 [灼烧]",
                    Cost = "真气: 15",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【招式】少侠施展「烈阳掌」，掌劲如烈日焚空！");
                        boundBattleManager?.PlayerSkill(SkillCategory.MartialArt);
                    }
                },
                new SubSkillEntry
                {
                    Name = "清风剑诀 (Sword Technique)",
                    Description = "灵动飘逸，快剑无影刺破破绽",
                    Cost = "真气: 12",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【招式】少侠施展「清风剑诀」，剑光如秋水荡漾！");
                        boundBattleManager?.PlayerSkill(SkillCategory.MartialArt);
                    }
                },
                new SubSkillEntry
                {
                    Name = "八卦刀法 (Saber Technique)",
                    Description = "刀势雄浑霸道，直斩筋脉 [破甲]",
                    Cost = "真气: 18",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【招式】少侠施展「八卦刀法」，重刀破空呼啸！");
                        boundBattleManager?.PlayerSkill(SkillCategory.MartialArt);
                    }
                }
            };
        }

        private List<SubSkillEntry> GetInnerSkills()
        {
            return new List<SubSkillEntry>
            {
                new SubSkillEntry
                {
                    Name = "紫霞真气 (Purple Cloud Qi)",
                    Description = "运转玄门正宗心法，调息恢复大量气血",
                    Cost = "真气: 20",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【心法】少侠运转「紫霞真气」，气血周天流转！");
                        boundBattleManager?.PlayerHeal();
                    }
                },
                new SubSkillEntry
                {
                    Name = "龟息功 (Turtle Breath)",
                    Description = "敛息静气，大幅增强防御与招架",
                    Cost = "真气: 10",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【心法】少侠运转「龟息功」，凝神聚气坚若磐石！");
                        boundBattleManager?.PlayerDefend();
                    }
                },
                new SubSkillEntry
                {
                    Name = "纯阳无极 (Pure Yang Surge)",
                    Description = "爆发纯阳气劲，提升下回合威力 [攻击强化]",
                    Cost = "真气: 25",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【心法】少侠运转「纯阳无极」，气劲暴涨！");
                        boundBattleManager?.PlayerSkill(SkillCategory.InnerSkill);
                    }
                }
            };
        }

        private List<SubSkillEntry> GetQinggongSkills()
        {
            return new List<SubSkillEntry>
            {
                new SubSkillEntry
                {
                    Name = "梯云纵 (Cloud Step)",
                    Description = "踏风借力，身法腾空，闪避率大幅提高",
                    Cost = "身法: 10",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【身法】少侠施展「梯云纵」，身轻如燕避开锋芒！");
                        boundBattleManager?.PlayerQinggong();
                    }
                },
                new SubSkillEntry
                {
                    Name = "凌波微步 (Shadowless Drift)",
                    Description = "罗袜生尘，动无常则，留下残影戏敌",
                    Cost = "身法: 20",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【身法】少侠施展「凌波微步」，残影重重！");
                        boundBattleManager?.PlayerQinggong();
                    }
                }
            };
        }

        private List<SubSkillEntry> GetInventoryItems()
        {
            return new List<SubSkillEntry>
            {
                new SubSkillEntry
                {
                    Name = "金创药 (Healing Medicine)",
                    Description = "江湖良药，服下可迅速止血恢复气血",
                    Cost = "库存: 3",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【道具】少侠使用了「金创药」，伤口迅速愈合！");
                        boundBattleManager?.PlayerHeal();
                    }
                },
                new SubSkillEntry
                {
                    Name = "聚气散 (Power Medicine)",
                    Description = "百年灵药炼制，激发周身内力",
                    Cost = "库存: 2",
                    OnSelect = () =>
                    {
                        CloseSubDrawers();
                        AddLog("【道具】少侠服下了「聚气散」，内息充盈！");
                        boundBattleManager?.PlayerSkill(SkillCategory.InnerSkill);
                    }
                }
            };
        }

        private void OpenSubSkillDrawer(string title, List<SubSkillEntry> entries)
        {
            if (subDrawerObject == null) return;

            subDrawerTitleText.text = title;

            // Clear old buttons
            for (int i = subSkillListParent.childCount - 1; i >= 0; i--)
            {
                Destroy(subSkillListParent.GetChild(i).gameObject);
            }

            foreach (var entry in entries)
            {
                GameObject itemObj = CreateUIObject("SkillEntry", subSkillListParent, typeof(Image), typeof(Button));
                RectTransform rt = itemObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(430f, 65f);
                itemObj.GetComponent<Image>().color = new Color(0.16f, 0.12f, 0.10f, 0.95f);

                // Name & Cost
                CreateAnchoredText("Name", itemObj.transform, entry.Name, 17, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10f, -8f), new Vector2(280f, 25f), Color.yellow);
                CreateAnchoredText("Cost", itemObj.transform, entry.Cost, 15, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10f, -8f), new Vector2(120f, 25f), new Color(0.9f, 0.75f, 0.5f));
                CreateAnchoredText("Desc", itemObj.transform, entry.Description, 14, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(10f, 8f), new Vector2(410f, 25f), new Color(0.85f, 0.85f, 0.85f));

                Button btn = itemObj.GetComponent<Button>();
                btn.onClick.AddListener(() => entry.OnSelect?.Invoke());
            }

            subDrawerObject.SetActive(true);
            if (equipDrawerObject != null) equipDrawerObject.SetActive(false);
        }

        private void ToggleEquipmentPreview()
        {
            if (equipDrawerObject == null) return;
            bool nextState = !equipDrawerObject.activeSelf;
            equipDrawerObject.SetActive(nextState);
            if (subDrawerObject != null) subDrawerObject.SetActive(false);
        }

        private void CloseSubDrawers()
        {
            if (subDrawerObject != null) subDrawerObject.SetActive(false);
            if (equipDrawerObject != null) equipDrawerObject.SetActive(false);
        }
        #endregion

        #region UI Construction
        private void BuildWuxiaUI()
        {
            EnsureEventSystem();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            canvasObject = new GameObject("BattleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            BuildTopStatusCards(canvasObject.transform);
            BuildBottomCommandDock(canvasObject.transform);
            BuildCombatLog(canvasObject.transform);
            BuildSubDrawer(canvasObject.transform);
            BuildEquipmentDrawer(canvasObject.transform);
        }

        private void BuildTopStatusCards(Transform parent)
        {
            // Player Card (Top Left, strictly anchored and pivoted at (0, 1))
            GameObject playerCard = CreatePanel("PlayerCard", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(35f, -30f), new Vector2(460f, 180f), new Color(0.10f, 0.12f, 0.15f, 0.92f));
            
            playerNameText = CreateAnchoredText("PlayerName", playerCard.transform, "少侠 · 凌云 [初窥门径]", 21, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -16f), new Vector2(340f, 28f), new Color(0.95f, 0.85f, 0.60f));
            
            // Player HP Bar
            playerHpBarFill = CreateStatBar("PlayerHpBar", playerCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -54f), new Vector2(340f, 22f), new Color(0.80f, 0.20f, 0.20f));
            playerHpText = CreateAnchoredText("PlayerHpText", playerCard.transform, "气血 (HP): 100 / 100", 15, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -54f), new Vector2(320f, 22f), Color.white);
            
            // Player MP / Qi Bar
            playerMpBarFill = CreateStatBar("PlayerMpBar", playerCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -82f), new Vector2(340f, 18f), new Color(0.20f, 0.55f, 0.85f));
            playerMpText = CreateAnchoredText("PlayerMpText", playerCard.transform, "真气 (Qi): 60 / 60", 13, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -82f), new Vector2(320f, 18f), Color.white);

            // Player Status Tray (Bottom of Player Card)
            playerStatusTray = CreateUIObject("StatusTray", playerCard.transform, typeof(HorizontalLayoutGroup)).transform;
            RectTransform trayRect = playerStatusTray.GetComponent<RectTransform>();
            trayRect.anchorMin = new Vector2(0f, 0f);
            trayRect.anchorMax = new Vector2(0f, 0f);
            trayRect.pivot = new Vector2(0f, 0f);
            trayRect.anchoredPosition = new Vector2(18f, 16f);
            trayRect.sizeDelta = new Vector2(420f, 30f);

            var hlgPlayer = playerStatusTray.GetComponent<HorizontalLayoutGroup>();
            hlgPlayer.spacing = 8f;
            hlgPlayer.childControlWidth = false;
            hlgPlayer.childControlHeight = false;

            CreateStatusBadge(playerStatusTray, "蓄力", new Color(0.9f, 0.4f, 0.1f));
            CreateStatusBadge(playerStatusTray, "护体", new Color(0.2f, 0.7f, 0.3f));
            CreateStatusBadge(playerStatusTray, "灵动", new Color(0.2f, 0.6f, 0.9f));

            // Enemy Card (Top Right, strictly anchored and pivoted at (1, 1))
            GameObject enemyCard = CreatePanel("EnemyCard", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-35f, -30f), new Vector2(460f, 180f), new Color(0.15f, 0.10f, 0.10f, 0.92f));

            enemyNameText = CreateAnchoredText("EnemyName", enemyCard.transform, "黑风寨头目 · 厉啸天 [强敌]", 21, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -16f), new Vector2(360f, 28f), new Color(0.95f, 0.40f, 0.35f));
            
            // Enemy HP Bar
            enemyHpBarFill = CreateStatBar("EnemyHpBar", enemyCard.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -54f), new Vector2(340f, 22f), new Color(0.80f, 0.20f, 0.20f), isRightAligned: true);
            enemyHpText = CreateAnchoredText("EnemyHpText", enemyCard.transform, "气血 (HP): 60 / 60", 15, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -54f), new Vector2(320f, 22f), Color.white);

            // Enemy Status Tray (Bottom of Enemy Card)
            enemyStatusTray = CreateUIObject("EnemyStatusTray", enemyCard.transform, typeof(HorizontalLayoutGroup)).transform;
            RectTransform eTrayRect = enemyStatusTray.GetComponent<RectTransform>();
            eTrayRect.anchorMin = new Vector2(1f, 0f);
            eTrayRect.anchorMax = new Vector2(1f, 0f);
            eTrayRect.pivot = new Vector2(1f, 0f);
            eTrayRect.anchoredPosition = new Vector2(-18f, 16f);
            eTrayRect.sizeDelta = new Vector2(420f, 30f);

            var hlgEnemy = enemyStatusTray.GetComponent<HorizontalLayoutGroup>();
            hlgEnemy.spacing = 8f;
            hlgEnemy.childAlignment = TextAnchor.MiddleRight;
            hlgEnemy.childControlWidth = false;
            hlgEnemy.childControlHeight = false;

            CreateStatusBadge(enemyStatusTray, "灼烧", new Color(0.95f, 0.3f, 0.1f));
            CreateStatusBadge(enemyStatusTray, "虚弱", new Color(0.6f, 0.3f, 0.7f));
        }

        private void BuildBottomCommandDock(Transform parent)
        {
            // Command Dock anchored at bottom center (0.5, 0), pivot (0.5, 0)
            GameObject dock = CreatePanel("CommandDock", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(1020f, 120f), new Color(0.12f, 0.10f, 0.08f, 0.96f));
            
            // Header title
            CreateAnchoredText("DockHeader", dock.transform, "◆ 武林对决 · 见招拆招 ◆", 16, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(400f, 22f), new Color(0.85f, 0.75f, 0.50f));

            // Equipment Inspect Button on top right of dock
            equipmentPreviewButton = CreateWuxiaButton("EquipPreview", dock.transform, "装束兵刃 [装备]", new Vector2(430f, 40f), new Color(0.38f, 0.35f, 0.28f), new Vector2(130f, 28f), 13);

            // 6 Core Action Buttons
            float startX = -375f;
            float spacing = 150f;

            basicAttackButton = CreateWuxiaButton("BasicAttack", dock.transform, "普通攻击\n[基础招式]", new Vector2(startX + 0 * spacing, -16f), new Color(0.65f, 0.25f, 0.20f), new Vector2(135f, 65f), 15);
            martialArtsButton = CreateWuxiaButton("MartialArts", dock.transform, "武学招式\n[掌/剑/刀]", new Vector2(startX + 1 * spacing, -16f), new Color(0.75f, 0.45f, 0.15f), new Vector2(135f, 65f), 15);
            innerSkillButton = CreateWuxiaButton("InnerSkill", dock.transform, "内功心法\n[调息聚气]", new Vector2(startX + 2 * spacing, -16f), new Color(0.25f, 0.50f, 0.75f), new Vector2(135f, 65f), 15);
            qinggongButton = CreateWuxiaButton("Qinggong", dock.transform, "轻功身法\n[踏风闪避]", new Vector2(startX + 3 * spacing, -16f), new Color(0.20f, 0.60f, 0.50f), new Vector2(135f, 65f), 15);
            defendButton = CreateWuxiaButton("Defend", dock.transform, "凝神招架\n[减伤防守]", new Vector2(startX + 4 * spacing, -16f), new Color(0.40f, 0.45f, 0.50f), new Vector2(135f, 65f), 15);
            inventoryButton = CreateWuxiaButton("Inventory", dock.transform, "行囊丹药\n[恢复增益]", new Vector2(startX + 5 * spacing, -16f), new Color(0.55f, 0.35f, 0.60f), new Vector2(135f, 65f), 15);
        }

        private void BuildCombatLog(Transform parent)
        {
            // Combat Log anchored at bottom left (0, 0), pivot (0, 0)
            GameObject logBox = CreatePanel("CombatLogBox", parent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(35f, 25f), new Vector2(390f, 210f), new Color(0.08f, 0.08f, 0.10f, 0.90f));
            CreateAnchoredText("LogTitle", logBox.transform, "【战况实录】", 16, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -12f), new Vector2(200f, 22f), new Color(0.9f, 0.8f, 0.6f));
            
            combatLogText = CreateAnchoredText("LogContent", logBox.transform, "江湖对决一触即发...\n请少侠选择招式。", 14, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -38f), new Vector2(362f, 160f), new Color(0.92f, 0.92f, 0.92f));
        }

        private void BuildSubDrawer(Transform parent)
        {
            // Sub-drawer centered above dock
            subDrawerObject = CreatePanel("SubSkillDrawer", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 160f), new Vector2(480f, 280f), new Color(0.12f, 0.10f, 0.08f, 0.98f));
            subDrawerTitleText = CreateAnchoredText("DrawerTitle", subDrawerObject.transform, "武学招式", 19, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(300f, 26f), new Color(0.95f, 0.80f, 0.45f));

            // Close Button
            subDrawerCloseButton = CreateWuxiaButton("CloseBtn", subDrawerObject.transform, "关闭 ×", new Vector2(195f, 115f), new Color(0.4f, 0.2f, 0.2f), new Vector2(65f, 30f), 13);
            subDrawerCloseButton.onClick.AddListener(CloseSubDrawers);

            // Skill List Root
            subSkillListParent = CreateUIObject("SkillList", subDrawerObject.transform, typeof(VerticalLayoutGroup)).transform;
            RectTransform listRect = subSkillListParent.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.pivot = new Vector2(0.5f, 0.5f);
            listRect.anchoredPosition = new Vector2(0f, -15f);
            listRect.sizeDelta = new Vector2(440f, 205f);

            var vlg = subSkillListParent.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            subDrawerObject.SetActive(false);
        }

        private void BuildEquipmentDrawer(Transform parent)
        {
            // Equipment modal anchored bottom right (1, 0), pivot (1, 0)
            equipDrawerObject = CreatePanel("EquipmentModal", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-35f, 25f), new Vector2(360f, 320f), new Color(0.10f, 0.10f, 0.12f, 0.98f));

            CreateAnchoredText("EquipTitle", equipDrawerObject.transform, "【侠客行装 · 装备一览】", 18, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -14f), new Vector2(300f, 26f), new Color(0.95f, 0.85f, 0.55f));

            string[] slots = new string[]
            {
                "冠帽 (Helmet): 斗笠 [防御+5]",
                "战袍 (Chest Armor): 青布长衫 [气血+30]",
                "护腕 (Bracer): 熟铜护腕 [招架+8%]",
                "兵刃 (Weapon): 龙泉青锋剑 [刀/剑/枪/拳]",
                "玉佩 (Necklace): 羊脂白玉佩 [真气+20]",
                "戒指 (Ring): 玄铁戒 [会心+5%]"
            };

            for (int i = 0; i < slots.Length; i++)
            {
                CreateAnchoredText($"Slot_{i}", equipDrawerObject.transform, $"◈ {slots[i]}", 15, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -50f - i * 36f), new Vector2(328f, 28f), new Color(0.85f, 0.88f, 0.92f));
            }

            Button closeEquip = CreateWuxiaButton("CloseEquip", equipDrawerObject.transform, "收起", new Vector2(120f, -275f), new Color(0.35f, 0.35f, 0.35f), new Vector2(80f, 30f), 13);
            closeEquip.onClick.AddListener(() => equipDrawerObject.SetActive(false));

            equipDrawerObject.SetActive(false);
        }
        #endregion

        #region UI Helper Methods
        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject panel = CreateUIObject(name, parent, typeof(Image));
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Image CreateStatBar(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color fillColor, bool isRightAligned = false)
        {
            // Background
            GameObject barBg = CreateUIObject(name + "_Bg", parent, typeof(Image));
            RectTransform bgRt = barBg.GetComponent<RectTransform>();
            bgRt.anchorMin = anchorMin;
            bgRt.anchorMax = anchorMax;
            bgRt.pivot = pivot;
            bgRt.anchoredPosition = anchoredPos;
            bgRt.sizeDelta = size;
            barBg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.85f);

            // Fill
            GameObject barFill = CreateUIObject(name + "_Fill", barBg.transform, typeof(Image));
            RectTransform fillRt = barFill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.pivot = isRightAligned ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
            fillRt.sizeDelta = Vector2.zero;

            Image fillImg = barFill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = isRightAligned ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;
            fillImg.color = fillColor;

            return fillImg;
        }

        private void CreateStatusBadge(Transform parent, string title, Color color)
        {
            GameObject badge = CreateUIObject("Badge_" + title, parent, typeof(Image));
            RectTransform rt = badge.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48f, 24f);
            badge.GetComponent<Image>().color = color;

            CreateAnchoredText("Label", badge.transform, title, 13, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, rt.sizeDelta, Color.white);
        }

        private Button CreateWuxiaButton(string objectName, Transform parent, string label, Vector2 anchoredPosition, Color btnColor, Vector2? size = null, int fontSize = 15)
        {
            Vector2 btnSize = size ?? new Vector2(135f, 65f);
            GameObject buttonObject = CreateUIObject(objectName, parent, typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = btnSize;

            Image image = buttonObject.GetComponent<Image>();
            image.color = btnColor;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            CreateAnchoredText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, btnSize, Color.white);
            return button;
        }

        private GameObject CreateUIObject(string objectName, Transform parent, params Type[] components)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);

            foreach (Type componentType in components)
            {
                uiObject.AddComponent(componentType);
            }

            return uiObject;
        }

        private Text CreateAnchoredText(
            string objectName,
            Transform parent,
            string initialText,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size,
            Color? textColor = null)
        {
            GameObject textObject = CreateUIObject(objectName, parent, typeof(Text));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = uiFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = textColor ?? Color.white;
            text.text = initialText;
            text.raycastTarget = false;
            return text;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(transform, false);
        }
        #endregion
    }
}


