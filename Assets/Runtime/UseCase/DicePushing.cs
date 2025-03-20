using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Entity;
using UnityEngine;
using VContainer;

namespace Runtime.UseCase
{
    public class DicePushing
    {
        private Field _field;

        [Inject]
        public DicePushing()
        {
        }

        public void SetField(Field field)
        {
            _field = field;
        }

        public async UniTask PerformPushXAsync(Devil devil, CancellationToken cancellation)
        {
            devil.ResetBumpingTimeX();
            await PerformAsync(devil, Vector2Int.right, cancellation);
        }

        public async UniTask PerformPushYAsync(Devil devil, CancellationToken cancellation)
        {
            devil.ResetBumpingTimeY();
            await PerformAsync(devil, Vector2Int.up, cancellation);
        }

        private async UniTask PerformAsync(Devil devil, Vector2Int directionScale, CancellationToken cancellation)
        {
            var direction = Vector2Int.RoundToInt(devil.Velocity.CurrentValue.normalized) * directionScale;
            var dicePosition = devil.DiscretePosition.CurrentValue + direction;

            if (!_field.TryGetDice(dicePosition, out var dice))
            {
                return;
            }

            var nextPosition = dicePosition + direction;
            if (!_field.IsValidPosition(nextPosition))
            {
                Debug.Log("Dice is on the edge.");
                return;
            }

            if (_field.TryGetDice(nextPosition, out var nextDice) && !nextDice.CanOverride.CurrentValue)
            {
                Debug.Log("Dice cannot be overriden.");
                return;
            }

            if (!dice.TryBeginPush(direction))
            {
                return;
            }

            await dice.MovingDirection
                .Where(v => v == Vector2Int.zero)
                .FirstAsync(cancellation)
                .AsUniTask();
        }
    }
}
