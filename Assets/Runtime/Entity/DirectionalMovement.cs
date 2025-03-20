using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class DirectionalMovement : IDisposable
    {
        private readonly ReactiveProperty<Vector2> _forward = new();
        private readonly ReactiveProperty<Vector2> _velocity = new();
        private float _currentSpeed;
        private Vector2 _desiredForward;
        private float _desiredSpeed;

        public DirectionalMovement(Vector2 initialForward)
        {
            initialForward.Normalize();
            _forward.Value = initialForward;
            _desiredForward = initialForward;
        }

        public float MaxDirectionSpeed { get; set; }
        public float MaxAcceleration { get; set; }
        public ReadOnlyReactiveProperty<Vector2> Forward => _forward;
        public ReadOnlyReactiveProperty<Vector2> Velocity => _velocity;

        public void Dispose()
        {
            _forward.Dispose();
            _velocity.Dispose();
        }

        public void SetDesiredVelocity(Vector2 velocity)
        {
            _desiredSpeed = velocity.magnitude;

            if (0 < _desiredSpeed)
            {
                _desiredForward = velocity.normalized;
            }
        }

        public void Tick(float deltaTime)
        {
            UpdateForward(deltaTime);
            UpdateVelocity(deltaTime);
        }

        private void UpdateForward(float deltaTime)
        {
            var diffAngle = Vector2.SignedAngle(_forward.Value, _desiredForward);
            if (Mathf.Approximately(diffAngle, 0))
            {
                _forward.Value = _desiredForward;
                return;
            }

            var maxDeltaAngle = MaxDirectionSpeed * deltaTime;
            var deltaAngle = Mathf.Clamp(diffAngle, -maxDeltaAngle, maxDeltaAngle);

            _forward.Value = Quaternion.Euler(0, 0, deltaAngle) * _forward.Value;
        }

        private void UpdateVelocity(float deltaTime)
        {
            var factor = Mathf.Max(0, Vector2.Dot(_forward.Value, _desiredForward));
            var deltaSpeed = factor * _desiredSpeed - _currentSpeed;
            var maxDeltaSpeed = MaxAcceleration * deltaTime;
            _currentSpeed += Mathf.Clamp(deltaSpeed, -maxDeltaSpeed, maxDeltaSpeed);

            _velocity.Value = _currentSpeed * _desiredForward;
        }
    }
}
