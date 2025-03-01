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
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;
        private readonly SessionPresenter _sessionPresenter;
        private readonly UIInitialization _uiInitialization;

        [Inject]
        public TestEntryPoint(
            PlayerInputSubject playerInput,
            Session session,
            SessionPresenter sessionPresenter,
            UIInitialization uiInitialization)
        {
            _playerInput = playerInput;
            _session = session;
            _sessionPresenter = sessionPresenter;
            _uiInitialization = uiInitialization;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _uiInitialization.PerformAsync(cancellation);
            await _sessionPresenter.InitializeAsync(cancellation);
            await _sessionPresenter.BindAsync(_session, cancellation);

            _playerInput.Enable();
            _session.Start();
        }

        public void Tick()
        {
            if (!_session.Started)
            {
                return;
            }

            _playerInput.Tick();
            _session.Tick(Time.deltaTime);
        }
    }
}
