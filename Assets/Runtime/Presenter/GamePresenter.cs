using System;
using R3;
using Runtime.Entity;
using VContainer;

namespace Runtime.Presenter
{
    public class GamePresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly Game _game;
        private readonly SessionPresenter _sessionPresenter;

        [Inject]
        public GamePresenter(Game game, SessionPresenter sessionPresenter)
        {
            _game = game;
            _sessionPresenter = sessionPresenter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize()
        {
            _game.Session
                .SubscribeAwait((session, token) => _sessionPresenter.BindAsync(session, token))
                .AddTo(_disposables);
        }
    }
}
