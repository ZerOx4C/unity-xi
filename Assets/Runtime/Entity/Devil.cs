using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : IDisposable
    {
        private readonly ReactiveProperty<Vector2> _direction = new(Vector2.up);
        private readonly ReactiveProperty<Vector2> _pushingTime = new();
        private readonly ReactiveProperty<float> _speed = new(0);
        private Vector2 _lastPosition;

        public ReadOnlyReactiveProperty<Vector2> PushingTime => _pushingTime;
        public Vector2 DesiredDirection { get; set; } = Vector2.up;
        public float DesiredSpeed { get; set; }
        public ReactiveProperty<Vector2> Position { get; } = new();

        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => Position
            .Select(Vector2Int.RoundToInt)
            .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<Vector2> Direction => _direction;
        public ReadOnlyReactiveProperty<float> Speed => _speed;

        public ReadOnlyReactiveProperty<Vector2> Velocity => _direction
            .CombineLatest(_speed, (d, s) => (d, s))
            .Select(v => v.d * v.s)
            .ToReadOnlyReactiveProperty();

        public void Dispose()
        {
            Position.Dispose();
            Direction.Dispose();
            Speed.Dispose();
        }

        public void Tick(float deltaTime)
        {
            _direction.Value = DesiredDirection;
            _speed.Value = DesiredSpeed;

            UpdatePushDirection();
        }

        private void UpdatePushDirection()
        {
            var desiredVelocity = _speed.Value * _direction.Value;
            var pushingTime = _pushingTime.Value;

            if (desiredVelocity.x != 0 && Mathf.Approximately(_lastPosition.x, Position.Value.x))
            {
                pushingTime.x += Time.deltaTime;
            }
            else
            {
                pushingTime.x = 0;
            }

            if (desiredVelocity.y != 0 && Mathf.Approximately(_lastPosition.y, Position.Value.y))
            {
                pushingTime.y += Time.deltaTime;
            }
            else
            {
                pushingTime.y = 0;
            }

            _pushingTime.Value = pushingTime;
            _lastPosition = Position.Value;
        }
    }
}
