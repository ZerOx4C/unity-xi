using System;
using R3;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Entity
{
    public class Dice : IDisposable
    {
        private readonly ReactiveProperty<bool> _canClimb = new(false);
        private readonly ReactiveProperty<bool> _canOverride = new(false);
        private readonly ReactiveProperty<bool> _canPush = new(true);
        private readonly ReactiveProperty<Vector2Int> _rollingDirection = new(Vector2Int.zero);
        private readonly ReactiveProperty<Vector2Int> _slidingDirection = new(Vector2Int.zero);

        public ReadOnlyReactiveProperty<bool> CanClimb => _canClimb;
        public ReadOnlyReactiveProperty<bool> CanOverride => _canOverride;
        public ReadOnlyReactiveProperty<bool> CanPush => _canPush;
        public ReadOnlyReactiveProperty<Vector2Int> RollingDirection => _rollingDirection;
        public ReadOnlyReactiveProperty<Vector2Int> SlidingDirection => _slidingDirection;

        public void Dispose()
        {
            _canClimb.Dispose();
            _canOverride.Dispose();
            _canPush.Dispose();
            _rollingDirection.Dispose();
            _slidingDirection.Dispose();
        }

        public bool TryBeginPush(Vector2Int direction)
        {
            AssertUtility.IsValidDirection(direction);

            if (_slidingDirection.Value != Vector2Int.zero)
            {
                Debug.Log("Dice is already pushing.");
                return false;
            }

            _slidingDirection.Value = direction;
            return true;
        }

        public void EndPush()
        {
            _slidingDirection.Value = Vector2Int.zero;
        }
    }
}
