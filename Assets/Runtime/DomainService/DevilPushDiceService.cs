using Runtime.Entity;
using UnityEngine;
using VContainer;

namespace Runtime.DomainService
{
    public class DevilPushDiceService : IDevilPushDiceService
    {
        private readonly Field _field;

        [Inject]
        public DevilPushDiceService(Field field)
        {
            _field = field;
        }

        public void PushDice(Devil devil, Vector2Int direction)
        {
            var dicePosition = devil.DiscretePosition.CurrentValue + direction;
            if (!_field.TryGetDice(dicePosition, out var dice))
            {
                return;
            }

            if (!dice.CanMove.CurrentValue)
            {
                Debug.Log("Dice cannot move.");
                return;
            }

            var nextPosition = dicePosition + direction;
            if (!_field.IsValidPosition(nextPosition))
            {
                Debug.Log("Dice is on the edge.");
                return;
            }

            if (_field.TryGetDice(nextPosition, out var nextDice) &&
                !nextDice.CanOverride.CurrentValue)
            {
                Debug.Log("Dice cannot be overriden.");
                return;
            }

            // TODO: Field側でのDice移動はここに書いた方が良いかも
            dice.TryBeginPush(direction);
        }
    }
}
