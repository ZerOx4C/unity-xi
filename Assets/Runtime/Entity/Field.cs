using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Runtime.Entity
{
    public class Field : IDisposable
    {
        private readonly RectInt _bounds;
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

        public void AddDice(Dice dice, Vector2Int position)
        {
            var index = GetIndex(position);

            Assert.IsNull(_dices[index]);

            _dices[index] = dice;
            _onDiceAdd.OnNext(dice);
        }

        public bool TryGetDice(Vector2Int position, out Dice dice)
        {
            dice = _dices[GetIndex(position)];
            return dice != null;
        }

        public Vector2Int GetDicePosition(Dice dice)
        {
            Assert.IsTrue(_dices.Contains(dice));

            var index = Array.IndexOf(_dices, dice);
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

            await targetDice.SlidingDirection
                .Where(v => v == Vector2Int.zero)
                .FirstAsync(cancellation)
                .AsUniTask();

            MoveDice(targetDice, nextPosition);

            return true;
        }

        private void MoveDice(Dice targetDice, Vector2Int destPosition)
        {
            var destIndex = GetIndex(destPosition);
            var sourceIndex = Array.IndexOf(_dices, targetDice);

            var oldDice = _dices[destIndex];
            if (oldDice != null)
            {
                _onDiceRemove.OnNext(oldDice);
            }

            _dices[destIndex] = targetDice;
            _dices[sourceIndex] = null;
        }

        private int GetIndex(Vector2Int position)
        {
            Assert.IsTrue(_bounds.Contains(position));

            return position.y * Width + position.x;
        }
    }
}
