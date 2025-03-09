using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Entity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Tester
{
    public class DiceRandomizeTester : MonoBehaviour
    {
        public enum RandomizeType
        {
            None,
            Cross4X4,
            ZigZag6X4,
        }

        public bool running = true;
        public RandomizeType randomizeType;

        [Header("検証結果")] public int trialCount;
        public string result1;
        public string result2;
        public string result3;
        public string result4;
        public string result5;
        public string result6;

        private readonly Dictionary<(int top, int front), int> _resultTable = new();

        private RandomizeType _lastRandomizeType;
        private Action<Dice> _randomizer;

        private void Update()
        {
            UpdateRandomizer();

            if (!running)
            {
                return;
            }

            for (var _ = 0; _ < 100; _++)
            {
                PerformTrial();
            }

            UpdateResult();
        }

        private void UpdateRandomizer()
        {
            if (randomizeType == _lastRandomizeType)
            {
                return;
            }

            trialCount = 0;
            _resultTable.Clear();
            _randomizer = randomizeType switch
            {
                RandomizeType.Cross4X4 => RandomizeCross4X4,
                RandomizeType.ZigZag6X4 => RandomizeZigZag6X4,
                _ => null,
            };

            _lastRandomizeType = randomizeType;
        }

        private void PerformTrial()
        {
            var dice = new Dice();
            _randomizer?.Invoke(dice);

            var key = (dice.FaceValues.CurrentValue.top, dice.FaceValues.CurrentValue.front);
            _resultTable[key] = _resultTable.GetValueOrDefault(key) + 1;

            dice.Dispose();
            ++trialCount;
        }

        private void UpdateResult()
        {
            var resultPairArray = _resultTable
                .GroupBy(kvp => kvp.Key.top)
                .OrderBy(group => group.Key)
                .Select(group => (top: group.Key, count: group.Sum(kvp => kvp.Value)))
                .ToArray();

            var resultArray = Enumerable.Range(1, 6)
                .Select(v => resultPairArray.FirstOrDefault(r => r.top == v).count)
                .Select(v => $"{100f * v / trialCount:f1}%")
                .ToArray();

            result1 = resultArray[0];
            result2 = resultArray[1];
            result3 = resultArray[2];
            result4 = resultArray[3];
            result5 = resultArray[4];
            result6 = resultArray[5];
        }

        private static void RandomizeCross4X4(Dice dice)
        {
            var x = Random.Range(0, 4);
            while (0 < x--)
            {
                dice.Roll(Vector2Int.right);
            }

            var y = Random.Range(0, 4);
            while (0 < y--)
            {
                dice.Roll(Vector2Int.up);
            }
        }

        private static void RandomizeZigZag6X4(Dice dice)
        {
            var count = Random.Range(0, 6);
            for (var i = 0; i < count; i++)
            {
                dice.Roll(i % 2 == 0 ? Vector2Int.right : Vector2Int.up);
            }

            count = Random.Range(0, 4);
            while (0 < count--)
            {
                dice.Roll(Vector2Int.up);
            }
        }
    }
}
