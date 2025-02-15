using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Entity;
using Runtime.Input;
using Runtime.UseCase;
using Runtime.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Runtime
{
    public class TestEntryPoint : IAsyncStartable, ITickable, IDisposable
    {
        private readonly DiceFinalization _diceFinalization;
        private readonly DiceInitialization _diceInitialization;
        private readonly CompositeDisposable _disposables = new();
        private readonly FloorInitialization _floorInitialization;
        private readonly PlayerInitialization _playerInitialization;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;
        private readonly TransformConverter _transformConverter;

        private bool _readyToMove;

        [Inject]
        public TestEntryPoint(
            DiceFinalization diceFinalization,
            DiceInitialization diceInitialization,
            FloorInitialization floorInitialization,
            PlayerInitialization playerInitialization,
            PlayerInputSubject playerInput,
            Session session,
            TransformConverter transformConverter)
        {
            _diceFinalization = diceFinalization;
            _diceInitialization = diceInitialization;
            _floorInitialization = floorInitialization;
            _playerInitialization = playerInitialization;
            _playerInput = playerInput;
            _session = session;
            _transformConverter = transformConverter;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var diceInitializationTask = _diceInitialization.InitializeAsync(cancellation);
            var playerInitializationTask = _playerInitialization.PerformAsync(cancellation);
            var floorInitializationTask = _floorInitialization.PerformAsync(cancellation);

            await diceInitializationTask;

            _session.Field.OnDiceAdd
                .SubscribeAwait((dice, token) => _diceInitialization.PerformAsync(dice, token))
                .AddTo(_disposables);

            _session.Field.OnDiceRemove
                .SubscribeAwait((dice, token) => _diceFinalization.PerformAsync(dice, token))
                .AddTo(_disposables);

            _session.Player.Position
                .Select(Vector2Int.RoundToInt)
                .DistinctUntilChanged()
                .Subscribe(v => Debug.Log($"position = {v}"))
                .AddTo(_disposables);

            _transformConverter.SetFieldSize(_session.Field.Width, _session.Field.Height);

            await playerInitializationTask;
            await floorInitializationTask;

            _readyToMove = true;

            var fieldBounds = new RectInt(0, 0, _session.Field.Width, _session.Field.Height);
            foreach (var position in fieldBounds.allPositionsWithin)
            {
                if (0.8f < Random.value)
                {
                    _session.Field.AddDice(new Dice(), position);
                }
            }

            await UniTask.WaitForSeconds(1f, cancellationToken: cancellation);

            while (true)
            {
                var position = new Vector2Int(
                    Random.Range(0, _session.Field.Width),
                    Random.Range(0, _session.Field.Height));

                if (!_session.Field.TryGetDice(position, out var dice))
                {
                    continue;
                }

                var direction = Random.Range(0, 4) switch
                {
                    0 => Vector2Int.left,
                    1 => Vector2Int.right,
                    2 => Vector2Int.up,
                    3 => Vector2Int.down,
                    _ => Vector2Int.zero,
                };

                await _session.Field.TryPushDiceAsync(dice, direction, cancellation);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Tick()
        {
            if (!_readyToMove)
            {
                return;
            }

            _playerInput.Tick();
            _session.Tick(Time.deltaTime);
        }
    }
}
