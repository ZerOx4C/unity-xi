using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Entity
{
    public class Field : IDisposable
    {
        private readonly RectInt _bounds;
        private readonly Dictionary<Dice, IDisposable> _diceDisposableTable = new();
        private readonly Dice[] _dices;
        private readonly Subject<Dice> _onDiceAdd = new();
        private readonly Subject<Dice> _onDiceRemove = new();

        public Field(int width, int height)
        {
            _bounds = new RectInt(0, 0, width, height);
            _dices = new Dice[width * height];
        }

        public int Width => _bounds.width;
        public int Height => _bounds.height;
        public Observable<Dice> OnDiceAdd => _onDiceAdd;
        public Observable<Dice> OnDiceRemove => _onDiceRemove;

        public void Dispose()
        {
            _onDiceAdd.Dispose();
            _onDiceRemove.Dispose();
        }

        public void AddDice(Dice dice)
        {
            if (_dices.Contains(dice))
            {
                throw new InvalidOperationException("Dice is already added.");
            }

            var index = GetIndex(dice.Position.Value);
            if (_dices[index] != null)
            {
                throw new InvalidOperationException("Position is already used.");
            }

            var disposables = new CompositeDisposable();
            _diceDisposableTable.Add(dice, disposables);

            dice.Position
                .Pairwise()
                .Subscribe(v => OnDicePositionChange(dice, v.Previous, v.Current))
                .AddTo(disposables);

            _dices[index] = dice;
            _onDiceAdd.OnNext(dice);
        }

        public void RemoveDice(Dice dice)
        {
            var index = Array.IndexOf(_dices, dice);
            if (index < 0)
            {
                throw new InvalidOperationException("Dice is not added to this field.");
            }

            _diceDisposableTable.Remove(dice, out var disposables);
            disposables.Dispose();

            _dices[index] = null;
            _onDiceRemove.OnNext(dice);
        }

        public bool TryGetDice(Vector2Int position, out Dice dice)
        {
            dice = _dices[GetIndex(position)];
            return dice != null;
        }

        public Vector2Int GetDicePosition(Dice dice)
        {
            var index = Array.IndexOf(_dices, dice);
            if (index < 0)
            {
                throw new InvalidOperationException("Dice is not added to this field.");
            }

            return new Vector2Int(index % Width, index / Width);
        }

        public async UniTask<bool> TryPushDiceAsync(Dice targetDice, Vector2Int direction, CancellationToken cancellation)
        {
            AssertUtility.IsValidDirection(direction);

            var nextPosition = GetDicePosition(targetDice) + direction;
            if (!_bounds.Contains(nextPosition))
            {
                Debug.Log("Dice is on edge.");
                return false;
            }

            if (TryGetDice(nextPosition, out var nextDice) && !nextDice.CanOverride.CurrentValue)
            {
                Debug.Log("Dice cannot be overriden.");
                return false;
            }

            if (!targetDice.TryBeginPush(direction))
            {
                return false;
            }

            await targetDice.MovingDirection
                .Where(v => v == Vector2Int.zero)
                .FirstAsync(cancellation)
                .AsUniTask();

            MoveDice(targetDice, nextPosition);

            return true;
        }

        private void MoveDice(Dice targetDice, Vector2Int destPosition)
        {
            var destIndex = GetIndex(destPosition);

            var oldDice = _dices[destIndex];
            if (oldDice != null)
            {
                _diceDisposableTable.Remove(oldDice, out var disposables);
                disposables.Dispose();

                _onDiceRemove.OnNext(oldDice);
            }

            targetDice.Position.Value = destPosition;
        }

        private int GetIndex(Vector2Int position)
        {
            if (!_bounds.Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is not on this field.");
            }

            return position.y * Width + position.x;
        }

        private void OnDicePositionChange(Dice dice, Vector2Int oldPosition, Vector2Int newPosition)
        {
            var oldDice = _dices[GetIndex(newPosition)];
            if (oldDice != null)
            {
                RemoveDice(oldDice);
            }

            _dices[GetIndex(newPosition)] = dice;
            _dices[GetIndex(oldPosition)] = null;
        }
    }
}
