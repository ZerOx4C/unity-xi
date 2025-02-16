using System.Threading;
using Cysharp.Threading.Tasks;
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
            var direction = Vector2Int.RoundToInt(devil.Direction.CurrentValue) * directionScale;
            var dicePosition = devil.DiscretePosition.CurrentValue + direction;

            if (!_session.Field.TryGetDice(dicePosition, out var dice))
            {
                return;
            }

            await _session.Field.TryPushDiceAsync(dice, direction, cancellation);
        }
    }
}
