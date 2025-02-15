using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Entity;
using Runtime.Presenter;
using Runtime.Utility;
using UnityEngine;
using VContainer;

namespace Runtime.UseCase
{
    public class DiceFinalization
    {
        private readonly DiceBehaviourRepository _diceBehaviourRepository;
        private readonly DicePresenter _dicePresenter;

        [Inject]
        public DiceFinalization(
            DiceBehaviourRepository diceBehaviourRepository,
            DicePresenter dicePresenter)
        {
            _diceBehaviourRepository = diceBehaviourRepository;
            _dicePresenter = dicePresenter;
        }

        public UniTask PerformAsync(Dice dice, CancellationToken cancellation)
        {
            _dicePresenter.Unbind(dice);
            _diceBehaviourRepository.Remove(dice, out var behaviour);
            Object.Destroy(behaviour.gameObject);

            return UniTask.CompletedTask;
        }
    }
}
