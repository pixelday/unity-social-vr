using UnityEngine;

namespace Game.Player.DesktopFps
{
    /// <summary>
    /// Handles player velocity, direction, and speed calculations.
    /// Single source of truth for movement state.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        #region Settings

        [Header("Movement Speeds")]
        [SerializeField] private float _walkSpeed = 1.4f;
        [SerializeField] private float _runSpeed = 2.5f;
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _speedChangeDamping = 10f;

        [Header("Gravity")]
        [SerializeField] private float _gravityMultiplier = 2f;

        #endregion

        #region Dependencies

        private CharacterController _controller;
        private CameraController _cameraController;
        private InputSystem.InputReader _inputReader;

        #endregion

        #region State

        private Vector3 _velocity;
        private Vector3 _targetVelocity;
        private Vector3 _moveDirection;
        private float _speed2D;
        private float _currentMaxSpeed;
        private float _targetMaxSpeed;
        private GaitState _currentGait;

        private const float ANIMATION_DAMP_TIME = 5f;

        #endregion

        #region Gait Enum

        public enum GaitState
        {
            Idle,
            Walk,
            Run,
            Sprint
        }

        #endregion

        #region Properties

        public Vector3 Velocity => _velocity;
        public Vector3 MoveDirection => _moveDirection;
        public float Speed2D => _speed2D;
        public GaitState CurrentGait => _currentGait;
        public float NewDirectionDifferenceAngle { get; private set; }

        #endregion

        #region Lifecycle

        public void Initialize(CharacterController controller, CameraController cameraController, InputSystem.InputReader inputReader)
        {
            _controller = controller;
            _cameraController = cameraController;
            _inputReader = inputReader;
        }

        public void Tick(bool isGrounded, bool isCrouching, bool isSprinting, bool isWalking)
        {
            CalculateMoveDirection(isGrounded, isCrouching, isSprinting, isWalking);
        }

        public void ApplyMovement()
        {
            _controller.Move(_velocity * Time.deltaTime);
        }

        public void ApplyGravity()
        {
            if (_velocity.y > Physics.gravity.y)
            {
                _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
            }
        }

        #endregion

        #region Movement Calculation

        private void CalculateMoveDirection(bool isGrounded, bool isCrouching, bool isSprinting, bool isWalking)
        {
            // Calculate input direction
            _moveDirection = (_cameraController.GetCameraForwardZeroedYNormalised() * _inputReader._moveComposite.y)
                + (_cameraController.GetCameraRightZeroedYNormalised() * _inputReader._moveComposite.x);

            // Determine target speed based on state
            if (!isGrounded)
            {
                _targetMaxSpeed = _currentMaxSpeed;
            }
            else if (isCrouching)
            {
                _targetMaxSpeed = _walkSpeed;
            }
            else if (isSprinting)
            {
                _targetMaxSpeed = _sprintSpeed;
            }
            else if (isWalking)
            {
                _targetMaxSpeed = _walkSpeed;
            }
            else
            {
                _targetMaxSpeed = _runSpeed;
            }

            _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, ANIMATION_DAMP_TIME * Time.deltaTime);

            _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
            _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;

            _velocity.z = Mathf.Lerp(_velocity.z, _targetVelocity.z, _speedChangeDamping * Time.deltaTime);
            _velocity.x = Mathf.Lerp(_velocity.x, _targetVelocity.x, _speedChangeDamping * Time.deltaTime);

            _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

            Vector3 playerForwardVector = transform.forward;

            NewDirectionDifferenceAngle = playerForwardVector != _moveDirection
                ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
                : 0f;

            CalculateGait();
        }

        private void CalculateGait()
        {
            float runThreshold = (_walkSpeed + _runSpeed) / 2;
            float sprintThreshold = (_runSpeed + _sprintSpeed) / 2;

            if (_speed2D < 0.01)
            {
                _currentGait = GaitState.Idle;
            }
            else if (_speed2D < runThreshold)
            {
                _currentGait = GaitState.Walk;
            }
            else if (_speed2D < sprintThreshold)
            {
                _currentGait = GaitState.Run;
            }
            else
            {
                _currentGait = GaitState.Sprint;
            }
        }

        #endregion

        #region Velocity Control

        public void SetVelocityY(float yVelocity)
        {
            _velocity.y = yVelocity;
        }

        public void ResetYVelocity()
        {
            _velocity.y = 0f;
        }

        #endregion
    }
}
