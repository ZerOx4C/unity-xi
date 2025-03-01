using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using Runtime.UseCase;
using Runtime.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestEntryPoint : IAsyncStartable, ITickable
    {
        private readonly FieldInitialization _fieldInitialization;
        private readonly FieldPresenter _fieldPresenter;
        private readonly PlayerInitialization _playerInitialization;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;
        private readonly TransformConverter _transformConverter;
        private readonly UIInitialization _uiInitialization;

        [Inject]
        public TestEntryPoint(
            FieldInitialization fieldInitialization,
            FieldPresenter fieldPresenter,
            PlayerInitialization playerInitialization,
            PlayerInputSubject playerInput,
            Session session,
            TransformConverter transformConverter,
            UIInitialization uiInitialization)
        {
            _fieldInitialization = fieldInitialization;
            _fieldPresenter = fieldPresenter;
            _playerInitialization = playerInitialization;
            _playerInput = playerInput;
            _session = session;
            _transformConverter = transformConverter;
            _uiInitialization = uiInitialization;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var playerInitializationTask = _playerInitialization.PerformAsync(cancellation);
            var fieldInitializationTask = _fieldInitialization.PerformAsync(cancellation);
            var uiInitializationTask = _uiInitialization.PerformAsync(cancellation);

            _fieldPresenter.Initialize(_session.Field);

            _transformConverter.SetFieldSize(_session.Field.Width, _session.Field.Height);

            await playerInitializationTask;
            await fieldInitializationTask;
            await uiInitializationTask;

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
