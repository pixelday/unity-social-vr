using UnityEngine;

namespace Game.Player.DesktopFps
{
    /// <summary>
    /// Manages animation state machine and animator parameter updates.
    /// Single responsibility: coordinate animator state based on player state.
    /// </summary>
    public class PlayerAnimationState : MonoBehaviour
    {
        #region Animation State Enum

        public enum AnimationState
        {
            Base,
            Locomotion,
            Jump,
            Fall,
            Crouch
        }

        #endregion

        #region Animation Variable Hashes

        private readonly int _movementInputTappedHash = Animator.StringToHash("MovementInputTapped");
        private readonly int _movementInputPressedHash = Animator.StringToHash("MovementInputPressed");
        private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
        private readonly int _shuffleDirectionXHash = Animator.StringToHash("ShuffleDirectionX");
        private readonly int _shuffleDirectionZHash = Animator.StringToHash("ShuffleDirectionZ");
        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
        private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
        private readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");
        private readonly int _inclineAngleHash = Animator.StringToHash("InclineAngle");
        private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
        private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
        private readonly int _forwardStrafeHash = Animator.StringToHash("ForwardStrafe");
        private readonly int _cameraRotationOffsetHash = Animator.StringToHash("CameraRotationOffset");
        private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
        private readonly int _isTurningInPlaceHash = Animator.StringToHash("IsTurningInPlace");
        private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");
        private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
        private readonly int _isStartingHash = Animator.StringToHash("IsStarting");
        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly int _leanValueHash = Animator.StringToHash("LeanValue");
        private readonly int _headLookXHash = Animator.StringToHash("HeadLookX");
        private readonly int _headLookYHash = Animator.StringToHash("HeadLookY");
        private readonly int _bodyLookXHash = Animator.StringToHash("BodyLookX");
        private readonly int _bodyLookYHash = Animator.StringToHash("BodyLookY");
        private readonly int _locomotionStartDirectionHash = Animator.StringToHash("LocomotionStartDirection");

        #endregion

        #region Settings

        [Header("Head Look")]
        [SerializeField] private AnimationCurve _headLookXCurve;

        [Header("Body Look")]
        [SerializeField] private AnimationCurve _bodyLookXCurve;

        [Header("Lean")]
        [SerializeField] private AnimationCurve _leanCurve;

        [Header("Grounded")]
        [SerializeField] private float _inclineAngle;

        #endregion

        #region Dependencies

        private Animator _animator;
        private CameraController _cameraController;

        #endregion

        #region State

        private AnimationState _currentState = AnimationState.Base;
        private bool _isStarting;
        private bool _isStopped = true;
        private float _locomotionStartDirection;
        private float _locomotionStartTimer;

        // Head/Body look and lean
        private bool _enableHeadTurn = true;
        private bool _enableBodyTurn = true;
        private bool _enableLean = true;
        private float _headLookDelay;
        private float _bodyLookDelay;
        private float _leanDelay;
        private float _headLookX;
        private float _headLookY;
        private float _bodyLookX;
        private float _bodyLookY;
        private float _leanValue;
        private float _initialLeanValue;
        private float _initialTurnValue;
        private float _rotationRate;
        private Vector3 _currentRotation = Vector3.zero;
        private Vector3 _previousRotation;

        #endregion

        #region Properties

        public AnimationState CurrentState => _currentState;
        public bool IsStarting => _isStarting;
        public bool IsStopped => _isStopped;

        #endregion

        #region Lifecycle

        public void Initialize(Animator animator, CameraController cameraController)
        {
            _animator = animator;
            _cameraController = cameraController;
            _previousRotation = transform.forward;
        }

