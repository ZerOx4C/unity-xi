using System;
using System.Collections.Generic;
using System.Linq;
using R3;
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

        public IEnumerable<Dice> Dices => _dices.Where(d => d != null);
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

        public bool IsValidPosition(Vector2Int position)
        {
            return _bounds.Contains(position);
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

            // TODO: バニッシュ判定はこの辺ですべきか
        }
    }
}
