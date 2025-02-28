using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Entity
{
    public class Spawner
    {
        private readonly Field _field;

        public Spawner(Field field)
        {
            _field = field;
        }

        public void SpawnInitialDices(float density)
        {
            if (density is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(density), "Density must be normalized.");
            }

            var positionList = new List<Vector2Int>();
            foreach (var position in _field.Bounds.allPositionsWithin)
            {
                positionList.Add(position);
            }

            var diceCount = Mathf.CeilToInt(density * positionList.Count);
            while (0 < diceCount--)
            {
                var position = positionList[Random.Range(0, positionList.Count)];
                positionList.Remove(position);

                var dice = new Dice();
                dice.Randomize();
                dice.Position.Value = position;
                _field.AddDice(dice);
            }
        }

        public void Tick(float deltaTime)
        {
            // TODO: 外から更新される難易度情報をもとに時々ダイスをスポーンさせる
        }
    }
}
