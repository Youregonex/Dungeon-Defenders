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
        private Vector3 _currentBlendInput = Vector3.zero;

        private static int inputXHash = Animator.StringToHash("InputX");
        private static int inputYHash = Animator.StringToHash("InputY");
        private static int inputMagnitudeHash = Animator.StringToHash("InputMagnitude");

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerState = GetComponent<PlayerState>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isSprinting = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Sprinting;

            Vector2 inputTarget = isSprinting ? _playerInput.MovementInput * 1.5f : _playerInput.MovementInput;

            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, _blendSpeed * Time.deltaTime);

            _animator.SetFloat(inputXHash, _currentBlendInput.x);
            _animator.SetFloat(inputYHash, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        }
    }
}

