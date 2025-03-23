using System;
using System.ComponentModel;
using R3;
using Runtime.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Runtime.Entity
{
    public class Dice : IDisposable
    {
        private const float CanOverrideHeight = 0.2f;
        private const float SpawnDuration = 1f;
        private const float VanishDuration = 10f;
        private const float VanishRewindRate = 0.1f;

        private readonly DiceMovementType _driveMovementType;
        private readonly ReactiveProperty<(int top, int front, int right)> _faceValues = new((1, 3, 5));
        private readonly ReactiveProperty<float> _height = new(0);
        private readonly DiceMovementType _pushMovementType;
        private readonly ReactiveProperty<DiceState> _state = new(DiceState.Spawning);

        public Dice(
            bool spawnImmediate = false,
            DiceMovementType pushMovementType = DiceMovementType.Slide,
            DiceMovementType driveMovementType = DiceMovementType.Roll)
        {
            if (spawnImmediate)
            {
                Tick(SpawnDuration);
            }

            _pushMovementType = pushMovementType;
            _driveMovementType = driveMovementType;
        }

        public Vector2Int MovingDirection { get; private set; }
        public ReadOnlyReactiveProperty<DiceState> State => _state;
        public ReadOnlyReactiveProperty<(int top, int front, int right)> FaceValues => _faceValues;
        public ReactiveProperty<Vector2Int> Position { get; } = new(Vector2Int.zero);
        public ReadOnlyReactiveProperty<float> Height => _height;

        public ReadOnlyReactiveProperty<bool> CanOverride => _height
            .Select(v => v <= CanOverrideHeight)
            .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<int> Value => _faceValues
            .Select(v => v.top)
            .ToReadOnlyReactiveProperty();

        public void Dispose()
        {
            _faceValues.Dispose();
            _height.Dispose();
            _state.Dispose();
            Position.Dispose();
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

        public bool CanMove(DiceMovementMethod method)
        {
            if (_state.Value != DiceState.Idle)
            {
                return false;
            }

            var movementType = method switch
            {
                DiceMovementMethod.Push => _pushMovementType,
                DiceMovementMethod.Drive => _driveMovementType,
                _ => throw new InvalidEnumArgumentException(nameof(method), (int)method, typeof(DiceMovementMethod)),
            };

            return movementType switch
            {
                DiceMovementType.Roll => true,
                DiceMovementType.Slide => true,
                _ => false,
            };
        }

        public void BeginMove(DiceMovementMethod method, Vector2Int direction)
        {
            AssertUtility.IsValidDirection(direction);
            Assert.IsTrue(CanMove(method));

            var movementType = method switch
            {
                DiceMovementMethod.Push => _pushMovementType,
                DiceMovementMethod.Drive => _driveMovementType,
                _ => throw new InvalidEnumArgumentException(nameof(method), (int)method, typeof(DiceMovementMethod)),
            };

            MovingDirection = direction;
            _state.Value = movementType switch
            {
                DiceMovementType.Roll => DiceState.Rolling,
                DiceMovementType.Slide => DiceState.Sliding,
                _ => DiceState.Idle,
            };
        }

        public void EndMove()
        {
            MovingDirection = Vector2Int.zero;
            _state.Value = DiceState.Idle;
        }

        public void BeginVanish()
        {
            _state.Value = DiceState.Vanishing;
        }

        public void RewindVanish()
        {
            _height.Value = Mathf.Clamp01(_height.Value - VanishRewindRate);
        }

        public void Tick(float deltaTime)
        {
            TickSpawning(deltaTime);
            TickVanishing(deltaTime);
        }

        private void TickSpawning(float deltaTime)
        {
            if (_state.Value != DiceState.Spawning)
            {
                return;
            }

            _height.Value = Mathf.Clamp01(_height.Value + deltaTime / SpawnDuration);

            if (1 <= _height.Value)
            {
                _state.Value = DiceState.Idle;
            }
        }

        private void TickVanishing(float deltaTime)
        {
            if (_state.Value != DiceState.Vanishing)
            {
                return;
            }

            _height.Value = Mathf.Clamp01(_height.Value - deltaTime / VanishDuration);

            if (_height.Value <= 0)
            {
                _state.Value = DiceState.Vanished;
            }
        }
    }
}
