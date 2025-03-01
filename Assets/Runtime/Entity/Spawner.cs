using System;
using System.Linq;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Entity
{
    public class Spawner : IDisposable
    {
        private readonly IFieldReader _fieldReader;
        private readonly Subject<Dice> _onSpawn = new();

        public Spawner(IFieldReader fieldReader)
        {
            _fieldReader = fieldReader;
        }

        public Observable<Dice> OnSpawn => _onSpawn;

        public void Dispose()
        {
            _onSpawn.Dispose();
        }

        public void FillRandomly(float targetDensity)
        {
            if (targetDensity is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetDensity), "Density must be normalized.");
            }

            var diceCount = Mathf.CeilToInt(targetDensity * _fieldReader.Width * _fieldReader.Height);
            diceCount -= _fieldReader.Dices.Count();

            var positions = _fieldReader.GetEmptyPositions().ToList();
            while (0 < diceCount--)
            {
                var position = positions[Random.Range(0, positions.Count)];
                positions.Remove(position);

                var dice = new Dice();
                dice.Randomize();
                dice.Position.Value = position;
                _onSpawn.OnNext(dice);
            }
        }

        public void Tick(float deltaTime)
        {
            // TODO: 外から更新される難易度情報をもとに時々ダイスをスポーンさせる
        }
    }
}
