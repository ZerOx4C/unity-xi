using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : IDisposable
    {
        private readonly ReactiveProperty<Vector2> _bumpingTime = new();
        private readonly ReactiveProperty<Vector2> _direction = new(Vector2.up);
        private readonly ReactiveProperty<float> _speed = new(0);
        private Vector2 _lastPosition;

        public float MaxDirectionSpeed { get; set; }
        public float MaxAcceleration { get; set; }
        public ReadOnlyReactiveProperty<Vector2> BumpingTime => _bumpingTime;
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
            UpdateDirection(deltaTime);
            UpdateSpeed(deltaTime);
            UpdateBumpingTime(deltaTime);
        }

        public void ResetBumpingTimeX()
        {
            _bumpingTime.Value *= Vector2.up;
        }

        public void ResetBumpingTimeY()
        {
            _bumpingTime.Value *= Vector2.right;
        }

        private void UpdateDirection(float deltaTime)
        {
            var diffAngle = Vector2.SignedAngle(_direction.Value, DesiredDirection);
            if (Mathf.Approximately(diffAngle, 0))
            {
                _direction.Value = DesiredDirection;
                return;
            }

            if (Mathf.Approximately(diffAngle, -180))
            {
                diffAngle = 180;
            }

            var maxDeltaAngle = MaxDirectionSpeed * deltaTime;
            var deltaAngle = Mathf.Clamp(diffAngle, -maxDeltaAngle, maxDeltaAngle);

            _direction.Value = Quaternion.Euler(0, 0, deltaAngle) * _direction.Value;
        }

        private void UpdateSpeed(float deltaTime)
        {
            var directionFactor = Mathf.Max(0, Vector2.Dot(_direction.Value, DesiredDirection));
            var diffSpeed = directionFactor * DesiredSpeed - _speed.Value;

            var maxDeltaSpeed = MaxAcceleration * deltaTime;
            var deltaSpeed = Mathf.Clamp(diffSpeed, -maxDeltaSpeed, maxDeltaSpeed);

            _speed.Value += deltaSpeed;
        }

        private void UpdateBumpingTime(float deltaTime)
        {
            var bumpingTime = _bumpingTime.Value;

            if (Velocity.CurrentValue.x != 0 && Mathf.Approximately(_lastPosition.x, Position.Value.x))
            {
                bumpingTime.x += deltaTime;
            }
            else
            {
                bumpingTime.x = 0;
            }

            if (Velocity.CurrentValue.y != 0 && Mathf.Approximately(_lastPosition.y, Position.Value.y))
            {
                bumpingTime.y += deltaTime;
            }
            else
            {
                bumpingTime.y = 0;
            }

            _bumpingTime.Value = bumpingTime;
            _lastPosition = Position.Value;
        }
    }
}
