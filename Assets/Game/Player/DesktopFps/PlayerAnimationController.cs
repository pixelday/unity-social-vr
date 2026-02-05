using UnityEngine;

namespace Game.Player.DesktopFps
{
    /// <summary>
    /// Coordinator for player animation and movement systems.
    /// Follows Constitution Principle VI: Explicit lifecycle instead of scattered Unity callbacks.
    /// Follows Constitution Principle XI: Narrow contracts between systems.
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        #region External Components

        [Header("External Components")]
        [Tooltip("FPS camera controller")]
        [SerializeField] private CameraController _cameraController;
        [Tooltip("InputReader handles player input")]
        [SerializeField] private InputSystem.InputReader _inputReader;
        [Tooltip("Animator component for controlling player animations")]
        [SerializeField] private Animator _animator;
        [Tooltip("Character Controller component for controlling player movement")]
        [SerializeField] private CharacterController _controller;
        [Tooltip("Capsule controller for managing height/center during crouch")]
        [SerializeField] private PlayerCapsuleController _capsuleController;

        #endregion

        #region System Components

        [Header("System Components")]
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerRotation _rotation;
        [SerializeField] private PlayerAnimationState _animationState;

        #endregion

        #region Shuffle Settings

        [Header("Shuffles")]
        [SerializeField] private float _buttonHoldThreshold = 0.15f;

        #endregion

        #region Grounded Settings

        [Header("Grounded")]
        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private float _groundedOffset = -0.14f;

        #endregion

        #region Jump Settings

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 10f;

        #endregion

        #region Player State

        private bool _crouchKeyPressed;
        private bool _isCrouching;
        private bool _isGrounded = true;
        private bool _isSliding;
        private bool _isSprinting;
        private bool _isWalking;
        private bool _movementInputHeld;
        private bool _movementInputPressed;
        private bool _movementInputTapped;
        private float _fallStartTime;
        private float _fallingDuration;

        #endregion

        #region Unity Lifecycle (Entry Point Only)

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            Tick();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        #endregion

        #region Explicit Lifecycle

        public void Initialize()
        {
            // Validate dependencies
            if (_capsuleController == null)
            {
                Debug.LogError($"{nameof(PlayerAnimationController)}: _capsuleController is not assigned. Crouch will not work properly.", this);
            }

            if (_movement == null || _rotation == null || _animationState == null)
            {
                Debug.LogError($"{nameof(PlayerAnimationController)}: System components not assigned. Add PlayerMovement, PlayerRotation, and PlayerAnimationState.", this);
                enabled = false;
                return;
            }

            // Initialize subsystems
            _movement.Initialize(_controller, _cameraController, _inputReader);
            _rotation.Initialize(_cameraController, alwaysStrafe: true);
            _animationState.Initialize(_animator, _cameraController);

            // Subscribe to input events
            _inputReader.onWalkToggled += ToggleWalk;
            _inputReader.onSprintActivated += ActivateSprint;
            _inputReader.onSprintDeactivated += DeactivateSprint;
            _inputReader.onCrouchActivated += ActivateCrouch;
            _inputReader.onCrouchDeactivated += DeactivateCrouch;

            // Enter initial state
            SwitchState(PlayerAnimationState.AnimationState.Locomotion);
        }

        public void Tick()
        {
            switch (_animationState.CurrentState)
            {
                case PlayerAnimationState.AnimationState.Locomotion:
                    TickLocomotion();
                    break;
                case PlayerAnimationState.AnimationState.Jump:
                    TickJump();
                    break;
                case PlayerAnimationState.AnimationState.Fall:
                    TickFall();
                    break;
                case PlayerAnimationState.AnimationState.Crouch:
                    TickCrouch();
                    break;
            }
        }

        public void Shutdown()
        {
            // Unsubscribe from input events
            if (_inputReader != null)
            {
                _inputReader.onWalkToggled -= ToggleWalk;
                _inputReader.onSprintActivated -= ActivateSprint;
                _inputReader.onSprintDeactivated -= DeactivateSprint;
                _inputReader.onCrouchActivated -= ActivateCrouch;
                _inputReader.onCrouchDeactivated -= DeactivateCrouch;
                _inputReader.onJumpPerformed -= OnJumpPerformed;
            }
        }

        #endregion

        #region State Machine

