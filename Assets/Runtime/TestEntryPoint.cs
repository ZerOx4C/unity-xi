using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using Runtime.UseCase;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestEntryPoint : IAsyncStartable, ITickable
    {
        private readonly Game _game;
        private readonly GamePresenter _gamePresenter;
        private readonly PlayerInputSubject _playerInput;
        private readonly SessionPresenter _sessionPresenter;
        private readonly UIInitialization _uiInitialization;

        [Inject]
        public TestEntryPoint(
            Game game,
            GamePresenter gamePresenter,
            PlayerInputSubject playerInput,
            SessionPresenter sessionPresenter,
            UIInitialization uiInitialization)
        {
            _game = game;
            _gamePresenter = gamePresenter;
            _playerInput = playerInput;
            _sessionPresenter = sessionPresenter;
            _uiInitialization = uiInitialization;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _uiInitialization.PerformAsync(cancellation);
            await _sessionPresenter.InitializeAsync(cancellation);

            _gamePresenter.Initialize();

            _playerInput.Enable();
        }

        public void Tick()
        {
            _playerInput.Tick();
            _game.Tick(Time.deltaTime);
        }
    }
}
