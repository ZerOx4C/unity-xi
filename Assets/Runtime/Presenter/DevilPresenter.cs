using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using UnityEngine;
using VContainer;

namespace Runtime.Presenter
{
    public class DevilPresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public DevilPresenter()
        {
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Devil devil, DevilBehaviour behaviour)
        {
            devil.MoveDirection
                .Subscribe(v => Debug.Log($"move: {v}"))
                .AddTo(_disposables);
        }
    }
}