        public void UpdateAnimator(
            PlayerMovement.GaitState gait,
            float speed2D,
            float sprintSpeed,
            bool isStrafing,
            bool isTurningInPlace,
            float strafeDirectionX,
            float strafeDirectionZ,
            float forwardStrafe,
            float cameraRotationOffset,
            float shuffleDirectionX,
            float shuffleDirectionZ,
            bool movementInputHeld,
            bool movementInputPressed,
            bool movementInputTapped,
            bool isCrouching,
            bool isGrounded,
            bool isWalking,
            float fallingDuration)
        {
            // Update enables
            CheckEnableTurns(isTurningInPlace);
            CheckEnableLean(isTurningInPlace);

            // Calculate rotational additives (lean, head look, body look)
            CalculateRotationalAdditives(_enableLean, _enableHeadTurn, _enableBodyTurn, speed2D, sprintSpeed);

            // Set all animator parameters
            _animator.SetFloat(_leanValueHash, _leanValue);
            _animator.SetFloat(_headLookXHash, _headLookX);
            _animator.SetFloat(_headLookYHash, _headLookY);
            _animator.SetFloat(_bodyLookXHash, _bodyLookX);
            _animator.SetFloat(_bodyLookYHash, _bodyLookY);

            _animator.SetFloat(_isStrafingHash, isStrafing ? 1.0f : 0.0f);
            _animator.SetFloat(_inclineAngleHash, _inclineAngle);

            _animator.SetFloat(_moveSpeedHash, speed2D);
            _animator.SetInteger(_currentGaitHash, (int)gait);

            _animator.SetFloat(_strafeDirectionXHash, strafeDirectionX);
            _animator.SetFloat(_strafeDirectionZHash, strafeDirectionZ);
            _animator.SetFloat(_forwardStrafeHash, forwardStrafe);
            _animator.SetFloat(_cameraRotationOffsetHash, cameraRotationOffset);

            _animator.SetBool(_movementInputHeldHash, movementInputHeld);
            _animator.SetBool(_movementInputPressedHash, movementInputPressed);
            _animator.SetBool(_movementInputTappedHash, movementInputTapped);
            _animator.SetFloat(_shuffleDirectionXHash, shuffleDirectionX);
            _animator.SetFloat(_shuffleDirectionZHash, shuffleDirectionZ);

            _animator.SetBool(_isTurningInPlaceHash, isTurningInPlace);
            _animator.SetBool(_isCrouchingHash, isCrouching);

            _animator.SetFloat(_fallingDurationHash, fallingDuration);
            _animator.SetBool(_isGroundedHash, isGrounded);

            _animator.SetBool(_isWalkingHash, isWalking);
            _animator.SetBool(_isStoppedHash, _isStopped);
            _animator.SetBool(_isStartingHash, _isStarting);

            _animator.SetFloat(_locomotionStartDirectionHash, _locomotionStartDirection);
        }

        public void UpdateGroundedIncline(bool isGrounded)
        {
            // For FPS we just keep the animator param alive with 0
            if (isGrounded)
            {
                _inclineAngle = Mathf.Lerp(_inclineAngle, 0f, 20f * Time.deltaTime);
            }
            else
            {
                _inclineAngle = Mathf.Lerp(_inclineAngle, 0f, 20f * Time.deltaTime);
            }
        }

        public void CheckStopped(Vector3 moveDirection, float speed2D)
        {
            _isStopped = moveDirection.magnitude == 0 && speed2D < 0.5f;
        }

        public void CheckStarting(Vector3 moveDirection, float speed2D, float newDirectionDifferenceAngle, bool isStrafing)
        {
            _locomotionStartTimer = VariableOverrideDelayTimer(_locomotionStartTimer);

            bool isStartingCheck = false;

            if (_locomotionStartTimer <= 0.0f)
            {
                if (moveDirection.magnitude > 0.01 && speed2D < 1 && !isStrafing)
                {
                    isStartingCheck = true;
                }

                if (isStartingCheck)
                {
                    if (!_isStarting)
                    {
                        _locomotionStartDirection = newDirectionDifferenceAngle;
                        _animator.SetFloat(_locomotionStartDirectionHash, _locomotionStartDirection);
                    }

                    float delayTime = 0.2f;
                    _leanDelay = delayTime;
                    _headLookDelay = delayTime;
                    _bodyLookDelay = delayTime;
                    _locomotionStartTimer = delayTime;
                }
            }
            else
            {
                isStartingCheck = true;
            }

            _isStarting = isStartingCheck;
        }

