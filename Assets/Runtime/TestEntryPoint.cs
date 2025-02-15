using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using Runtime.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Runtime
{
    public class TestEntryPoint : IAsyncStartable, ITickable, IDisposable
    {
        private readonly DevilBehaviour _devilBehaviourPrefab;
        private readonly DevilPresenter _devilPresenter;
        private readonly DiceBehaviour _diceBehaviourPrefab;
        private readonly DicePresenter _dicePresenter;
        private readonly CompositeDisposable _disposables = new();
        private readonly FloorBehaviour _floorBehaviourPrefab;
        private readonly PlayerDevilController _playerDevilController;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;
        private readonly TransformConverter _transformConverter;

        private bool _readyToMove;

        [Inject]
        public TestEntryPoint(
            DevilBehaviour devilBehaviourPrefab,
            DevilPresenter devilPresenter,
            DiceBehaviour diceBehaviourPrefab,
            DicePresenter dicePresenter,
            FloorBehaviour floorBehaviourPrefab,
            PlayerDevilController playerDevilController,
            PlayerInputSubject playerInput,
            Session session,
            TransformConverter transformConverter)
        {
            _devilBehaviourPrefab = devilBehaviourPrefab;
            _devilPresenter = devilPresenter;
            _diceBehaviourPrefab = diceBehaviourPrefab;
            _dicePresenter = dicePresenter;
            _floorBehaviourPrefab = floorBehaviourPrefab;
            _playerDevilController = playerDevilController;
            _playerInput = playerInput;
            _session = session;
            _transformConverter = transformConverter;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var floorInstantiateTask = Instantiator.Create(_floorBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            var playerInstantiateTask = Instantiator.Create(_devilBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            _session.Field.OnDiceAdd
                .SubscribeAwait(async (dice, token) =>
                {
                    // TODO: 引数でくれないかなぁ
                    var position = _session.Field.GetDicePosition(dice);

                    var diceBehaviour = await Instantiator.Create(_diceBehaviourPrefab)
                        .SetTransforms(_transformConverter.ToView(position), Quaternion.identity)
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

            _transformConverter.SetFieldSize(_session.Field.Width, _session.Field.Height);

            var floorBehaviour = await floorInstantiateTask;
            await floorBehaviour.SetupAsync(new FloorBehaviour.Config
                {
                    CellSize = 1,
                    FieldWidth = _session.Field.Width,
                    FieldHeight = _session.Field.Height,
                },
                cancellation);

            var playerDevilBehaviour = await playerInstantiateTask;
            _devilPresenter.Bind(_session.Player, playerDevilBehaviour);
            _playerDevilController.Initialize(_session.Player);

            _playerInput.Enable();
            _readyToMove = true;

            var fieldBounds = new RectInt(0, 0, _session.Field.Width, _session.Field.Height);
            foreach (var position in fieldBounds.allPositionsWithin)
            {
                if (0.8f < Random.value)
                {
                    _session.Field.AddDice(new Dice(), position);
                }
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
