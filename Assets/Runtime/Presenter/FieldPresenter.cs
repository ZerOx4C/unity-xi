using System;
using System.Collections.Generic;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class FieldPresenter : IDisposable
    {
        private readonly DiceBehaviourProvider _diceBehaviourProvider;
        private readonly DicePresenter _dicePresenter;
        private readonly Dictionary<Dice, DiceBehaviour> _diceToBehaviourTable = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly ReactiveProperty<bool> _isReady = new();

        [Inject]
        public FieldPresenter(
            DiceBehaviourProvider diceBehaviourProvider,
            DicePresenter dicePresenter)
        {
            _diceBehaviourProvider = diceBehaviourProvider;
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
            _diceBehaviourProvider.ReleaseAll();

            field.OnDiceAdd
                .Subscribe(dice =>
                {
                    var diceBehaviour = _diceBehaviourProvider.Obtain();
                    _diceToBehaviourTable.Add(dice, diceBehaviour);
                    _dicePresenter.Bind(dice, diceBehaviour);
                })
                .AddTo(_disposables);

            field.OnDiceRemove
                .Subscribe(dice =>
                {
                    _dicePresenter.Unbind(dice);
                    _diceToBehaviourTable.Remove(dice, out var diceBehaviour);
                    _diceBehaviourProvider.Release(diceBehaviour);
                })
                .AddTo(_disposables);

            fieldBehaviour.BeginSetup(1, field.Width, field.Height);
            fieldBehaviour.IsReady
                .Subscribe(_isReady.OnNext)
                .AddTo(_disposables);
        }
    }
}
