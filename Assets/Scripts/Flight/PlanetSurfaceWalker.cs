using UnityEngine;
using UnityEngine.InputSystem;

namespace StrategyRPG.Flight
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlanetSurfaceWalker : MonoBehaviour
    {
        public float moveSpeed = 3.5f;
        public float swimSpeed = 2.4f;
        public float gravity = -14f;
        public float swimVerticalSpeed = 2.2f;
        public float maxDiveDepth = 8f;
        public float waterLevel = -.08f;
        public float platformTop = .65f;
        public Vector2 platformHalfExtents = new Vector2(18f, 16f);
        public Transform viewPivot;
        public float interactionDistance = 8.5f;
        public float lookSensitivity = .08f;
        public float maxPitch = 85f;
        private CharacterController motor;
        private float verticalVelocity;
        private float pitch;
        private float ignoreLookUntil;
        private bool controlsEnabled = true;
        public bool ControlsEnabled { get => controlsEnabled; set => controlsEnabled = value; }
        public bool IsSwimming { get; private set; }
        public bool IsUnderwater => IsSwimming && transform.position.y < waterLevel - .15f;
        public float DepthBelowWater => Mathf.Max(0f, waterLevel - transform.position.y);

        private void Awake()
        {
            motor = GetComponent<CharacterController>(); motor.height = 1.8f; motor.radius = .35f; motor.center = Vector3.up * .9f;
            motor.stepOffset = .35f; motor.slopeLimit = 55f; motor.skinWidth = .04f;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            // Ignore the editor-to-game mouse transition so the first frame cannot
            // spin the explorer away from the authored landing view.
            ignoreLookUntil = Time.unscaledTime + .35f;
        }

        private void Update()
        {
            var keys = Keyboard.current; if (keys == null || !controlsEnabled) return;
            var mouse = Mouse.current;
            if (mouse != null && viewPivot != null)
            {
                if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
                if (Cursor.lockState != CursorLockMode.Locked) return;
                if (Time.unscaledTime >= ignoreLookUntil)
                {
                    Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
                    transform.Rotate(Vector3.up, look.x);
                    pitch = Mathf.Clamp(pitch - look.y, -maxPitch, maxPitch);
                    viewPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
                }
            }
            float forward = (keys.wKey.isPressed ? 1 : 0) - (keys.sKey.isPressed ? 1 : 0);
            float strafe = (keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0);
            bool insidePlatform = Mathf.Abs(transform.position.x) < platformHalfExtents.x && Mathf.Abs(transform.position.z) < platformHalfExtents.y;
            if (!IsSwimming && transform.position.y <= waterLevel + .45f) IsSwimming = true;
            // Allow a player who swims back to the deck to regain walking controls.
            if (IsSwimming && insidePlatform && transform.position.y >= platformTop - .08f) IsSwimming = false;
            float speed = IsSwimming ? swimSpeed : moveSpeed;
            Vector3 move = (transform.forward * forward + transform.right * strafe).normalized * speed;
            if (keys.leftArrowKey.isPressed) transform.Rotate(Vector3.up, -90f * Time.deltaTime);
            if (keys.rightArrowKey.isPressed) transform.Rotate(Vector3.up, 90f * Time.deltaTime);
            if (IsSwimming)
            {
                float verticalInput = (keys.spaceKey.isPressed ? 1f : 0f)
                    - (keys.leftCtrlKey.isPressed || keys.rightCtrlKey.isPressed ? 1f : 0f);
                verticalVelocity = verticalInput == 0f
                    ? Mathf.MoveTowards(verticalVelocity, 0f, swimVerticalSpeed * 3f * Time.deltaTime)
                    : verticalInput * swimVerticalSpeed;
            }
            else if (motor.isGrounded) verticalVelocity = -1f;
            else verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity; motor.Move(move * Time.deltaTime);
            if (!IsSwimming && transform.position.y < platformTop && insidePlatform)
            {
                transform.position = new Vector3(transform.position.x, platformTop, transform.position.z); verticalVelocity = 0f;
            }
            // The controller's transform is its feet. Keep a shallow dive volume so the
            // player can put the camera below the surface and swim back up with Space.
            float minimumDive = waterLevel - maxDiveDepth;
            if (IsSwimming && transform.position.y < minimumDive)
            {
                transform.position = new Vector3(transform.position.x, minimumDive, transform.position.z);
                verticalVelocity = 0f;
            }
        }

        public Ray GetLookRay()
        {
            Transform origin = viewPivot != null ? viewPivot : transform;
            return new Ray(origin.position, origin.forward);
        }

        public bool IsWithinInteractionDistance(Transform target)
        {
            if (target == null) return false;
            var collider = target.GetComponentInChildren<Collider>();
            Vector3 nearest = collider != null ? collider.ClosestPoint(transform.position) : target.position;
            return Vector3.Distance(transform.position, nearest) <= interactionDistance;
        }

        public bool TryInteract(Transform target)
        { return IsWithinInteractionDistance(target) && Physics.Raycast(GetLookRay(), interactionDistance, ~0, QueryTriggerInteraction.Ignore); }

        public void ResetLook(float yaw = 0f)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f); pitch = 0f;
            if (viewPivot != null) viewPivot.localRotation = Quaternion.identity;
        }
    }
}
