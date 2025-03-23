using System;
using System.Collections.Generic;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using UnityEngine.Assertions;
using VContainer;

namespace Runtime.Presenter
{
    public class DicePresenter : IDisposable
    {
        private readonly Dictionary<Dice, IDisposable> _disposableTable = new();
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DicePresenter(ITransformConverter transformConverter)
        {
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposableTable.Values)
            {
                disposable.Dispose();
            }

            _disposableTable.Clear();
        }

        public void Bind(Dice dice, DiceBehaviour behaviour)
        {
            Assert.IsFalse(_disposableTable.ContainsKey(dice));

            var disposables = new CompositeDisposable();

            dice.Position.CombineLatest(dice.Height, (position, height) => (position, height))
                .Subscribe(v =>
                {
                    var position = _transformConverter.ToViewPosition(v.position);
                    position.y = v.height - 1;
                    behaviour.SetPosition(position);
                })
                .AddTo(disposables);

            dice.FaceValues
                .Subscribe(v => behaviour.SetRotation(_transformConverter.ToDiceRotation(v.top, v.front)))
                .AddTo(disposables);

            dice.State
                .Where(v => v == DiceState.Sliding)
                .SubscribeAwait(async (_, token) =>
                {
                    var direction = _transformConverter.ToView(dice.MovingDirection);
                    await behaviour.PerformSlideAsync(direction, token);
                    dice.EndMove();
                })
                .AddTo(disposables);

            dice.State
                .Where(v => v == DiceState.Rolling)
                .SubscribeAwait(async (_, token) =>
                {
                    var direction = _transformConverter.ToView(dice.MovingDirection);
                    await behaviour.PerformRollAsync(direction, token);
                    dice.EndMove();
                })
                .AddTo(disposables);

            _disposableTable.Add(dice, disposables);
        }

        public void Unbind(Dice dice)
        {
            Assert.IsTrue(_disposableTable.ContainsKey(dice));

            _disposableTable.Remove(dice, out var disposables);
            disposables.Dispose();
        }
    }
}
