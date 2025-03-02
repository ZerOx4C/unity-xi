using System;
using System.Linq;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Entity
{
    public class Spawner : IDisposable
    {
        private const float SpawnRoughInterval = 10f;

        private readonly IFieldReader _fieldReader;
        private readonly Subject<Dice> _onSpawn = new();

        private float _elapsedTime;

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

            Spawn(diceCount);
        }

        public void Tick(float deltaTime)
        {
            _elapsedTime += deltaTime;

            // TODO: 外から更新される難易度情報も使う
            var spawnChance = 1f / (1 + Mathf.Exp(SpawnRoughInterval - _elapsedTime));
            if (spawnChance < Random.value)
            {
                return;
            }

            _elapsedTime = 0;
            Spawn(1);
        }

        private int Spawn(int count)
        {
            var positions = _fieldReader.GetEmptyPositions().ToList();
            var ret = Mathf.Min(count, positions.Count);

            while (0 < count--)
            {
                var position = positions[Random.Range(0, positions.Count)];
                positions.Remove(position);

                var dice = new Dice();
                dice.Randomize();
                dice.Position.Value = position;
                _onSpawn.OnNext(dice);
            }

            return ret;
        }
    }
}
