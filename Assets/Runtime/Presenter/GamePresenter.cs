using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class GamePresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly Game _game;
        private readonly SessionPresenter _sessionPresenter;
        private readonly UIBehaviour _uiBehaviourPrefab;
        private readonly UIPresenter _uiPresenter;

        [Inject]
        public GamePresenter(
            Game game,
            SessionPresenter sessionPresenter,
            UIBehaviour uiBehaviourPrefab,
            UIPresenter uiPresenter)
        {
            _game = game;
            _sessionPresenter = sessionPresenter;
            _uiBehaviourPrefab = uiBehaviourPrefab;
            _uiPresenter = uiPresenter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            var uiBehaviour = await Instantiator.Create(_uiBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            _uiPresenter.Bind(_game, uiBehaviour);

            _game.Session
                .SubscribeAwait((session, token) => _sessionPresenter.BindAsync(session, token))
                .AddTo(_disposables);
        }
    }
}
