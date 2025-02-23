using System;
using R3;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Entity
{
    public enum DiceMovementType
    {
        None,
        Roll,
        Slide,
    }

    public class Dice : IDisposable
    {
        private readonly ReactiveProperty<bool> _canClimb = new(false);
        private readonly ReactiveProperty<bool> _canOverride = new(false);
        private readonly ReactiveProperty<bool> _canPush = new(true);
        private readonly ReactiveProperty<(int top, int front, int right)> _faceValues = new((1, 3, 5));
        private readonly ReactiveProperty<DiceMovementType> _movementType = new(DiceMovementType.None);
        private readonly ReactiveProperty<Vector2Int> _movingDirection = new(Vector2Int.zero);

        public ReadOnlyReactiveProperty<bool> CanClimb => _canClimb;
        public ReadOnlyReactiveProperty<bool> CanOverride => _canOverride;
        public ReadOnlyReactiveProperty<bool> CanPush => _canPush;
        public ReadOnlyReactiveProperty<(int top, int front, int right)> FaceValues => _faceValues;
        public ReadOnlyReactiveProperty<DiceMovementType> MovementType => _movementType;
        public ReadOnlyReactiveProperty<Vector2Int> MovingDirection => _movingDirection;

        public void Dispose()
        {
            _canClimb.Dispose();
            _canOverride.Dispose();
            _canPush.Dispose();
            _movementType.Dispose();
            _movingDirection.Dispose();
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

            if (_movementType.Value != DiceMovementType.None)
            {
                Debug.Log("Dice is still moving.");
                return false;
            }

            _movementType.Value = DiceMovementType.Slide;
            _movingDirection.Value = direction;
            return true;
        }

        public void EndPush()
        {
            _movementType.Value = DiceMovementType.None;
            _movingDirection.Value = Vector2Int.zero;
        }
    }
}
