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
        private readonly Game _game;

        [Inject]
        public DebugLogPresenter(Game game)
        {
            _game = game;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Start()
        {
            _game.Session
                .WhereNotNull()
                .Select(s => s.Player.DiscretePosition)
                .DistinctUntilChanged()
                .Subscribe(v => Debug.Log($"position = {v}"))
                .AddTo(_disposables);
        }
    }
}
