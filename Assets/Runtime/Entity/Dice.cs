using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Dice : IDisposable
    {
        private readonly ReactiveProperty<Vector2> _pushDirection = new(Vector2.zero);

        public ReadOnlyReactiveProperty<Vector2> PushDirection => _pushDirection;

        public void Dispose()
        {
            _pushDirection.Dispose();
        }

        public void BeginPush(Vector2 direction)
        {
            if (_pushDirection.Value != Vector2.zero)
            {
                throw new InvalidOperationException("Dice is already pushing.");
            }

            _pushDirection.Value = direction.normalized;
        }

        public void EndPush()
        {
            _pushDirection.Value = Vector2.zero;
        }
    }
}
