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
        private readonly Session _session;

        [Inject]
        public DicePushing(Session session)
        {
            _session = session;
        }

        public async UniTask PerformPushXAsync(Devil devil, CancellationToken cancellation)
        {
            await PerformAsync(devil, Vector2Int.right, cancellation);
        }

        public async UniTask PerformPushYAsync(Devil devil, CancellationToken cancellation)
        {
            await PerformAsync(devil, Vector2Int.up, cancellation);
        }

        private async UniTask PerformAsync(Devil devil, Vector2Int directionScale, CancellationToken cancellation)
        {
            var field = _session.Field;
            var direction = Vector2Int.RoundToInt(devil.Direction.CurrentValue) * directionScale;
            var dicePosition = devil.DiscretePosition.CurrentValue + direction;

            if (!field.TryGetDice(dicePosition, out var dice))
            {
                return;
            }

            var nextPosition = dicePosition + direction;
            if (!field.IsValidPosition(nextPosition))
            {
                Debug.Log("Dice is on the edge.");
                return;
            }

            if (field.TryGetDice(nextPosition, out var nextDice) && !nextDice.CanOverride.CurrentValue)
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
