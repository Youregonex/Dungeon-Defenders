using UnityEngine;

namespace Youregone.FinalCharacterController
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;

        [Header("Movement Settings")]
        [SerializeField] private float _runAcceleration = 35f;
        [SerializeField] private float _runSpeed = 4f;
        [SerializeField] private float _drag = 15f;
        [SerializeField] private float _movementThreshhold = .01f;
        [SerializeField] private float _sprintAcceleration = 50f;
        [SerializeField] private float _sprintSpeed = 7f;

        [Header("Camera Settings")]
        [SerializeField] private float _lookSensitivityHorizontal = .5f;
        [SerializeField] private float _lookSensitivityVertical = .5f;
        [SerializeField] private float _LookLimitVertical = 89f;

        private PlayerInput _playerInput;
        private PlayerState _playerState;
        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _characterController = GetComponent<CharacterController>();
            _playerState = GetComponent<PlayerState>();
        }

        private void Update()
        {
            UpdateMovementState();
            HandleMovement();
        }

        private void LateUpdate()
        {
            _cameraRotation.x += _lookSensitivityHorizontal * _playerInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(
                _cameraRotation.y - _lookSensitivityVertical * _playerInput.LookInput.y,
                -_LookLimitVertical,
                _LookLimitVertical);

            _playerTargetRotation.x += transform.eulerAngles.x + _lookSensitivityHorizontal * _playerInput.LookInput.x;
            transform.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);

            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
        }

        private void UpdateMovementState()
        {
            bool isMovementInput = _playerInput.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = _playerInput.SprintToggledOn && isMovingLaterally;

            EPlayerMovementState lateralState = isSprinting ? EPlayerMovementState.Sprinting :
                                                isMovingLaterally || isMovementInput ? EPlayerMovementState.Running : EPlayerMovementState.Idling;

            _playerState.SetPlayerMovementState(lateralState);
        }

        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

            return lateralVelocity.magnitude > _movementThreshhold;
        }

        private void HandleMovement()
        {
            bool isSprinting = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Sprinting;

            float lateralAcceleration = isSprinting ? _sprintAcceleration : _runAcceleration;
            float clampLateralMagnitude = isSprinting ? _sprintSpeed : _runSpeed;

            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * _playerInput.MovementInput.x + cameraForwardXZ * _playerInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity + movementDelta;

            Vector3 currentDrag = newVelocity.normalized * _drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > _drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);

            _characterController.Move(newVelocity * Time.deltaTime);
        }    
    }
}
