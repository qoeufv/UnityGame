using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StrategyRPG.Flight
{
    /// <summary>Keyboard-first title menu for the standalone vertical slice.</summary>
    public sealed class PrototypeStartMenu : MonoBehaviour
    {
        private GameObject panel;
        private Text menuText;
        private Behaviour[] controlledBehaviours;
        private Action onNewGame;
        private Action onContinue;
        private bool settingsOpen;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Build(Transform canvas, Behaviour pilot, Behaviour cameraControl, Action newGame, Action continueGame)
        {
            controlledBehaviours = new[] { pilot, cameraControl };
            onNewGame = newGame;
            onContinue = continueGame;

            panel = new GameObject("Start Menu", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(.005f, .015f, .03f, .94f);

            var titleObject = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObject.transform.SetParent(panel.transform, false);
            var titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(.18f, .62f); titleRect.anchorMax = new Vector2(.82f, .87f);
            titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
            var title = titleObject.GetComponent<Text>();
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 54; title.alignment = TextAnchor.MiddleCenter; title.color = new Color(.48f, .88f, 1f);
            title.text = "GAME FRONTIER\n<size=23>SPACE EXPLORATION PROTOTYPE  /  v0.1.0</size>";

            var menuObject = new GameObject("Menu", typeof(RectTransform), typeof(Text));
            menuObject.transform.SetParent(panel.transform, false);
            var menuRect = menuObject.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(.25f, .20f); menuRect.anchorMax = new Vector2(.75f, .61f);
            menuRect.offsetMin = menuRect.offsetMax = Vector2.zero;
            menuText = menuObject.GetComponent<Text>();
            menuText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            menuText.fontSize = 27; menuText.alignment = TextAnchor.MiddleCenter; menuText.color = Color.white;
            panel.SetActive(false);
        }

        public void Show()
        {
            if (panel == null) return;
            settingsOpen = false;
            panel.SetActive(true);
            SetControls(false);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshText();
        }

        private void Update()
        {
            if (!IsOpen || Keyboard.current == null) return;
            var keys = Keyboard.current;
            if (settingsOpen)
            {
                if (keys.fKey.wasPressedThisFrame)
                {
                    Screen.fullScreen = !Screen.fullScreen;
                    RefreshText();
                }
                if (keys.mKey.wasPressedThisFrame)
                {
                    AudioListener.pause = !AudioListener.pause;
                    RefreshText();
                }
                if (keys.escapeKey.wasPressedThisFrame || keys.backspaceKey.wasPressedThisFrame)
                {
                    settingsOpen = false;
                    RefreshText();
                }
                return;
            }

            if (keys.enterKey.wasPressedThisFrame)
            {
                onContinue?.Invoke();
                Close();
            }
            else if (keys.nKey.wasPressedThisFrame)
            {
                onNewGame?.Invoke();
                Close();
            }
            else if (keys.sKey.wasPressedThisFrame)
            {
                settingsOpen = true;
                RefreshText();
            }
            else if (keys.qKey.wasPressedThisFrame)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void Close()
        {
            panel.SetActive(false);
            SetControls(true);
            Time.timeScale = 1f;
        }

        private void SetControls(bool enabled)
        {
            if (controlledBehaviours == null) return;
            foreach (var controlledBehaviour in controlledBehaviours)
                if (controlledBehaviour != null) controlledBehaviour.enabled = enabled;
        }

        private void RefreshText()
        {
            if (settingsOpen)
            {
                menuText.text = "SETTINGS\n\n[ F ]  Fullscreen: " + (Screen.fullScreen ? "ON" : "OFF") +
                    "\n[ M ]  Audio: " + (AudioListener.pause ? "MUTED" : "ON") +
                    "\n\n[ ESC / BACKSPACE ]  Back";
                return;
            }

            string continueLabel = PrototypeSessionState.HasSave ? "Continue saved expedition" : "Begin expedition";
            menuText.text = "[ ENTER ]  " + continueLabel +
                "\n\n[ N ]  New Game" +
                "\n[ S ]  Settings" +
                "\n[ Q ]  Quit" +
                "\n\n<size=18>Progress is saved automatically after discoveries and completed surveys.</size>";
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
