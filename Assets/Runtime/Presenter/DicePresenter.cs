using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using UnityEngine;
using VContainer;

namespace Runtime.Presenter
{
    public class DicePresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DicePresenter(ITransformConverter transformConverter)
        {
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Dice dice, DiceBehaviour behaviour)
        {
            dice.PushDirection
                .Where(dir => dir != Vector2.zero)
                .SubscribeAwait(async (dir, token) =>
                {
                    var direction = _transformConverter.ToView(dir);
                    await behaviour.PerformPush(direction, token);
                    dice.EndPush();
                })
                .AddTo(_disposables);
        }
    }
}
