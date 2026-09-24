using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StrategyRPG.Flight
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SpaceFlightController : MonoBehaviour
    {
        public float cruiseSpeed = 18f;
        public float boostSpeed = 42f;
        public float acceleration = 16f;
        public float brakingAcceleration = 45f;
        public float coastDeceleration = 6f;
        public float turnSpeed = 65f;
        public float BoundRadius = 450f;
        public Vector3 boundaryCenter = Vector3.zero;

        public float Speed { get; private set; }
        public bool Boosting { get; private set; }
        public bool AtBoundary { get; private set; }
        public bool ControlsEnabled { get; set; } = true;

        private CharacterController motor;

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            motor.radius = 0.7f;
            motor.height = 2f;
            motor.center = Vector3.zero;
            motor.stepOffset = 0f;
            motor.minMoveDistance = 0f;
            motor.skinWidth = 0.05f;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (!ControlsEnabled || !Application.isFocused || keyboard == null || IsEditingText())
            {
                TickFlight(Time.deltaTime, 0f, 0f, 0f, 0f, false, true);
                return;
            }

            TickFlight(Time.deltaTime,
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f),
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.downArrowKey.isPressed ? 1f : 0f) - (keyboard.upArrowKey.isPressed ? 1f : 0f),
                (keyboard.qKey.isPressed ? 1f : 0f) - (keyboard.eKey.isPressed ? 1f : 0f),
                keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed,
                keyboard.spaceKey.isPressed);
        }

        private static bool IsEditingText()
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            return selected != null && (selected.GetComponent<InputField>() != null || selected.GetComponent("TMP_InputField") != null);
        }

        // Inputs are normalized axes. Kept separate from input polling for repeatable validation.
        public void TickFlight(float dt, float throttle, float yaw, float pitch, float roll, bool boost, bool brake)
        {
            if (dt <= 0f || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            if (motor == null) motor = GetComponent<CharacterController>();
            if (!ControlsEnabled)
            {
                Speed = 0f;
                Boosting = false;
                return;
            }

            // Substeps keep boundary checks and collision sweeps stable after long frames.
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / 0.025f), 1, 100);
            float step = Mathf.Min(dt, 2.5f) / steps;
            for (int i = 0; i < steps; i++)
                StepFlight(step, Mathf.Clamp(throttle, -1f, 1f), Mathf.Clamp(yaw, -1f, 1f),
                    Mathf.Clamp(pitch, -1f, 1f), Mathf.Clamp(roll, -1f, 1f), boost, brake);
        }

        private void StepFlight(float dt, float throttle, float yaw, float pitch, float roll, bool boost, bool brake)
        {
            transform.Rotate(pitch * turnSpeed * dt, yaw * turnSpeed * dt, roll * turnSpeed * dt, Space.Self);
            if (Mathf.Abs(roll) < 0.01f && Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up)) < 0.98f)
            {
                Quaternion level = Quaternion.LookRotation(transform.forward, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, level, 1f - Mathf.Exp(-1.5f * dt));
            }

            Boosting = boost && throttle > 0f && !brake;
            float requestedSpeed = brake ? 0f : throttle * (Boosting ? boostSpeed : cruiseSpeed);
            if (requestedSpeed < 0f) requestedSpeed *= 0.45f;
            float rate = brake ? brakingAcceleration : Mathf.Abs(throttle) < 0.01f ? coastDeceleration : acceleration;
            Speed = Mathf.MoveTowards(Speed, requestedSpeed, Mathf.Max(0f, rate) * dt);

            Vector3 fromCenter = transform.position - boundaryCenter;
            float radius = Mathf.Max(10f, BoundRadius);
            AtBoundary = fromCenter.magnitude >= radius - 20f;
            Vector3 displacement = transform.forward * (Speed * dt);
            // Block outward travel at the boundary, but always allow steering and returning inward.
            if (AtBoundary && Vector3.Dot(displacement, fromCenter) > 0f)
            {
                float remaining = Mathf.Max(0f, radius - fromCenter.magnitude - motor.radius);
                displacement *= Mathf.Clamp01(remaining / 20f);
                Boosting = false;
                if (remaining < 0.1f) Speed = 0f;
            }

            if (motor.enabled && motor.gameObject.activeInHierarchy)
            {
                CollisionFlags collision = motor.Move(displacement);
                if (collision != CollisionFlags.None)
                {
                    Speed = 0f;
                    Boosting = false;
                }
            }
        }

        public void ResetFlight(Vector3 position, Quaternion rotation)
        {
            if (motor == null) motor = GetComponent<CharacterController>();
            bool wasEnabled = motor.enabled;
            motor.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            motor.enabled = wasEnabled;
            Speed = 0f;
            Boosting = false;
            AtBoundary = Vector3.Distance(position, boundaryCenter) >= Mathf.Max(10f, BoundRadius) - 20f;
        }
    }
}
