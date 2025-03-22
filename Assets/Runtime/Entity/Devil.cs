using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : ICellBoundedMovementOwner, IMovableAgentReader, IDisposable
    {
        private const float MovableHeightGap = 0.55f;
        private readonly BumpingTicker _bumpingTicker;
        private readonly CellBoundedMovement _cellBoundedMovement;
        private readonly DirectionalMovement _directionalMovement;
        private readonly IFieldReader _fieldReader;
        private readonly ReactiveProperty<float> _height = new(0);

        public Devil(IFieldReader fieldReader, Vector2 initialForward, Vector2 initialPosition)
        {
            _fieldReader = fieldReader;
            _bumpingTicker = new BumpingTicker(this);
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

        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => _cellBoundedMovement.DiscretePosition;
        public ReadOnlyReactiveProperty<Vector2> Forward => _directionalMovement.Forward;
        public ReadOnlyReactiveProperty<float> Height => _height;
        public Vector2Int BumpDirectionX => _bumpingTicker.DirectionX;
        public Vector2Int BumpDirectionY => _bumpingTicker.DirectionY;
        public ReadOnlyReactiveProperty<float> BumpDurationX => _bumpingTicker.DurationX;
        public ReadOnlyReactiveProperty<float> BumpDurationY => _bumpingTicker.DurationY;

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
            _bumpingTicker.Dispose();
            _cellBoundedMovement.Dispose();
            _directionalMovement.Dispose();
            _height.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2> Position => _cellBoundedMovement.Position;
        public ReadOnlyReactiveProperty<Vector2> Velocity => _directionalMovement.Velocity;

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
            _height.Value = _fieldReader.GetHeight(DiscretePosition.CurrentValue);

            _bumpingTicker.Tick(deltaTime);
        }
    }
}
