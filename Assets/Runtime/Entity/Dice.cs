using System;
using R3;
using UnityEngine;
using UnityEngine.Assertions;

namespace Runtime.Entity
{
    public class Dice : IDisposable
    {
        private readonly ReactiveProperty<bool> _canClimb = new(false);
        private readonly ReactiveProperty<bool> _canOverride = new(false);
        private readonly ReactiveProperty<bool> _canPush = new(true);
        private readonly ReactiveProperty<Vector2Int> _pushDirection = new(Vector2Int.zero);

        public ReadOnlyReactiveProperty<bool> CanClimb => _canClimb;
        public ReadOnlyReactiveProperty<bool> CanOverride => _canOverride;
        public ReadOnlyReactiveProperty<bool> CanPush => _canPush;
        public ReadOnlyReactiveProperty<Vector2Int> PushDirection => _pushDirection;

        public void Dispose()
        {
            _canClimb.Dispose();
            _canOverride.Dispose();
            _canPush.Dispose();
            _pushDirection.Dispose();
        }

        public bool TryBeginPush(Vector2Int direction)
        {
            Assert.IsTrue(direction == Vector2Int.left ||
                          direction == Vector2Int.right ||
                          direction == Vector2Int.up ||
                          direction == Vector2Int.down);

            if (_pushDirection.Value != Vector2Int.zero)
            {
                Debug.Log("Dice is already pushing.");
                return false;
            }

            _pushDirection.Value = direction;
            return true;
        }

        public void EndPush()
        {
            _pushDirection.Value = Vector2Int.zero;
        }
    }
}
