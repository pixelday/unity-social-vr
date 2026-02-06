using System;
using UnityEngine;

namespace Game.Player.DesktopFps
{
    /// <summary>
    /// Manages CharacterController capsule dimensions for crouching.
    /// Single source of truth for player height and center position.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCapsuleController : MonoBehaviour
    {
        [Header("Capsule Dimensions")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float standingCenter = 0.93f;
        [SerializeField] private float crouchingHeight = 1.2f;
        [SerializeField] private float crouchingCenter = 0.6f;

        [Header("Ceiling Detection")]
        [SerializeField] private LayerMask groundLayerMask;

        private CharacterController characterController;
        private bool isCrouching;

        public event Action<float> OnHeightChanged;

        public float CurrentHeight => characterController.height;
        public float CurrentCenter => characterController.center.y;
        public float StandingHeight => standingHeight;
        public float CrouchingHeight => crouchingHeight;
        public bool IsCrouching => isCrouching;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (characterController == null)
            {
                Debug.LogError($"{nameof(PlayerCapsuleController)}: CharacterController component not found.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Sets the crouching state and adjusts capsule dimensions accordingly.
        /// </summary>
        public void SetCrouching(bool crouch)
        {
            if (characterController == null)
            {
                Debug.LogError($"{nameof(PlayerCapsuleController)}.SetCrouching: CharacterController is null!", this);
                return;
            }

            if (crouch && !isCrouching)
            {
                ApplyCrouchDimensions();
                isCrouching = true;
            }
            else if (!crouch && isCrouching)
            {
               ApplyStandingDimensions();
                isCrouching = false;
            }
        }

        /// <summary>
        /// Checks if there's enough ceiling clearance to stand up.
        /// </summary>
        public bool CanStandUp()
        {
            if (!isCrouching)
            {
                return true;
            }

            float radius = Mathf.Max(0.01f, characterController.radius * 0.95f);
            Vector3 centerWorld = transform.position + characterController.center;

            float currentHeight = characterController.height;
            float targetHeight = standingHeight;

            if (targetHeight <= currentHeight + 0.001f)
            {
                return true;
            }

            float bottomY = centerWorld.y - (currentHeight * 0.5f) + radius;
            float topY = bottomY + (targetHeight - 2.0f * radius);
            float extra = (targetHeight - currentHeight) + 0.05f;

            Vector3 p1 = new Vector3(centerWorld.x, bottomY, centerWorld.z);
            Vector3 p2 = new Vector3(centerWorld.x, topY + extra, centerWorld.z);

            return !Physics.CheckCapsule(p1, p2, radius, groundLayerMask, QueryTriggerInteraction.Ignore);
        }

        private void ApplyCrouchDimensions()
        {
            float previousHeight = characterController.height;

            characterController.center = new Vector3(0f, crouchingCenter, 0f);
            characterController.height = crouchingHeight;

            OnHeightChanged?.Invoke(crouchingHeight - previousHeight);
        }

        private void ApplyStandingDimensions()
        {
            float previousHeight = characterController.height;

            characterController.center = new Vector3(0f, standingCenter, 0f);
            characterController.height = standingHeight;

            OnHeightChanged?.Invoke(standingHeight - previousHeight);
        }
    }
}
