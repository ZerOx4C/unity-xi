using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : ICellBoundedMovementOwner, IDisposable
    {
        private const float MovableHeightGap = 0.55f;
        private readonly ReactiveProperty<Vector2> _bumpingTime = new();
        private readonly CellBoundedMovement _cellBoundedMovement;
        private readonly DirectionalMovement _directionalMovement;
        private readonly IFieldReader _fieldReader;
        private readonly ReactiveProperty<float> _height = new(0);
        private Vector2 _lastPosition;

        public Devil(IFieldReader fieldReader, Vector2 initialForward, Vector2 initialPosition)
        {
            _fieldReader = fieldReader;
            _cellBoundedMovement = new CellBoundedMovement(this, initialPosition);
            _directionalMovement = new DirectionalMovement(initialForward);
        }

        public float MaxDirectionSpeed
        {
            get => _directionalMovement.MaxDirectionSpeed;
            set => _directionalMovement.MaxDirectionSpeed = value;
        }

        public float MaxAcceleration
        {
            get => _directionalMovement.MaxAcceleration;
            set => _directionalMovement.MaxAcceleration = value;
        }

        public ReadOnlyReactiveProperty<Vector2> BumpingTime => _bumpingTime;
        public ReadOnlyReactiveProperty<Vector2> Position => _cellBoundedMovement.Position;
        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => _cellBoundedMovement.DiscretePosition;
        public ReadOnlyReactiveProperty<Vector2> Forward => _directionalMovement.Forward;
        public ReadOnlyReactiveProperty<Vector2> Velocity => _directionalMovement.Velocity;
        public ReadOnlyReactiveProperty<float> Height => _height;

        bool ICellBoundedMovementOwner.CanMove(Vector2Int from, Vector2Int to)
        {
            if (!_fieldReader.IsValidPosition(to))
            {
                return false;
            }

            var fromHeight = _fieldReader.GetHeight(from);
            var toHeight = _fieldReader.GetHeight(to);
            if (MovableHeightGap < Mathf.Abs(fromHeight - toHeight))
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _bumpingTime.Dispose();
            _cellBoundedMovement.Dispose();
            _directionalMovement.Dispose();
            _height.Dispose();
        }

        public void SetDesiredVelocity(Vector2 velocity)
        {
            _directionalMovement.SetDesiredVelocity(velocity);
        }

        public void SimulateMove(Vector2 desiredPosition)
        {
            _cellBoundedMovement.Simulate(desiredPosition);
        }

        public void Tick(float deltaTime)
        {
            _directionalMovement.Tick(deltaTime);
            UpdateHeight();
            UpdateBumpingTime(deltaTime);
        }

        private void UpdateHeight()
        {
            _height.Value = _fieldReader.GetHeight(DiscretePosition.CurrentValue);
        }

        public void ResetBumpingTimeX()
        {
            _bumpingTime.Value *= Vector2.up;
        }

        public void ResetBumpingTimeY()
        {
            _bumpingTime.Value *= Vector2.right;
        }

        private void UpdateBumpingTime(float deltaTime)
        {
            var bumpingTime = _bumpingTime.Value;
            var currentPosition = Position.CurrentValue;

            if (Velocity.CurrentValue.x != 0 && Mathf.Approximately(_lastPosition.x, currentPosition.x))
            {
                bumpingTime.x += deltaTime;
            }
            else
            {
                bumpingTime.x = 0;
            }

            if (Velocity.CurrentValue.y != 0 && Mathf.Approximately(_lastPosition.y, currentPosition.y))
            {
                bumpingTime.y += deltaTime;
            }
            else
            {
                bumpingTime.y = 0;
            }

            _bumpingTime.Value = bumpingTime;
            _lastPosition = currentPosition;
        }
    }
}
