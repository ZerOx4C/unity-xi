using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
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

        public void Bind(Game game, DebugUIBehaviour behaviour)
        {
            behaviour.resetButton.OnClickAsObservable()
                .Subscribe(_ => game.Reset())
                .AddTo(_disposables);
        }
    }
}
