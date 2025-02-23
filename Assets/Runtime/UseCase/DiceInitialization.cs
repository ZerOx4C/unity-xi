using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Presenter;
using Runtime.Utility;
using UnityEngine;
using VContainer;

namespace Runtime.UseCase
{
    public class DiceInitialization
    {
        private readonly Instantiator.Config<DiceBehaviour> _diceBehaviourInstantiator;
        private readonly DiceBehaviourRepository _diceBehaviourRepository;
        private readonly DicePresenter _dicePresenter;

        private Transform _rootTransform;

        [Inject]
        public DiceInitialization(
            DiceBehaviour diceBehaviourPrefab,
            DiceBehaviourRepository diceBehaviourRepository,
            DicePresenter dicePresenter)
        {
            _diceBehaviourInstantiator = Instantiator.Create(diceBehaviourPrefab);
            _diceBehaviourRepository = diceBehaviourRepository;
            _dicePresenter = dicePresenter;
        }

        public UniTask InitializeAsync(CancellationToken cancellation)
        {
            _rootTransform = new GameObject("Dices").transform;
            _diceBehaviourInstantiator.SetParent(_rootTransform);
            return UniTask.CompletedTask;
        }

        public async UniTask PerformAsync(Dice dice, CancellationToken cancellation)
        {
            var behaviour = await _diceBehaviourInstantiator
                .InstantiateAsync(cancellation).First;

            _diceBehaviourRepository.Add(dice, behaviour);
            _dicePresenter.Bind(dice, behaviour);
        }
    }
}
