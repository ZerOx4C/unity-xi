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

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Field field)
        {
            _disposables.Clear();
            _diceBehaviourRepository.Clear();

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
        }
    }
}
