using System;
using R3;
using Runtime.Entity;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime.Presenter
{
    public class DebugLogPresenter : IStartable, IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly Session _session;

        [Inject]
        public DebugLogPresenter(Session session)
        {
            _session = session;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Start()
        {
            _session.Player.DiscretePosition
                .DistinctUntilChanged()
                .Subscribe(v => Debug.Log($"position = {v}"))
                .AddTo(_disposables);
        }
    }
}
