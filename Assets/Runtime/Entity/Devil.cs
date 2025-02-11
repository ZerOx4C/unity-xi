using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : IDisposable
    {
        private readonly ReactiveProperty<Vector2> _direction = new(Vector2.up);
        private readonly ReactiveProperty<float> _speed = new(0);

        public Vector2 DesiredDirection { get; set; } = Vector2.up;
        public float DesiredSpeed { get; set; }

        public ReadOnlyReactiveProperty<Vector2> Direction => _direction;
        public ReadOnlyReactiveProperty<float> Speed => _speed;

        public void Dispose()
        {
            Direction.Dispose();
            Speed.Dispose();
        }

        public void Tick(float deltaTime)
        {
            _direction.Value = DesiredDirection;
            _speed.Value = DesiredSpeed;
        }
    }
}
