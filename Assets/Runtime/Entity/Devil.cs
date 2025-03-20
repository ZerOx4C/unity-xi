using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : IDisposable
    {
        private const float CellPadding = 0.05f;
        private const float MovableHeightGap = 0.55f;
        private static readonly Vector3 CellSize = new(1, 1, 0);
        private readonly ReactiveProperty<Vector2> _bumpingTime = new();
        private readonly DirectionalMovement _directionalMovement;
        private readonly IFieldReader _fieldReader;
        private readonly ReactiveProperty<float> _height = new(0);
        private readonly ReactiveProperty<Vector2> _position = new();
        private Bounds _cellBounds = CreateCellBounds(Vector2Int.zero, -float.Epsilon);
        private Vector2 _lastPosition;

        public Devil(IFieldReader fieldReader, Vector2 initialForward, Vector2 initialPosition)
        {
            _fieldReader = fieldReader;
            _directionalMovement = new DirectionalMovement(initialForward);
            _position.Value = initialPosition;
            UpdateCellBounds(initialPosition);
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
        public ReadOnlyReactiveProperty<Vector2> Position => _position;

        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => _position
            .Select(Vector2Int.RoundToInt)
            .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<Vector2> Forward => _directionalMovement.Forward;
        public ReadOnlyReactiveProperty<Vector2> Velocity => _directionalMovement.Velocity;
        public ReadOnlyReactiveProperty<float> Height => _height;

        public void Dispose()
        {
            _bumpingTime.Dispose();
            _directionalMovement.Dispose();
            _height.Dispose();
            _position.Dispose();
        }

        public void SetDesiredVelocity(Vector2 velocity)
        {
            _directionalMovement.SetDesiredVelocity(velocity);
        }

        public void SetDesiredPosition(Vector2 position)
        {
            if (_cellBounds.Contains(position))
            {
                _position.Value = position;
                return;
            }

            var newPosition = _position.Value;
            var totalDelta = position - _position.Value;
            var remainingDistance = totalDelta.magnitude;
            var direction = totalDelta.normalized;

            while (0 < remainingDistance)
            {
                var factor = Mathf.Min(remainingDistance, 0.5f);
                newPosition = SimulateMove(newPosition, newPosition + factor * direction);
                remainingDistance -= factor;
            }

            _position.Value = newPosition;
            UpdateCellBounds(newPosition);
        }

        private Vector2 SimulateMove(Vector2 from, Vector2 to)
        {
            var delta = to - from;
            var discreteFrom = Vector2Int.RoundToInt(from);
            var discreteToX = Vector2Int.RoundToInt(from + delta * Vector2.right);
            var discreteToY = Vector2Int.RoundToInt(from + delta * Vector2.up);
            var moveBounds = _cellBounds;

            if (discreteFrom != discreteToX && CanMove(discreteFrom, discreteToX))
            {
                moveBounds.Encapsulate(CreateCellBounds(discreteToX, -CellPadding));
            }

            if (discreteFrom != discreteToY && CanMove(discreteFrom, discreteToY))
            {
                moveBounds.Encapsulate(CreateCellBounds(discreteToY, -CellPadding));
            }

            if (!moveBounds.Contains(to))
            {
                return moveBounds.ClosestPoint(to);
            }

            var discreteTo = Vector2Int.RoundToInt(to);

            if (discreteToX != discreteFrom && CanMove(discreteToX, discreteTo))
            {
                return to;
            }

            if (discreteToY != discreteFrom && CanMove(discreteToY, discreteTo))
            {
                return to;
            }

            var excludeBounds = CreateCellBounds(discreteTo, CellPadding);
            return excludeBounds.ClosestPoint(to);
        }

        private bool CanMove(Vector2Int from, Vector2Int to)
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

        private static Bounds CreateCellBounds(Vector2Int position, float expand = 0)
        {
            var bounds = new Bounds(new Vector3(position.x, position.y), CellSize);
            bounds.Expand(expand);
            return bounds;
        }

        private void UpdateCellBounds(Vector2 position)
        {
            _cellBounds = CreateCellBounds(Vector2Int.RoundToInt(position), -CellPadding);
        }

        public void Tick(float deltaTime)
        {
            _directionalMovement.Tick(deltaTime);
            UpdateHeight();
            UpdateBumpingTime(deltaTime);
        }

        private void UpdateHeight()
        {
            _height.Value = _fieldReader.GetHeight(Vector2Int.RoundToInt(_position.Value));
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

            if (Velocity.CurrentValue.x != 0 && Mathf.Approximately(_lastPosition.x, _position.Value.x))
            {
                bumpingTime.x += deltaTime;
            }
            else
            {
                bumpingTime.x = 0;
            }

            if (Velocity.CurrentValue.y != 0 && Mathf.Approximately(_lastPosition.y, _position.Value.y))
            {
                bumpingTime.y += deltaTime;
            }
            else
            {
                bumpingTime.y = 0;
            }

            _bumpingTime.Value = bumpingTime;
            _lastPosition = _position.Value;
        }
    }
}
