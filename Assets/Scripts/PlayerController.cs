using UnityEngine;

namespace Youregone.FinalCharacterController
{
    public class PlayerController : MonoBehaviour
    {
        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;

        [Header("Movement Settings")]
        [SerializeField] private float _walkAcceleration = 50f;
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _runAcceleration = 50f;
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _drag = 30f;
        [SerializeField] private float _movementThreshhold = .01f;
        [SerializeField] private float _sprintAcceleration = 50f;
        [SerializeField] private float _sprintSpeed = 9f;
        [SerializeField] private float _inAirAcceleration = 40;

        [Header("Jump Settings")]
        [SerializeField] private float _gravity = 25f;
        [SerializeField] private float _jumpSpeed = 1f;

        [Header("Camera Settings")]
        [SerializeField] private float _lookSensitivityHorizontal = .5f;
        [SerializeField] private float _lookSensitivityVertical = .5f;
        [SerializeField] private float _LookLimitVertical = 89f;

        [Header("Animation")]
        [SerializeField] private float _playerModelRotationSpeed = 10f;
        [SerializeField] private float _rotateToTargetTime = .25f;

        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private Transform _groundedCheckTransform;

        private PlayerInput _playerInput;
        private PlayerState _playerState;
        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;
        private float _rotatingToTargetTimer = 0f;
        private float _verticalVelocity = 0f;
        private float _antiBump;
        private float _stepOffset;
        private bool _isRotatingClockwise = false;
        private bool _jumpedLastFrame = false;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _characterController = GetComponent<CharacterController>();
            _playerState = GetComponent<PlayerState>();

            _antiBump = _sprintSpeed;
            _stepOffset = _characterController.stepOffset;
        }

        private void Update()
        {
            UpdateMovementState();
            HandleVerticalMovement();
            HandleMovement();
        }

        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            _cameraRotation.x += _lookSensitivityHorizontal * _playerInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(
                _cameraRotation.y - _lookSensitivityVertical * _playerInput.LookInput.y,
                -_LookLimitVertical,
                _LookLimitVertical);

            _playerTargetRotation.x += transform.eulerAngles.x + _lookSensitivityHorizontal * _playerInput.LookInput.x;

            float rotationTolerence = 90f;
            bool isIdling = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Idling;
            IsRotatingToTarget = _rotatingToTargetTimer > 0f;

            if(!isIdling)
            {
                RotatePlayerToTarget();
            }
            else if(Mathf.Abs(RotationMismatch) > rotationTolerence || IsRotatingToTarget)
            {
                UpdateIdleRotation(rotationTolerence);
            }

            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            Vector3 cameraForwardProjectionXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, cameraForwardProjectionXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, cameraForwardProjectionXZ);
        }

        private void UpdateIdleRotation(float rotationTolerence)
        {
            if (Mathf.Abs(RotationMismatch) > rotationTolerence)
            {
                _rotatingToTargetTimer = _rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationTolerence;
            }

            _rotatingToTargetTimer -= Time.deltaTime;

            if (_isRotatingClockwise && RotationMismatch > 0f ||
                !_isRotatingClockwise && RotationMismatch < 0f)
            {

                RotatePlayerToTarget();
            }
        }

        private void RotatePlayerToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, _playerModelRotationSpeed * Time.deltaTime);
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = _playerState.IsInGroundedState();

            _verticalVelocity -= _gravity * Time.deltaTime;

            if (isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -_antiBump;

            if(isGrounded && _playerInput.JumpPressed)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpSpeed * 3f * _gravity);
                _jumpedLastFrame = true;
            }
        }

        private void UpdateMovementState()
        {
            bool canRun = CanRun();
            bool isMovementInput = _playerInput.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = _playerInput.SprintToggledOn && isMovingLaterally;
            bool isGrounded = IsGrounded();
            bool isWalking = (isMovingLaterally && !canRun) || _playerInput.WalkToggledOn;

            EPlayerMovementState lateralState = isWalking ? EPlayerMovementState.Walking :
                                                isSprinting ? EPlayerMovementState.Sprinting :
                                                isMovingLaterally || isMovementInput ? EPlayerMovementState.Running : EPlayerMovementState.Idling;

            _playerState.SetPlayerMovementState(lateralState);

            if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y >= 0f)
            {
                _playerState.SetPlayerMovementState(EPlayerMovementState.Jumping);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y < 0f)
            {
                _playerState.SetPlayerMovementState(EPlayerMovementState.Falling);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else
                _characterController.stepOffset = _stepOffset;
        }

        private bool CanRun()
        {
            return _playerInput.MovementInput.y >= Mathf.Abs(_playerInput.MovementInput.x);
        }

        private bool IsGrounded()
        {
            bool grounded = _playerState.IsInGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            bool grounded = Physics.CheckSphere(_groundedCheckTransform.position,
                                                _characterController.radius,
                                                _groundLayerMask,
                                                QueryTriggerInteraction.Ignore);

            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            return _characterController.isGrounded;
        }

        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

            return lateralVelocity.magnitude > _movementThreshhold;
        }

        private void HandleMovement()
        {
            bool isSprinting = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Sprinting;
            bool isGrounded = _playerState.IsInGroundedState();
            bool isWalking = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Walking;

            float lateralAcceleration = !isGrounded ? _inAirAcceleration :
                                        isWalking ? _walkAcceleration :
                                        isSprinting ? _sprintAcceleration : _runAcceleration;

            float clampLateralMagnitude = !isGrounded ? _sprintSpeed :
                                          isWalking ? _walkSpeed :
                                          isSprinting ? _sprintSpeed : _runSpeed;

            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * _playerInput.MovementInput.x + cameraForwardXZ * _playerInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity + movementDelta;

            Vector3 currentDrag = newVelocity.normalized * _drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > _drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
            newVelocity.y += _verticalVelocity;

            _characterController.Move(newVelocity * Time.deltaTime);
        }
    }
}
