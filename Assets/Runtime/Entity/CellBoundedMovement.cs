using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class CellBoundedMovement : IDisposable
    {
        private const float CellPadding = 0.05f;
        private const float MaxSimulationDistance = 0.5f;
        private static readonly Vector3 CellSize = new(1, 1, 0);
        private readonly ICellBoundedMovementOwner _owner;
        private readonly ReactiveProperty<Vector2> _position = new();
        private Bounds _cellBounds;

        public CellBoundedMovement(ICellBoundedMovementOwner owner, Vector2 initialPosition)
        {
            _owner = owner;
            _position.Value = initialPosition;
            UpdateCellBounds(initialPosition);
        }

        public ReadOnlyReactiveProperty<Vector2> Position => _position;

        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => _position
            .Select(Vector2Int.RoundToInt)
            .ToReadOnlyReactiveProperty();

        public void Dispose()
        {
            _position.Dispose();
        }

        public void Simulate(Vector2 desiredPosition)
        {
            if (_cellBounds.Contains(desiredPosition))
            {
                _position.Value = desiredPosition;
                return;
            }

            var newPosition = _position.Value;
            var totalDelta = desiredPosition - _position.Value;
            var remainingDistance = totalDelta.magnitude;
            var direction = totalDelta.normalized;

            while (0 < remainingDistance)
            {
                var distance = Mathf.Min(remainingDistance, MaxSimulationDistance);
                newPosition = Simulate(newPosition, distance * direction);
                remainingDistance -= distance;
            }

            _position.Value = newPosition;
            UpdateCellBounds(newPosition);
        }

        private Vector2 Simulate(Vector2 from, Vector2 delta)
        {
            var to = from + delta;
            var discreteFrom = Vector2Int.RoundToInt(from);
            var discreteToX = Vector2Int.RoundToInt(from + delta * Vector2.right);
            var discreteToY = Vector2Int.RoundToInt(from + delta * Vector2.up);
            var moveBounds = _cellBounds;

            if (discreteFrom != discreteToX && _owner.CanMove(discreteFrom, discreteToX))
            {
                moveBounds.Encapsulate(CreateCellBounds(discreteToX, -CellPadding));
            }

            if (discreteFrom != discreteToY && _owner.CanMove(discreteFrom, discreteToY))
            {
                moveBounds.Encapsulate(CreateCellBounds(discreteToY, -CellPadding));
            }

            if (!moveBounds.Contains(to))
            {
                return moveBounds.ClosestPoint(to);
            }

            var discreteTo = Vector2Int.RoundToInt(to);

            if (discreteFrom != discreteToX && _owner.CanMove(discreteToX, discreteTo))
            {
                return to;
            }

            if (discreteFrom != discreteToY && _owner.CanMove(discreteToY, discreteTo))
            {
                return to;
            }

            var excludeBounds = CreateCellBounds(discreteTo, CellPadding);
            return excludeBounds.ClosestPoint(to);
        }

        private static Bounds CreateCellBounds(Vector2Int position, float expand)
        {
            var bounds = new Bounds(new Vector3(position.x, position.y), CellSize);
            bounds.Expand(expand);
            return bounds;
        }

        private void UpdateCellBounds(Vector2 position)
        {
            _cellBounds = CreateCellBounds(Vector2Int.RoundToInt(position), -CellPadding);
        }
    }
}
