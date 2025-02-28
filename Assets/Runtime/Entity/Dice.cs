using System;
using R3;
using Runtime.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Entity
{
    public enum DiceMovementType
    {
        None,
        Roll,
        Slide,
    }

    public class Dice : IDisposable
    {
        private const float CanClimbHeight = 0.5f;
        private const float CanOverrideHeight = 0.2f;
        private const float SpawnDuration = 1;
        private const float VanishDuration = 1f;

        private readonly ReactiveProperty<bool> _canClimb = new(true);
        private readonly ReactiveProperty<bool> _canOverride = new(true);
        private readonly ReactiveProperty<(int top, int front, int right)> _faceValues = new((1, 3, 5));
        private readonly ReactiveProperty<float> _height = new(0);
        private readonly ReactiveProperty<DiceMovementType> _movementType = new(DiceMovementType.None);
        private readonly ReactiveProperty<Vector2Int> _movingDirection = new(Vector2Int.zero);
        private readonly DiceMovementType _pushMovementType;
        private readonly ReactiveProperty<bool> _spawning = new(true);
        private readonly ReactiveProperty<bool> _vanishing = new(false);
        private float _spawnProgress;
        private float _vanishProgress;

        public Dice(bool spawnImmediate = false, DiceMovementType pushMovementType = DiceMovementType.Slide)
        {
            if (spawnImmediate)
            {
                Tick(SpawnDuration);
            }

            _pushMovementType = pushMovementType;
        }

        public ReadOnlyReactiveProperty<bool> CanClimb => _canClimb;
        public ReadOnlyReactiveProperty<bool> CanOverride => _canOverride;
        public ReadOnlyReactiveProperty<(int top, int front, int right)> FaceValues => _faceValues;
        public ReadOnlyReactiveProperty<float> Height => _height;
        public ReadOnlyReactiveProperty<DiceMovementType> MovementType => _movementType;
        public ReadOnlyReactiveProperty<Vector2Int> MovingDirection => _movingDirection;
        public ReactiveProperty<Vector2Int> Position { get; } = new(Vector2Int.zero);
        public ReadOnlyReactiveProperty<bool> Spawning => _spawning;

        public ReadOnlyReactiveProperty<int> Value => _faceValues
            .Select(v => v.top)
            .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<bool> Vanishing => _vanishing;

        public void Dispose()
        {
            _canClimb.Dispose();
            _canOverride.Dispose();
            _faceValues.Dispose();
            _height.Dispose();
            _movementType.Dispose();
            _movingDirection.Dispose();
            Position.Dispose();
            _spawning.Dispose();
            _vanishing.Dispose();
        }

        public void Randomize()
        {
            var count = Random.Range(0, 6);
            for (var i = 0; i < count; i++)
            {
                Roll(i % 2 == 0 ? Vector2Int.right : Vector2Int.up);
            }

            count = Random.Range(0, 4);
            while (0 < count--)
            {
                Roll(Vector2Int.up);
            }
        }

        public void Roll(Vector2Int direction)
        {
            AssertUtility.IsValidDirection(direction);

            var values = _faceValues.Value;

            if (direction.x < 0)
            {
                values = (values.right, values.front, 7 - values.top);
            }
            else if (0 < direction.x)
            {
                values = (7 - values.right, values.front, values.top);
            }
            else if (direction.y < 0)
            {
                values = (7 - values.front, values.top, values.right);
            }
            else
            {
                values = (values.front, 7 - values.top, values.right);
            }

            _faceValues.Value = values;
        }

        public bool TryBeginPush(Vector2Int direction)
        {
            AssertUtility.IsValidDirection(direction);

            if (_height.Value < 1)
            {
                return false;
            }

            if (_movementType.Value != DiceMovementType.None)
            {
                Debug.Log("Dice is still moving.");
                return false;
            }

            if (_pushMovementType == DiceMovementType.None)
            {
                Debug.Log("Dice cannot be pushed.");
                return false;
            }

            _movementType.Value = _pushMovementType;
            _movingDirection.Value = direction;
            return true;
        }

        public void EndPush()
        {
            if (_movementType.Value == DiceMovementType.Roll)
            {
                Roll(_movingDirection.Value);
            }

            Position.Value += _movingDirection.Value;

            _movementType.Value = DiceMovementType.None;
            _movingDirection.Value = Vector2Int.zero;
        }

        public void Tick(float deltaTime)
        {
            UpdateSpawnProgress(deltaTime);
            UpdateVanishProgress(deltaTime);
            UpdateHeight();

            _canClimb.Value = _height.Value <= CanClimbHeight;
            _canOverride.Value = _height.Value <= CanOverrideHeight;
        }

        private void UpdateSpawnProgress(float deltaTime)
        {
            if (!_spawning.Value)
            {
                return;
            }

            _spawnProgress = Mathf.Clamp01(_spawnProgress + deltaTime / SpawnDuration);

            if (1 <= _spawnProgress)
            {
                _spawning.Value = false;
            }
        }

        private void UpdateVanishProgress(float deltaTime)
        {
            if (!_vanishing.Value)
            {
                return;
            }

            _vanishProgress = Mathf.Clamp01(_vanishProgress + deltaTime / VanishDuration);

            if (1 <= _vanishProgress)
            {
                _vanishing.Value = false;
            }
        }

        private void UpdateHeight()
        {
            if (_spawning.Value)
            {
                _height.Value = _spawnProgress;
            }
            else if (_vanishing.Value)
            {
                _height.Value = 1 - _vanishProgress;
            }
            else
            {
                _height.Value = 1;
            }
        }
    }
}
