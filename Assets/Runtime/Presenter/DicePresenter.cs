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

        [Inject]
        public DicePresenter()
        {
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
                    await behaviour.PerformPush(MiscUtility.Convert(dir), token);
                    dice.EndPush();
                })
                .AddTo(_disposables);
        }
    }
}
