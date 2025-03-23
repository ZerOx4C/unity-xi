using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Field : IFieldReader, IDisposable
    {
        private readonly RectInt _bounds;
        private readonly Dice[] _dices;
        private readonly Vector2Int[] _neighborOffsets = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        private readonly Subject<Dice> _onDiceAdd = new();
        private readonly Subject<Dice> _onDiceMove = new();
        private readonly Subject<Dice> _onDiceRemove = new();

        public Field(int width, int height)
        {
            _bounds = new RectInt(0, 0, width, height);
            _dices = new Dice[width * height];
        }

        public Observable<Dice> OnDiceAdd => _onDiceAdd;
        public Observable<Dice> OnDiceMove => _onDiceMove;
        public Observable<Dice> OnDiceRemove => _onDiceRemove;

        public void Dispose()
        {
            _onDiceAdd.Dispose();
            _onDiceMove.Dispose();
            _onDiceRemove.Dispose();
        }

        public IEnumerable<Dice> Dices => _dices.Where(d => d != null);
        public int Width => _bounds.width;
        public int Height => _bounds.height;

        public IEnumerable<Vector2Int> GetEmptyPositions()
        {
            var emptyPositions = _dices
                .Select((dice, index) => (dice, index))
                .Where(e => e.dice == null)
                .Select(e => ToPosition(e.index));

            var movingPositions = _dices
                .Where(dice => dice != null && dice.MovingDirection != Vector2.zero)
                .Select(dice => dice.Position.CurrentValue + dice.MovingDirection);

            return emptyPositions.Except(movingPositions);
        }

        public bool TryGetDice(Vector2Int position, out Dice dice)
        {
            dice = _dices[ToIndex(position)];
            return dice != null;
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return _bounds.Contains(position);
        }

        public float GetHeight(Vector2Int position)
        {
            if (TryGetDice(position, out var dice))
            {
                return dice.Height.CurrentValue;
            }

            return 0;
        }

        public void AddDice(Dice dice, Vector2Int position)
        {
            if (_dices.Contains(dice))
            {
                throw new InvalidOperationException("Dice is already added.");
            }

            var index = ToIndex(position);
            if (_dices[index] != null)
            {
                throw new InvalidOperationException("Position is already used.");
            }

            dice.Position.Value = position;
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

            _dices[index] = null;
            _onDiceRemove.OnNext(dice);
        }

        public void MoveDice(Dice dice, Vector2Int position)
        {
            var oldIndex = Array.IndexOf(_dices, dice);
            if (oldIndex < 0)
            {
                throw new InvalidOperationException("Dice is not added to this field.");
            }

            var newIndex = ToIndex(position);

            dice.Position.Value = position;
            _dices[oldIndex] = null;
            _dices[newIndex] = dice;
            _onDiceMove.OnNext(dice);
        }

        public void GetNeighborDices(Vector2Int position, out Dice[] dices)
        {
            dices = _neighborOffsets
                .Select(offset => position + offset)
                .Where(IsValidPosition)
                .Select(ToIndex)
                .Select(index => _dices[index])
                .Where(dice => dice != null)
                .ToArray();
        }

        private int ToIndex(Vector2Int position)
        {
            if (!_bounds.Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is not on this field.");
            }

            return position.y * Width + position.x;
        }

        private Vector2Int ToPosition(int index)
        {
            if (index < 0 || _dices.Length <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            return new Vector2Int(index % Width, index / Width);
        }
    }
}
