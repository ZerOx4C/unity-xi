using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : IDisposable
    {
        private const float CellPadding = 0.05f;
        private static readonly Vector3 CellSize = new(1, 1, 0);
        private readonly ReactiveProperty<Vector2> _bumpingTime = new();
        private readonly ReactiveProperty<Vector2> _faceDirection = new(Vector2.up);
        private readonly IFieldReader _fieldReader;
        private readonly ReactiveProperty<Vector2> _moveDirection = new(Vector2.up);
        private readonly ReactiveProperty<Vector2> _position = new(Vector2.zero);
        private readonly ReactiveProperty<float> _speed = new(0);
        private Bounds _cellBounds = CreateCellBounds(Vector2Int.zero, -float.Epsilon);
        private Vector2 _desiredDirection = Vector2.up;
        private float _desiredSpeed;
        private Vector2 _lastPosition;

        public Devil(IFieldReader fieldReader)
        {
            _fieldReader = fieldReader;
        }

        public float MaxDirectionSpeed { get; set; }
        public float MaxAcceleration { get; set; }
        public ReadOnlyReactiveProperty<Vector2> BumpingTime => _bumpingTime;
        public ReadOnlyReactiveProperty<Vector2> Position => _position;

        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => _position
            .Select(Vector2Int.RoundToInt)
            .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<Vector2> FaceDirection => _faceDirection;
        public ReadOnlyReactiveProperty<Vector2> MoveDirection => _moveDirection;
        public ReadOnlyReactiveProperty<float> Speed => _speed;

        public ReadOnlyReactiveProperty<Vector2> Velocity => _moveDirection
            .CombineLatest(_speed, (d, s) => (d, s))
            .Select(v => v.d * v.s)
            .ToReadOnlyReactiveProperty();

        public void Dispose()
        {
            _bumpingTime.Dispose();
            _faceDirection.Dispose();
            _moveDirection.Dispose();
            _position.Dispose();
            _speed.Dispose();
        }

        public void SetDesiredVelocity(Vector2 velocity)
        {
            _desiredSpeed = velocity.magnitude;

            if (0 < _desiredSpeed)
            {
                _desiredDirection = velocity.normalized;
            }
        }

        public void SetDesiredPosition(Vector2 position, bool force = false)
        {
            if (force)
            {
                _position.Value = position;
                UpdateCellBounds(position);
                return;
            }

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

            if (_fieldReader.TryGetDice(to, out var dice) &&
                !dice.CanClimb.CurrentValue)
            {
                return false;
            }

            // TODO: fromとtoで高さの比較？

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
            _moveDirection.Value = _desiredDirection;
            UpdateFaceDirection(deltaTime);
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

        private void UpdateFaceDirection(float deltaTime)
        {
            var diffAngle = Vector2.SignedAngle(_faceDirection.Value, _desiredDirection);
            if (Mathf.Approximately(diffAngle, 0))
            {
                _faceDirection.Value = _desiredDirection;
                return;
            }

            var maxDeltaAngle = MaxDirectionSpeed * deltaTime;
            var deltaAngle = Mathf.Clamp(diffAngle, -maxDeltaAngle, maxDeltaAngle);

            _faceDirection.Value = Quaternion.Euler(0, 0, deltaAngle) * _faceDirection.Value;
        }

        private void UpdateSpeed(float deltaTime)
        {
            var directionFactor = Mathf.Max(0, Vector2.Dot(_moveDirection.Value, _faceDirection.Value));
            var diffSpeed = directionFactor * _desiredSpeed - _speed.Value;

            var maxDeltaSpeed = MaxAcceleration * deltaTime;
            var deltaSpeed = Mathf.Clamp(diffSpeed, -maxDeltaSpeed, maxDeltaSpeed);

            _speed.Value += deltaSpeed;
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
