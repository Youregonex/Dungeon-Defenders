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

    }
}
