using UnityEngine;

namespace Game.Player.DesktopFps
{
    /// <summary>
    /// Handles player rotation, facing direction, and strafing logic.
    /// </summary>
    public class PlayerRotation : MonoBehaviour
    {
        #region Settings

        [Header("Rotation")]
        [SerializeField] private float _rotationSmoothing = 10f;

        [Header("Strafing")]
        [SerializeField] private float _forwardStrafeMinThreshold = -55.0f;
        [SerializeField] private float _forwardStrafeMaxThreshold = 125.0f;

        #endregion

        #region Dependencies

        private CameraController _cameraController;

        #endregion

        #region State

        private bool _isStrafing;
        private bool _isTurningInPlace;
        private float _strafeDirectionX;
        private float _strafeDirectionZ;
        private float _forwardStrafe = 1f;
        private float _cameraRotationOffset;
        private float _strafeAngle;
        private float _shuffleDirectionX;
        private float _shuffleDirectionZ;

        private const float STRAFE_DIRECTION_DAMP_TIME = 20f;
        private const float ANIMATION_DAMP_TIME = 5f;

        #endregion

        #region Properties

        public bool IsStrafing => _isStrafing;
        public bool IsTurningInPlace => _isTurningInPlace;
        public float StrafeDirectionX => _strafeDirectionX;
        public float StrafeDirectionZ => _strafeDirectionZ;
        public float ForwardStrafe => _forwardStrafe;
        public float CameraRotationOffset => _cameraRotationOffset;
        public float ShuffleDirectionX => _shuffleDirectionX;
        public float ShuffleDirectionZ => _shuffleDirectionZ;

        #endregion

        #region Lifecycle

        public void Initialize(CameraController cameraController, bool alwaysStrafe)
        {
            _cameraController = cameraController;
            _isStrafing = alwaysStrafe;
        }

        public void UpdateFacing(Vector3 moveDirection, Vector3 velocity, float speed2D, bool isSprinting)
        {
            Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
            Vector3 directionForward = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;

            Vector3 cameraForward = _cameraController.GetCameraForwardZeroedYNormalised();
            Quaternion strafingTargetRotation = Quaternion.LookRotation(cameraForward);

            _strafeAngle = characterForward != directionForward
                ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up)
                : 0f;

            _isTurningInPlace = false;

            if (_isStrafing)
            {
                HandleStrafingRotation(moveDirection, characterForward, characterRight, directionForward, cameraForward, strafingTargetRotation);
            }
            else
            {
                HandleNormalRotation(velocity);
            }
        }

        #endregion

        #region Strafing Control

        public void SetStrafing(bool strafing)
        {
            _isStrafing = strafing;
        }

        #endregion

        #region Private Methods

        private void HandleStrafingRotation(Vector3 moveDirection, Vector3 characterForward, Vector3 characterRight,
            Vector3 directionForward, Vector3 cameraForward, Quaternion strafingTargetRotation)
        {
            if (moveDirection.magnitude > 0.01)
            {
                if (cameraForward != Vector3.zero)
                {
                    _shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                    _shuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                    UpdateStrafeDirection(
                        Vector3.Dot(characterForward, directionForward),
                        Vector3.Dot(characterRight, directionForward)
                    );

                    _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

                    float targetValue = _strafeAngle > _forwardStrafeMinThreshold && _strafeAngle < _forwardStrafeMaxThreshold ? 1f : 0f;

                    if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                    {
                        _forwardStrafe = targetValue;
                    }
                    else
                    {
                        float t = Mathf.Clamp01(STRAFE_DIRECTION_DAMP_TIME * Time.deltaTime);
                        _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                    }
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _rotationSmoothing * Time.deltaTime);
            }
            else
            {
                UpdateStrafeDirection(1f, 0f);

                float t = 20 * Time.deltaTime;
                float newOffset = 0f;

                if (characterForward != cameraForward)
                {
                    newOffset = Vector3.SignedAngle(characterForward, cameraForward, Vector3.up);
                }

                _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, newOffset, t);

                if (Mathf.Abs(_cameraRotationOffset) > 10)
                {
                    _isTurningInPlace = true;
                }
            }
        }

        private void HandleNormalRotation(Vector3 velocity)
        {
            UpdateStrafeDirection(1f, 0f);
            _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

            _shuffleDirectionZ = 1;
            _shuffleDirectionX = 0;

            Vector3 faceDirection = new Vector3(velocity.x, 0f, velocity.z);

            if (faceDirection == Vector3.zero)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(faceDirection),
                _rotationSmoothing * Time.deltaTime
            );
        }

        private void UpdateStrafeDirection(float targetZ, float targetX)
        {
            _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, targetZ, ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, targetX, ANIMATION_DAMP_TIME * Time.deltaTime);
            _strafeDirectionZ = Mathf.Round(_strafeDirectionZ * 1000f) / 1000f;
            _strafeDirectionX = Mathf.Round(_strafeDirectionX * 1000f) / 1000f;
        }

        #endregion
    }
}
