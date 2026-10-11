using UnityEngine;
using UnityEngine.UI;

namespace StrategyRPG.Flight
{
    /// <summary>Shared lightweight pause presentation for runtime-built prototype scenes.</summary>
    public sealed class PrototypePauseOverlay : MonoBehaviour
    {
        private GameObject panel;
        private Behaviour[] controlledBehaviours;
        private bool lockCursorOnResume;

        public bool IsPaused { get; private set; }

        public void Build(Transform canvas, string secondaryAction, bool lockCursor, params Behaviour[] controls)
        {
            controlledBehaviours = controls;
            lockCursorOnResume = lockCursor;
            panel = new GameObject("Pause Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.28f, .32f); rect.anchorMax = new Vector2(.72f, .68f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(.015f, .035f, .055f, .94f);

            var textObject = new GameObject("Pause Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(.08f, .08f); textRect.anchorMax = new Vector2(.92f, .92f);
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
            text.text = "PAUSED\n\nESC  Resume" + (string.IsNullOrEmpty(secondaryAction) ? string.Empty : "\n" + secondaryAction);
            panel.SetActive(false);
        }

        public void Toggle()
        {
            SetPaused(!IsPaused);
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            if (controlledBehaviours != null)
                foreach (var controlledBehaviour in controlledBehaviours)
                    if (controlledBehaviour != null) controlledBehaviour.enabled = !paused;
            if (panel != null) panel.SetActive(paused);
            Cursor.lockState = paused ? CursorLockMode.None : lockCursorOnResume ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = paused || !lockCursorOnResume;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
