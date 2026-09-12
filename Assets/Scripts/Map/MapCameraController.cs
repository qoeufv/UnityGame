using UnityEngine;
using UnityEngine.InputSystem;

namespace StrategyRPG.Map
{
    /// <summary>
    /// Civilisation-style camera controls for the XY hex map.
    /// Right-drag pans, Shift + right-drag orbits, and the wheel zooms.
    /// The map itself never moves or rotates.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MapCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField, Min(0.001f)] private float panSensitivity = 0.0035f;

        [Header("Orbit")]
        [SerializeField, Min(0.01f)] private float orbitSensitivity = 0.15f;
        [SerializeField, Range(0f, 89f)] private float minimumTilt = 5f;
        [SerializeField, Range(1f, 89f)] private float maximumTilt = 65f;

        [Header("Zoom")]
        [SerializeField, Min(0.01f)] private float zoomSensitivity = 0.45f;
        [SerializeField, Min(0.1f)] private float minimumOrthographicSize = 2f;
        [SerializeField, Min(0.1f)] private float maximumOrthographicSize = 8f;
        [SerializeField, Min(0.1f)] private float minimumDistance = 4f;
        [SerializeField, Min(0.1f)] private float maximumDistance = 18f;

        private Camera controlledCamera;
        private Vector3 focusPoint;
        private float orbitDistance;
        private float yaw;
        private float tilt;

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();
            ReadCurrentCameraPose();
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            Vector2 pointerDelta = Mouse.current.delta.ReadValue();
            if (Mouse.current.rightButton.isPressed && pointerDelta.sqrMagnitude > 0f)
            {
                if (IsOrbitModifierPressed())
                {
                    Orbit(pointerDelta);
                }
                else
                {
                    Pan(pointerDelta);
                }
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (!Mathf.Approximately(scroll, 0f))
            {
                Zoom(scroll / 120f);
            }
        }

        private void ReadCurrentCameraPose()
        {
            Plane mapPlane = new Plane(Vector3.forward, Vector3.zero);
            Ray cameraRay = new Ray(transform.position, transform.forward);

            if (mapPlane.Raycast(cameraRay, out float distanceToMap))
            {
                focusPoint = cameraRay.GetPoint(distanceToMap);
            }
            else
            {
                focusPoint = Vector3.zero;
            }

            Vector3 offset = transform.position - focusPoint;
            orbitDistance = Mathf.Clamp(offset.magnitude, minimumDistance, maximumDistance);

            Vector3 offsetDirection = offset.sqrMagnitude > 0f ? offset.normalized : Vector3.back;
            tilt = Mathf.Clamp(Vector3.Angle(Vector3.back, offsetDirection), minimumTilt, maximumTilt);

            Vector3 planarDirection = Vector3.ProjectOnPlane(offsetDirection, Vector3.forward);
            yaw = planarDirection.sqrMagnitude > 0f
                ? Vector3.SignedAngle(Vector3.down, planarDirection.normalized, Vector3.back)
                : 0f;
        }

        private void Pan(Vector2 pointerDelta)
        {
            Vector3 planarRight = Vector3.ProjectOnPlane(transform.right, Vector3.forward).normalized;
            Vector3 planarUp = Vector3.ProjectOnPlane(transform.up, Vector3.forward).normalized;

            float zoomScale = controlledCamera.orthographic
                ? controlledCamera.orthographicSize
                : orbitDistance * 0.5f;

            focusPoint +=
                (-planarRight * pointerDelta.x - planarUp * pointerDelta.y) *
                panSensitivity * zoomScale;

            ApplyCameraPose();
        }

        private void Orbit(Vector2 pointerDelta)
        {
            yaw += pointerDelta.x * orbitSensitivity;
            tilt = Mathf.Clamp(
                tilt - pointerDelta.y * orbitSensitivity,
                minimumTilt,
                maximumTilt);

            ApplyCameraPose();
        }

        private void Zoom(float scrollSteps)
        {
            if (controlledCamera.orthographic)
            {
                controlledCamera.orthographicSize = Mathf.Clamp(
                    controlledCamera.orthographicSize - scrollSteps * zoomSensitivity,
                    minimumOrthographicSize,
                    maximumOrthographicSize);
            }
            else
            {
                orbitDistance = Mathf.Clamp(
                    orbitDistance - scrollSteps * zoomSensitivity,
                    minimumDistance,
                    maximumDistance);
                ApplyCameraPose();
            }
        }

        private void ApplyCameraPose()
        {
            float tiltRadians = tilt * Mathf.Deg2Rad;
            Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.back);
            Vector3 planarBack = yawRotation * Vector3.down;

            Vector3 offset =
                Vector3.back * Mathf.Cos(tiltRadians) * orbitDistance +
                planarBack * Mathf.Sin(tiltRadians) * orbitDistance;

            transform.position = focusPoint + offset;
            transform.rotation = Quaternion.LookRotation(-offset.normalized, -planarBack);
        }

        private static bool IsOrbitModifierPressed()
        {
            if (Keyboard.current == null) return false;

            return Keyboard.current.leftShiftKey.isPressed ||
                   Keyboard.current.rightShiftKey.isPressed;
        }
    }
}
