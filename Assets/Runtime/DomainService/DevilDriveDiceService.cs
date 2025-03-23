using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Entity;
using UnityEngine;
using VContainer;

namespace Runtime.DomainService
{
    public class DevilDriveDiceService : IDevilDriveDiceService
    {
        private readonly Field _field;

        [Inject]
        public DevilDriveDiceService(Field field)
        {
            _field = field;
        }

        public async UniTask DriveDiceAsync(Devil devil, Vector2Int direction, CancellationToken cancellation)
        {
            var dicePosition = devil.DiscretePosition.CurrentValue;
            if (!_field.TryGetDice(dicePosition, out var dice))
            {
                return;
            }

            if (!dice.CanMove(DiceMovementMethod.Drive))
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

            dice.BeginMove(DiceMovementMethod.Drive, direction);

            await dice.State.FirstAsync(v => v == DiceState.Idle, cancellation);

            dice.Roll(direction);
            _field.MoveDice(dice, nextPosition);
        }
    }
}
