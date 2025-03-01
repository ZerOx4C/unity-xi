using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;
using Object = UnityEngine.Object;

namespace Runtime.Presenter
{
    public class FieldPresenter : IDisposable
    {
        private readonly Instantiator.Config<DiceBehaviour> _diceBehaviourInstantiator;
        private readonly DiceBehaviourRepository _diceBehaviourRepository;
        private readonly DicePresenter _dicePresenter;
        private readonly CompositeDisposable _disposables = new();
        private readonly ReactiveProperty<bool> _isReady = new();

        [Inject]
        public FieldPresenter(
            DiceBehaviour diceBehaviourPrefab,
            DiceBehaviourRepository diceBehaviourRepository,
            DicePresenter dicePresenter)
        {
            _diceBehaviourInstantiator = Instantiator.Create(diceBehaviourPrefab);
            _diceBehaviourRepository = diceBehaviourRepository;
            _dicePresenter = dicePresenter;
        }

        public ReadOnlyReactiveProperty<bool> IsReady => _isReady;

        public void Dispose()
        {
            _isReady.Dispose();
            _disposables.Dispose();
        }

        public void Bind(Field field, FieldBehaviour fieldBehaviour)
        {
            _disposables.Clear();
            _diceBehaviourRepository.Clear();

            fieldBehaviour.IsReady
                .Subscribe(_isReady.OnNext)
                .AddTo(_disposables);

            field.OnDiceAdd
                .SubscribeAwait(async (dice, token) =>
                {
                    var diceBehaviour = await _diceBehaviourInstantiator.InstantiateAsync(token).First;
                    _diceBehaviourRepository.Add(dice, diceBehaviour);
                    _dicePresenter.Bind(dice, diceBehaviour);
                }, AwaitOperation.Parallel)
                .AddTo(_disposables);

            field.OnDiceRemove
                .Subscribe(dice =>
                {
                    _dicePresenter.Unbind(dice);
                    _diceBehaviourRepository.Remove(dice, out var diceBehaviour);
                    Object.Destroy(diceBehaviour.gameObject);
                })
                .AddTo(_disposables);

            fieldBehaviour.BeginSetup(new FieldBehaviour.Config
            {
                CellSize = 1,
                FieldWidth = field.Width,
                FieldHeight = field.Height,
            });
        }
    }
}