        private void SwitchState(PlayerAnimationState.AnimationState newState)
        {
            ExitCurrentState();
            EnterState(newState);
        }

        private void EnterState(PlayerAnimationState.AnimationState stateToEnter)
        {
            _animationState.SetState(stateToEnter);

            switch (stateToEnter)
            {
                case PlayerAnimationState.AnimationState.Locomotion:
                    _inputReader.onJumpPerformed += OnJumpPerformed;
                    break;
                case PlayerAnimationState.AnimationState.Jump:
                    _animationState.SetJumping(true);
                    _isSliding = false;
                    _movement.SetVelocityY(_jumpForce);
                    break;
                case PlayerAnimationState.AnimationState.Fall:
                    ResetFallingDuration();
                    _movement.ResetYVelocity();
                    DeactivateCrouch();
                    _isSliding = false;
                    break;
                case PlayerAnimationState.AnimationState.Crouch:
                    _inputReader.onJumpPerformed += OnCrouchJump;
                    break;
            }
        }

        private void ExitCurrentState()
        {
            switch (_animationState.CurrentState)
            {
                case PlayerAnimationState.AnimationState.Locomotion:
                    _inputReader.onJumpPerformed -= OnJumpPerformed;
                    break;
                case PlayerAnimationState.AnimationState.Jump:
                    _animationState.SetJumping(false);
                    break;
                case PlayerAnimationState.AnimationState.Crouch:
                    _inputReader.onJumpPerformed -= OnCrouchJump;
                    break;
            }
        }

        #endregion

        #region State Update Methods

        private void TickLocomotion()
        {
            GroundedCheck();

            if (!_isGrounded)
            {
                SwitchState(PlayerAnimationState.AnimationState.Fall);
                return;
            }

            if (_isCrouching)
            {
                SwitchState(PlayerAnimationState.AnimationState.Crouch);
                return;
            }

            CalculateInput();
            _movement.Tick(_isGrounded, _isCrouching, _isSprinting, _isWalking);
            _animationState.CheckStarting(_movement.MoveDirection, _movement.Speed2D, _movement.NewDirectionDifferenceAngle, _rotation.IsStrafing);
            _animationState.CheckStopped(_movement.MoveDirection, _movement.Speed2D);
            _rotation.UpdateFacing(_movement.MoveDirection, _movement.Velocity, _movement.Speed2D, _isSprinting);
            _movement.ApplyMovement();
            _animationState.UpdateGroundedIncline(_isGrounded);
            UpdateAnimator();
        }

        private void TickJump()
        {
            _movement.ApplyGravity();

            if (_movement.Velocity.y <= 0f)
            {
                _animationState.SetJumping(false);
                SwitchState(PlayerAnimationState.AnimationState.Fall);
                return;
            }

            GroundedCheck();
            CalculateInput();
            _movement.Tick(_isGrounded, _isCrouching, _isSprinting, _isWalking);
            _rotation.UpdateFacing(_movement.MoveDirection, _movement.Velocity, _movement.Speed2D, _isSprinting);
            _movement.ApplyMovement();
            UpdateAnimator();
        }

        private void TickFall()
        {
            GroundedCheck();

            CalculateInput();
            _movement.Tick(_isGrounded, _isCrouching, _isSprinting, _isWalking);
            _rotation.UpdateFacing(_movement.MoveDirection, _movement.Velocity, _movement.Speed2D, _isSprinting);

            _movement.ApplyGravity();
            _movement.ApplyMovement();
            UpdateAnimator();

            if (_controller.isGrounded)
            {
                SwitchState(PlayerAnimationState.AnimationState.Locomotion);
                return;
            }

            UpdateFallingDuration();
        }

        private void TickCrouch()
        {
            GroundedCheck();

            if (!_isGrounded)
            {
                DeactivateCrouch();
                if (_capsuleController != null)
                {
                    _capsuleController.SetCrouching(false);
                }
                SwitchState(PlayerAnimationState.AnimationState.Fall);
                return;
            }

            if (_capsuleController != null && !_crouchKeyPressed && _capsuleController.CanStandUp())
            {
                DeactivateCrouch();
                SwitchState(PlayerAnimationState.AnimationState.Locomotion);
                return;
            }

            if (!_isCrouching && _capsuleController != null)
            {
                _capsuleController.SetCrouching(false);
                SwitchState(PlayerAnimationState.AnimationState.Locomotion);
                return;
            }

            CalculateInput();
            _movement.Tick(_isGrounded, _isCrouching, _isSprinting, _isWalking);
            _animationState.CheckStarting(_movement.MoveDirection, _movement.Speed2D, _movement.NewDirectionDifferenceAngle, _rotation.IsStrafing);
            _animationState.CheckStopped(_movement.MoveDirection, _movement.Speed2D);
            _rotation.UpdateFacing(_movement.MoveDirection, _movement.Velocity, _movement.Speed2D, _isSprinting);
            _movement.ApplyMovement();
            UpdateAnimator();
        }

