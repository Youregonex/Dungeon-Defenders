using UnityEngine;

namespace Youregone.FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _animator;

        [Header("Animator Settings")]
        [SerializeField] private float _blendSpeed = .02f;

        private PlayerState _playerState;
        private PlayerInput _playerInput;
        private PlayerController _playerController;

        private Vector3 _currentBlendInput = Vector3.zero;

        private static int inputXHash = Animator.StringToHash("InputX");
        private static int inputYHash = Animator.StringToHash("InputY");
        private static int inputMagnitudeHash = Animator.StringToHash("InputMagnitude");
        private static int isGroundedHash = Animator.StringToHash("IsGrounded");
        private static int isFallingHash = Animator.StringToHash("IsFalling");
        private static int isJumpingHash = Animator.StringToHash("IsJumping");
        private static int rotationMismatchHash = Animator.StringToHash("RotationMismatch");
        private static int isIdlingHash = Animator.StringToHash("IsIdling");
        private static int isRotatingToTargetHash = Animator.StringToHash("IsRotatingToTarget");

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerState = GetComponent<PlayerState>();
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Idling;
            bool isRunning = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Running;
            bool isSprinting = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Sprinting;
            bool isJumping = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Jumping;
            bool isFalling = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Falling;
            bool isGrounded = _playerState.IsInGroundedState();

            Vector2 inputTarget = isSprinting ? _playerInput.MovementInput * 1.5f :
                                  isRunning ? _playerInput.MovementInput * 1f : _playerInput.MovementInput * .5f;

            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, _blendSpeed * Time.deltaTime);

            _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
            _animator.SetBool(isIdlingHash, isIdling);
            _animator.SetBool(isGroundedHash, isGrounded);
            _animator.SetBool(isJumpingHash, isJumping);
            _animator.SetBool(isFallingHash, isFalling);
            _animator.SetFloat(inputXHash, _currentBlendInput.x);
            _animator.SetFloat(inputYHash, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
            _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
        }
    }
}

