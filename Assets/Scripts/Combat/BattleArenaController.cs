using StrategyRPG.Map;
using UnityEngine;

namespace StrategyRPG.Combat
{
    /// <summary>
    /// Controls the 3D visual battle arena, terrain environment switching,
    /// combatant positioning, and battle camera transitions.
    /// Lives on BattleSystem or a persistent manager to reliably toggle BattleArenaRoot.
    /// </summary>
    public class BattleArenaController : MonoBehaviour
    {
        [Header("Root & Cameras")]
        [SerializeField] private GameObject arenaRoot;
        [SerializeField] private Camera mapCamera;
        [SerializeField] private Camera battleCamera;

        [Header("Combatant Anchors")]
        [SerializeField] private Transform playerSpot;
        [SerializeField] private Transform enemySpot;
        [SerializeField] private Transform environmentAnchor;

        [Header("3D Environment Prefabs")]
        [SerializeField] private GameObject grassEnvPrefab;
        [SerializeField] private GameObject forestEnvPrefab;
        [SerializeField] private GameObject mountainEnvPrefab;
        [SerializeField] private GameObject townEnvPrefab;

        [Header("3D Combatant Prefabs")]
        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject enemyPrefab;

        private GameObject currentEnvInstance;
        private GameObject playerInstance;
        private GameObject enemyInstance;

        private BattleCharacterVisual playerVisual;
        private BattleCharacterVisual enemyVisual;

        public BattleCharacterController PlayerController { get; private set; }
        public BattleCharacterController EnemyController { get; private set; }

        public GameObject ArenaRoot => arenaRoot;
        public Camera MapCamera => mapCamera;
        public Camera BattleCamera => battleCamera;
        public BattleCharacterVisual PlayerVisual => playerVisual;
        public BattleCharacterVisual EnemyVisual => enemyVisual;

        private void Awake()
        {
            if (mapCamera == null)
            {
                mapCamera = Camera.main;
            }

            if (arenaRoot == null)
            {
                GameObject rootObj = GameObject.Find("BattleArenaRoot");
                if (rootObj != null) arenaRoot = rootObj;
            }

            if (battleCamera == null && arenaRoot != null)
            {
                battleCamera = arenaRoot.GetComponentInChildren<Camera>(true);
            }

            SetupCombatants();
            SetArenaActive(false);
        }

        public void SetupCombatants()
        {
            if (playerSpot != null && heroPrefab != null && playerSpot.childCount == 0)
            {
                playerInstance = Instantiate(heroPrefab, playerSpot);
                playerInstance.transform.localPosition = Vector3.zero;
                playerInstance.transform.localRotation = Quaternion.Euler(0, 75f, 0); // Face towards enemy
                playerVisual = playerInstance.GetComponent<BattleCharacterVisual>();
            }
            else if (playerSpot != null && playerSpot.childCount > 0)
            {
                playerInstance = playerSpot.GetChild(0).gameObject;
                playerVisual = playerInstance.GetComponent<BattleCharacterVisual>();
            }

            if (enemySpot != null && enemyPrefab != null && enemySpot.childCount == 0)
            {
                enemyInstance = Instantiate(enemyPrefab, enemySpot);
                enemyInstance.transform.localPosition = Vector3.zero;
                enemyInstance.transform.localRotation = Quaternion.Euler(0, -75f, 0); // Face towards player
                enemyVisual = enemyInstance.GetComponent<BattleCharacterVisual>();
            }
            else if (enemySpot != null && enemySpot.childCount > 0)
            {
                enemyInstance = enemySpot.GetChild(0).gameObject;
                enemyVisual = enemyInstance.GetComponent<BattleCharacterVisual>();
            }

            PlayerController = GetController(playerVisual);
            EnemyController = GetController(enemyVisual);
        }

        private static BattleCharacterController GetController(BattleCharacterVisual visual)
        {
            if (visual == null) return null;
            BattleCharacterController controller = visual.GetComponent<BattleCharacterController>();
            return controller != null ? controller : visual.gameObject.AddComponent<BattleCharacterController>();
        }

        /// <summary>
        /// Activates the 3D battle arena, switches camera, and loads terrain stage.
        /// </summary>
        public void ShowBattle(TerrainType terrainType)
        {
            SetArenaActive(true);
            SwitchEnvironment(terrainType);
            SetupCombatants();

            // Reset combatant animation states
            if (PlayerController != null) PlayerController.PlayIdle();
            if (EnemyController != null) EnemyController.PlayIdle();
        }

        /// <summary>
        /// Deactivates the battle arena and restores the map camera.
        /// </summary>
        public void HideBattle()
        {
            SetArenaActive(false);
        }

        public void SetArenaActive(bool isActive)
        {
            if (arenaRoot != null)
            {
                arenaRoot.SetActive(isActive);
            }

            if (battleCamera != null)
            {
                battleCamera.gameObject.SetActive(isActive);
                battleCamera.enabled = isActive;
            }

            if (mapCamera != null)
            {
                mapCamera.gameObject.SetActive(!isActive);
                mapCamera.enabled = !isActive;
            }
        }

        private void SwitchEnvironment(TerrainType terrain)
        {
            if (currentEnvInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(currentEnvInstance);
                }
                else
                {
                    DestroyImmediate(currentEnvInstance);
                }
                currentEnvInstance = null;
            }

            GameObject selectedPrefab = grassEnvPrefab;
            switch (terrain)
            {
                case TerrainType.Grass:
                    selectedPrefab = grassEnvPrefab;
                    break;
                case TerrainType.Forest:
                    selectedPrefab = forestEnvPrefab;
                    break;
                case TerrainType.Mountain:
                    selectedPrefab = mountainEnvPrefab;
                    break;
                case TerrainType.Town:
                    selectedPrefab = townEnvPrefab;
                    break;
                default:
                    selectedPrefab = grassEnvPrefab;
                    break;
            }

            if (selectedPrefab != null && environmentAnchor != null)
            {
                currentEnvInstance = Instantiate(selectedPrefab, environmentAnchor);
                currentEnvInstance.transform.localPosition = Vector3.zero;
                currentEnvInstance.transform.localRotation = Quaternion.identity;
            }
        }
    }
}