        #endregion

        #region Input Processing

        private void CalculateInput()
        {
            if (_inputReader._movementInputDetected)
            {
                if (_inputReader._movementInputDuration == 0)
                {
                    _movementInputTapped = true;
                }
                else if (_inputReader._movementInputDuration > 0 && _inputReader._movementInputDuration < _buttonHoldThreshold)
                {
                    _movementInputTapped = false;
                    _movementInputPressed = true;
                    _movementInputHeld = false;
                }
                else
                {
                    _movementInputTapped = false;
                    _movementInputPressed = false;
                    _movementInputHeld = true;
                }

                _inputReader._movementInputDuration += Time.deltaTime;
            }
            else
            {
                _inputReader._movementInputDuration = 0;
                _movementInputTapped = false;
                _movementInputPressed = false;
                _movementInputHeld = false;
            }
        }

        #endregion

        #region Animator Update

        private void UpdateAnimator()
        {
            _animationState.UpdateAnimator(
                _movement.CurrentGait,
                _movement.Speed2D,
                7f, // Sprint speed for lean calculation
                _rotation.IsStrafing,
                _rotation.IsTurningInPlace,
                _rotation.StrafeDirectionX,
                _rotation.StrafeDirectionZ,
                _rotation.ForwardStrafe,
                _rotation.CameraRotationOffset,
                _rotation.ShuffleDirectionX,
                _rotation.ShuffleDirectionZ,
                _movementInputHeld,
                _movementInputPressed,
                _movementInputTapped,
                _isCrouching,
                _isGrounded,
                _isWalking,
                _fallingDuration
            );
        }

        #endregion

        #region Input Event Handlers

        private void ToggleWalk()
        {
            _isWalking = !_isWalking && _isGrounded && !_isSprinting;
        }

        private void ActivateSprint()
        {
            if (!_isCrouching)
            {
                _isWalking = false;
                _isSprinting = true;
                _rotation.SetStrafing(false);
            }
        }

        private void DeactivateSprint()
        {
            _isSprinting = false;
            _rotation.SetStrafing(true); // Always strafe when not sprinting
        }

        private void ActivateCrouch()
        {
            _crouchKeyPressed = true;

            if (_isGrounded && _capsuleController != null)
            {
                _capsuleController.SetCrouching(true);
                DeactivateSprint();
                _isCrouching = true;
            }
        }

        private void DeactivateCrouch()
        {
            _crouchKeyPressed = false;

            if (_capsuleController != null && _capsuleController.CanStandUp() && !_isSliding)
            {
                _capsuleController.SetCrouching(false);
                _isCrouching = false;
            }
        }

        private void OnJumpPerformed()
        {
            SwitchState(PlayerAnimationState.AnimationState.Jump);
        }

        private void OnCrouchJump()
        {
            if (_capsuleController != null && _capsuleController.CanStandUp())
            {
                DeactivateCrouch();
                SwitchState(PlayerAnimationState.AnimationState.Jump);
            }
        }

        #endregion

        #region Ground Checks

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                _controller.transform.position.x,
                _controller.transform.position.y - _groundedOffset,
                _controller.transform.position.z
            );

            _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);
        }

        #endregion

        #region Falling Duration

        private void ResetFallingDuration()
        {
            _fallStartTime = Time.time;
            _fallingDuration = 0f;
        }

        private void UpdateFallingDuration()
        {
            _fallingDuration = Time.time - _fallStartTime;
        }

        #endregion

        #region Public API (for sliding - legacy)

        public void ActivateSliding()
        {
            _isSliding = true;
        }

        public void DeactivateSliding()
        {
            _isSliding = false;
        }

        #endregion
    }
}
