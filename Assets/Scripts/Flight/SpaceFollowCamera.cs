using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace StrategyRPG.Flight
{
    [RequireComponent(typeof(Camera))]
    public sealed class SpaceFollowCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 12f;
        public float minDistance = 5f;
        public float maxDistance = 35f;
        public float followHeight = 3f;
        public float followSharpness = 9f;
        public float orbitSensitivity = 0.15f;
        public LayerMask collisionMask = ~0;

        private Vector2 orbit;
        private bool orbitDragging;
        private bool initialized;
        private Quaternion smoothRotation;

        private void Awake()
        {
            GetComponent<Camera>().nearClipPlane = 0.2f;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Mouse mouse = Mouse.current;
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (mouse != null && Application.isFocused)
            {
                if (mouse.rightButton.wasPressedThisFrame && !overUI) orbitDragging = true;
                if (!mouse.rightButton.isPressed || overUI) orbitDragging = false;
                if (orbitDragging)
                {
                    Vector2 delta = mouse.delta.ReadValue() * orbitSensitivity;
                    orbit.x = Mathf.Repeat(orbit.x + delta.x + 180f, 360f) - 180f;
                    orbit.y = Mathf.Clamp(orbit.y - delta.y, -65f, 65f);
                }
                if (!overUI)
                {
                    float scroll = mouse.scroll.ReadValue().y;
                    distance = Mathf.Clamp(distance - scroll * 0.025f, minDistance, maxDistance);
                }
            }
            else orbitDragging = false;

            float dt = Time.deltaTime;
            if (!orbitDragging) orbit = Vector2.Lerp(orbit, Vector2.zero, 1f - Mathf.Exp(-3f * dt));
            Quaternion desiredRotation = target.rotation * Quaternion.Euler(orbit.y, orbit.x, 0f);
            if (!initialized) { smoothRotation = desiredRotation; initialized = true; }
            smoothRotation = Quaternion.Slerp(smoothRotation, desiredRotation, 1f - Mathf.Exp(-followSharpness * dt));
            Vector3 focus = target.position + target.up * 0.6f;
            Vector3 offset = smoothRotation * new Vector3(0f, followHeight, -Mathf.Clamp(distance, minDistance, maxDistance));
            float allowedDistance = offset.magnitude;
            Vector3 direction = offset.normalized;
            RaycastHit[] hits = Physics.SphereCastAll(focus, 0.35f, direction, allowedDistance,
                collisionMask, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == target || hit.transform.IsChildOf(target)) continue;
                allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0.05f, hit.distance - 0.15f));
            }

            // Smooth the rig orientation, then resolve occlusion: positional smoothing after
            // the collision query could otherwise let the camera pass through a planet.
            transform.position = focus + direction * allowedDistance;
            transform.rotation = Quaternion.LookRotation((focus - transform.position).normalized, smoothRotation * Vector3.up);
        }

        public void ResetView()
        {
            orbit = Vector2.zero;
            orbitDragging = false;
            initialized = false;
            if (target != null) LateUpdate();
        }

        private void OnDisable() { orbitDragging = false; initialized = false; }
    }
}
