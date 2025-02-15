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

            dice.PushDirection
                .Where(dir => dir != Vector2.zero)
                .SubscribeAwait(async (dir, token) =>
                {
                    var direction = _transformConverter.ToView(dir);
                    await behaviour.PerformPush(direction, token);
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
