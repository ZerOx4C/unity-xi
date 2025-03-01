using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
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
    public class TestEntryPoint : IAsyncStartable, ITickable, IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly FieldPresenter _fieldPresenter;
        private readonly FloorInitialization _floorInitialization;
        private readonly PlayerInitialization _playerInitialization;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;
        private readonly TransformConverter _transformConverter;
        private readonly UIInitialization _uiInitialization;

        [Inject]
        public TestEntryPoint(
            FieldPresenter fieldPresenter,
            FloorInitialization floorInitialization,
            PlayerInitialization playerInitialization,
            PlayerInputSubject playerInput,
            Session session,
            TransformConverter transformConverter,
            UIInitialization uiInitialization)
        {
            _fieldPresenter = fieldPresenter;
            _floorInitialization = floorInitialization;
            _playerInitialization = playerInitialization;
            _playerInput = playerInput;
            _session = session;
            _transformConverter = transformConverter;
            _uiInitialization = uiInitialization;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var playerInitializationTask = _playerInitialization.PerformAsync(cancellation);
            var floorInitializationTask = _floorInitialization.PerformAsync(cancellation);
            var uiInitializationTask = _uiInitialization.PerformAsync(cancellation);

            _fieldPresenter.Initialize(_session.Field);

            _session.Player.DiscretePosition
                .DistinctUntilChanged()
                .Subscribe(v => Debug.Log($"position = {v}"))
                .AddTo(_disposables);

            _transformConverter.SetFieldSize(_session.Field.Width, _session.Field.Height);

            await playerInitializationTask;
            await floorInitializationTask;
            await uiInitializationTask;

            _session.Start();
        }

        public void Dispose()
        {
            _disposables.Dispose();
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
