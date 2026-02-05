using UnityEngine;
using UnityEngine.InputSystem;
namespace Game.Player.DesktopFps
{
    [DisallowMultipleComponent]
    public sealed class CameraController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Player body transform that receives yaw rotation. If null, uses this transform.")]
        [SerializeField] private Transform playerBody;
        [Tooltip("Camera pivot transform that receives pitch rotation.")]
        [SerializeField] private Transform cameraPivot;
        [Tooltip("Optional Camera transform. Only used for position/forward if set; otherwise pivot is used.")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("InputReader handles player input")]
        [SerializeField] private InputSystem.InputReader inputReader;
        [Tooltip("Capsule controller for tracking crouch height changes")]
        [SerializeField] private PlayerCapsuleController capsuleController;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.08f;
        [SerializeField] private float minPitch = -85.0f;
        [SerializeField] private float maxPitch = 85.0f;

        [Header("Crouch Camera Settings")]
        [Tooltip("How far forward the camera moves when crouching")]
        [SerializeField] private float crouchForwardOffset = 0.2f;
        [Tooltip("How fast the camera transitions during crouch/stand (units per second). Lower = slower/smoother")]
        [SerializeField] private float crouchTransitionSpeed = 0.8f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnEnable = true;

        private float yaw;
        private float pitch;
        private float baseCameraPivotY;
        private float baseCameraPivotZ;
        private float currentForwardOffset;
        private float currentVerticalOffset;

        private void Awake()
        {
            if (playerBody == null)
            {
                playerBody = transform;
            }

            if (cameraPivot == null)
            {
                Debug.LogError($"{nameof(CameraController)}: cameraPivot is not set.", this);
                enabled = false;
                return;
            }

            if (cameraTransform == null)
            {
                cameraTransform = cameraPivot;
            }

            // Store baseline camera pivot Y and Z for capsule height and crouch forward offset tracking
            baseCameraPivotY = cameraPivot.localPosition.y;
            baseCameraPivotZ = cameraPivot.localPosition.z;

            // Initialize current offsets to zero (start at standing position)
            currentForwardOffset = 0f;
            currentVerticalOffset = 0f;

            var euler = playerBody.rotation.eulerAngles;
            yaw = euler.y;

            pitch = cameraPivot.localRotation.eulerAngles.x;
            if (pitch > 180.0f)
            {
                pitch -= 360.0f;
            }
        }

        private void OnEnable()
        {
            ApplyCursorLock(lockCursorOnEnable);

            if (inputReader != null)
            {
                inputReader.onLook += OnLook;
            }
        }

        private void OnDisable()
        {
            ApplyCursorLock(false);

            if (inputReader != null)
            {
                inputReader.onLook -= OnLook;
            }
        }

        private void LateUpdate()
        {
            if (cameraPivot == null)
            {
                return;
            }

            // Determine target offsets based on crouch state
            float targetForwardOffset = 0f;
            float targetVerticalOffset = 0f;

            if (capsuleController != null)
            {
                if (capsuleController.IsCrouching)
                {
                    targetForwardOffset = crouchForwardOffset;
                }

                // Calculate vertical offset from capsule height change
                targetVerticalOffset = capsuleController.CurrentHeight - capsuleController.StandingHeight;
            }

            // Smoothly move toward target offsets at a constant speed
            currentForwardOffset = Mathf.MoveTowards(currentForwardOffset, targetForwardOffset, crouchTransitionSpeed * Time.deltaTime);
            currentVerticalOffset = Mathf.MoveTowards(currentVerticalOffset, targetVerticalOffset, crouchTransitionSpeed * Time.deltaTime);

            // Apply offsets to camera position
            Vector3 currentPos = cameraPivot.localPosition;
            currentPos.y = baseCameraPivotY + currentVerticalOffset;
            currentPos.z = baseCameraPivotZ + currentForwardOffset;
            cameraPivot.localPosition = currentPos;
        }

        public void OnLook(Vector2 lookDelta)
        {
            yaw += lookDelta.x * mouseSensitivity;
            pitch -= lookDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            playerBody.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        }

        // Compatibility API (used by Synty sample logic)

        public Vector3 GetCameraForward()
        {
            return cameraTransform != null ? cameraTransform.forward : cameraPivot.forward;
        }

        public Vector3 GetCameraForwardZeroedYNormalised()
        {
            Vector3 f = GetCameraForward();
            f.y = 0f;

            float mag = f.magnitude;
            if (mag <= 0.0001f)
            {
                Vector3 fallback = playerBody != null ? playerBody.forward : Vector3.forward;
                fallback.y = 0f;
                return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
            }

            return f / mag;
        }

        public Vector3 GetCameraRightZeroedYNormalised()
        {
            Vector3 r = cameraTransform != null ? cameraTransform.right : cameraPivot.right;
            r.y = 0f;

            float mag = r.magnitude;
            if (mag <= 0.0001f)
            {
                return Vector3.right;
            }

            return r / mag;
        }

        public Vector3 GetCameraPosition()
        {
            return cameraTransform != null ? cameraTransform.position : cameraPivot.position;
        }

        public float GetCameraTiltX()
        {
            // Synty code expects 0..360 style Euler pitch
            return cameraPivot != null ? cameraPivot.eulerAngles.x : 0f;
        }

        public void LockOn(bool enable, Transform target)
        {
            // No-op for FPS. Retained for animation controller compatibility.
        }

        private static void ApplyCursorLock(bool shouldLock)
        {
            if (!shouldLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
