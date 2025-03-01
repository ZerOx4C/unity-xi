using System;
using R3;
using Runtime.Behaviour;
using UnityEngine;
using VContainer;

namespace Runtime.Presenter
{
    public class DebugUIPresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public DebugUIPresenter()
        {
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize(DebugUIBehaviour behaviour)
        {
            behaviour.resetButton.OnClickAsObservable()
                .Subscribe(_ => Debug.Log("reset")) // TODO: ここでSessionを作り直す
                .AddTo(_disposables);
        }
    }
}