        public void SetJumping(bool jumping)
        {
            _animator.SetBool(_isJumpingAnimHash, jumping);
        }

        public void SetState(AnimationState newState)
        {
            _currentState = newState;
        }

        #endregion

        #region Private Methods

        private void CheckEnableTurns(bool isTurningInPlace)
        {
            _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
            _enableHeadTurn = _headLookDelay == 0.0f && !_isStarting;

            _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
            _enableBodyTurn = _bodyLookDelay == 0.0f && !(_isStarting || isTurningInPlace);
        }

        private void CheckEnableLean(bool isTurningInPlace)
        {
            _leanDelay = VariableOverrideDelayTimer(_leanDelay);
            _enableLean = _leanDelay == 0.0f && !(_isStarting || isTurningInPlace);
        }

        private void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated,
            float speed2D, float sprintSpeed)
        {
            if (headLookActivated || leansActivated || bodyLookActivated)
            {
                _currentRotation = transform.forward;

                _rotationRate = _currentRotation != _previousRotation
                    ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                    : 0f;
            }

            _initialLeanValue = leansActivated ? _rotationRate : 0f;

            float leanSmoothness = 5;
            float maxLeanRotationRate = 275.0f;

            float referenceValue = speed2D / sprintSpeed;
            _leanValue = CalculateSmoothedValue(
                _leanValue,
                _initialLeanValue,
                maxLeanRotationRate,
                leanSmoothness,
                _leanCurve,
                referenceValue,
                true
            );

            float headTurnSmoothness = 5f;
            float cameraRotationOffset = _cameraController != null ?
                (_animator.GetFloat(_cameraRotationOffsetHash)) : 0f;

            if (headLookActivated && _animator.GetBool(_isTurningInPlaceHash))
            {
                _initialTurnValue = cameraRotationOffset;
                _headLookX = Mathf.Lerp(_headLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
            }
            else
            {
                _initialTurnValue = headLookActivated ? _rotationRate : 0f;
                _headLookX = CalculateSmoothedValue(
                    _headLookX,
                    _initialTurnValue,
                    maxLeanRotationRate,
                    headTurnSmoothness,
                    _headLookXCurve,
                    _headLookX,
                    false
                );
            }

            float bodyTurnSmoothness = 5f;

            _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;

            _bodyLookX = CalculateSmoothedValue(
                _bodyLookX,
                _initialTurnValue,
                maxLeanRotationRate,
                bodyTurnSmoothness,
                _bodyLookXCurve,
                _bodyLookX,
                false
            );

            float cameraTilt = _cameraController != null ? _cameraController.GetCameraTiltX() : 0f;
            cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
            cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);

            _headLookY = cameraTilt;
            _bodyLookY = cameraTilt;

            _previousRotation = _currentRotation;
        }

        private float CalculateSmoothedValue(
            float mainVariable,
            float newValue,
            float maxRateChange,
            float smoothness,
            AnimationCurve referenceCurve,
            float referenceValue,
            bool isMultiplier
        )
        {
            float changeVariable = newValue / maxRateChange;
            changeVariable = Mathf.Clamp(changeVariable, -1.0f, 1.0f);

            if (isMultiplier)
            {
                float multiplier = referenceCurve.Evaluate(referenceValue);
                changeVariable *= multiplier;
            }
            else
            {
                changeVariable = referenceCurve.Evaluate(changeVariable);
            }

            if (!changeVariable.Equals(mainVariable))
            {
                changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);
            }

            return changeVariable;
        }

        private float VariableOverrideDelayTimer(float timeVariable)
        {
            if (timeVariable > 0.0f)
            {
                timeVariable -= Time.deltaTime;
                timeVariable = Mathf.Clamp(timeVariable, 0.0f, 1.0f);
            }
            else
            {
                timeVariable = 0.0f;
            }

            return timeVariable;
        }

        #endregion
    }
}
