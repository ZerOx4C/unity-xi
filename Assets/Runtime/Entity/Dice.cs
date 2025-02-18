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
        private readonly ReactiveProperty<(int top, int front, int right)> _faceValues = new((1, 3, 5));
        private readonly ReactiveProperty<Vector2Int> _rollingDirection = new(Vector2Int.zero);
        private readonly ReactiveProperty<Vector2Int> _slidingDirection = new(Vector2Int.zero);

        public ReadOnlyReactiveProperty<bool> CanClimb => _canClimb;
        public ReadOnlyReactiveProperty<bool> CanOverride => _canOverride;
        public ReadOnlyReactiveProperty<bool> CanPush => _canPush;
        public ReadOnlyReactiveProperty<(int top, int front, int right)> FaceValues => _faceValues;
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

        public void Roll(Vector2Int direction)
        {
            AssertUtility.IsValidDirection(direction);

            var values = _faceValues.Value;

            if (direction.x < 0)
            {
                values = (values.right, values.front, 7 - values.top);
            }
            else if (0 < direction.x)
            {
                values = (7 - values.right, values.front, values.top);
            }
            else if (direction.y < 0)
            {
                values = (7 - values.front, values.top, values.right);
            }
            else
            {
                values = (values.front, 7 - values.top, values.right);
            }

            _faceValues.Value = values;
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
