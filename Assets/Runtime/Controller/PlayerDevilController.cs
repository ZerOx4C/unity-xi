using System;
using R3;
using Runtime.Entity;
using Runtime.Input;
using UnityEngine;
using VContainer;

namespace Runtime.Controller
{
    public class PlayerDevilController : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly IPlayerInputObservable _playerInput;

        [Inject]
        public PlayerDevilController(IPlayerInputObservable playerInput)
        {
            _playerInput = playerInput;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize(Devil devil)
        {
            _disposables.Clear();

            _playerInput.Move.OnBegan
                .Subscribe(_ => devil.DesiredSpeed = 5)
                .AddTo(_disposables);

            _playerInput.Move.OnProgress
                .Subscribe(ctx => devil.DesiredDirection = ctx.ReadValue<Vector2>())
                .AddTo(_disposables);

            _playerInput.Move.OnEnded
                .Subscribe(_ => devil.DesiredSpeed = 0)
                .AddTo(_disposables);
        }
    }
}
