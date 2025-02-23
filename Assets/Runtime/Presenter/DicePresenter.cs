using System;
using System.Collections.Generic;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using UnityEngine;
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

            dice.FaceValues
                .Subscribe(v => behaviour.SetRotation(_transformConverter.ToDiceRotation(v.top, v.front)))
                .AddTo(disposables);

            dice.MovementType.CombineLatest(dice.MovingDirection, (type, dir) => (type, dir))
                .Where(v => v.type == DiceMovementType.Slide && v.dir != Vector2.zero)
                .SubscribeAwait(async (v, token) =>
                {
                    var direction = _transformConverter.ToView(v.dir);
                    await behaviour.PerformSlideAsync(direction, token);
                    dice.EndPush();
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
