using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
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
        private readonly DiceBehaviour _diceBehaviourPrefab;
        private readonly DicePresenter _dicePresenter;
        private readonly CompositeDisposable _disposables = new();
        private readonly FloorInitialization _floorInitialization;
        private readonly PlayerInitialization _playerInitialization;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;
        private readonly TransformConverter _transformConverter;

        private bool _readyToMove;

        [Inject]
        public TestEntryPoint(
            DiceBehaviour diceBehaviourPrefab,
            DicePresenter dicePresenter,
            FloorInitialization floorInitialization,
            PlayerInitialization playerInitialization,
            PlayerInputSubject playerInput,
            Session session,
            TransformConverter transformConverter)
        {
            _diceBehaviourPrefab = diceBehaviourPrefab;
            _dicePresenter = dicePresenter;
            _floorInitialization = floorInitialization;
            _playerInitialization = playerInitialization;
            _playerInput = playerInput;
            _session = session;
            _transformConverter = transformConverter;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var playerInitializationTask = _playerInitialization.InitializeAsync(cancellation);
            var floorInitializationTask = _floorInitialization.InitializeAsync(cancellation);

            _session.Field.OnDiceAdd
                .SubscribeAwait(async (dice, token) =>
                {
                    // TODO: 引数でくれないかなぁ
                    var position = _session.Field.GetDicePosition(dice);

                    var diceBehaviour = await Instantiator.Create(_diceBehaviourPrefab)
                        .SetTransforms(_transformConverter.ToViewPosition(position), Quaternion.identity)
                        .InstantiateAsync(token).First;

                    _dicePresenter.Bind(dice, diceBehaviour);
                })
                .AddTo(_disposables);

            _session.Field.OnDiceRemove
                .Subscribe(dice =>
                {
                    // TODO: どうやって削除しようかな
                })
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
