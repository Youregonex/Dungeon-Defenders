using UnityEngine;

namespace Youregone.FinalCharacterController
{
    public class PlayerState : MonoBehaviour
    {
        [field: SerializeField] public EPlayerMovementState CurrentPlayerMovementState { get; private set; } = EPlayerMovementState.Idling;

        public void SetPlayerMovementState(EPlayerMovementState movementState)
        {
            CurrentPlayerMovementState = movementState;
        }

        public bool IsInGroundedState()
        {
            return CurrentPlayerMovementState == EPlayerMovementState.Idling ||
                   CurrentPlayerMovementState == EPlayerMovementState.Running ||
                   CurrentPlayerMovementState == EPlayerMovementState.Sprinting ||
                   CurrentPlayerMovementState == EPlayerMovementState.Walking;
        }

    }
}
