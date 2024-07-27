using UnityEngine;
using UnityEngine.InputSystem;

namespace Youregone.FinalCharacterController
{
    public class PlayerInput : MonoBehaviour, PlayerInputActions.IPlayerMovementActions
    {
        [SerializeField] private bool _holdToSprint = true;

        public bool SprintToggledOn { get; private set; }
        public PlayerInputActions PlayerInputActions { get; private set; }
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        private void OnEnable()
        {
            if (PlayerInputActions == null)
                PlayerInputActions = new PlayerInputActions();

            PlayerInputActions.PlayerMovement.Enable();
            PlayerInputActions.PlayerMovement.SetCallbacks(this);
        }

        private void OnDisable()
        {
            PlayerInputActions.PlayerMovement.Disable();
            PlayerInputActions.PlayerMovement.RemoveCallbacks(this);
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnToggleSpring(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SprintToggledOn = _holdToSprint || !SprintToggledOn;
            }
            else if(context.canceled)
            {
                SprintToggledOn = !_holdToSprint && SprintToggledOn;
            }
        }
    }
}
