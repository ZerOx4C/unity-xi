using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Runtime.Tester
{
    public class DiceTesterEntryPoint : IStartable, ITickable, IDisposable
    {
        private readonly Dice _dice = new();
        private readonly DiceBehaviour _diceBehaviourPrefab;
        private readonly CompositeDisposable _disposables = new();
        private readonly PlayerInputSubject _playerInput;
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DiceTesterEntryPoint(
            DiceBehaviour diceBehaviourPrefab,
            PlayerInputSubject playerInput,
            ITransformConverter transformConverter)
        {
            _diceBehaviourPrefab = diceBehaviourPrefab;
            _playerInput = playerInput;
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _dice.Dispose();
        }

        public void Start()
        {
            TestRandomize("Randomize1", 10000, Randomize1);
            TestRandomize("Randomize2", 10000, Randomize2);

            var diceBehaviour = Instantiator.Create(_diceBehaviourPrefab).Instantiate().First;

            _playerInput.Move.OnBegan
                .Select(v => v.ReadValue<Vector2>())
                .Select(Vector2Int.RoundToInt)
                .Subscribe(_dice.Roll)
                .AddTo(_disposables);

            _dice.FaceValues
                .Subscribe(v => Debug.Log($"(top, front, right) = {v}"))
                .AddTo(_disposables);

            _dice.FaceValues
                .Subscribe(v => diceBehaviour.SetRotation(_transformConverter.ToDiceRotation(v.top, v.front)))
                .AddTo(_disposables);

            _playerInput.Enable();
        }

        public void Tick()
        {
            _playerInput.Tick();
        }

        private static void TestRandomize(string title, int count, Action<Dice> randomizer)
        {
            var result = new Dictionary<(int top, int front), int>();
            for (var _ = 0; _ < count; _++)
            {
                var dice = new Dice();
                randomizer(dice);

                var key = (dice.FaceValues.CurrentValue.top, dice.FaceValues.CurrentValue.front);
                result[key] = result.GetValueOrDefault(key) + 1;
            }

            var odds = string.Join(", ", result
                .GroupBy(p => p.Key.top)
                .OrderBy(p => p.Key)
                .Select(g => $"{g.Key}: {100f * g.Sum(p => p.Value) / count:f1}%"));

            Debug.Log($"[{title}] {odds}");
        }

        private static void Randomize1(Dice dice)
        {
            // TODO: パターン数的に偏ってるハズなので要改善
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

        private static void Randomize2(Dice dice)
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
