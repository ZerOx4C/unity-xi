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

            _playerInput.Move.OnBegan.Merge(_playerInput.Move.OnProgress)
                .Select(ctx => 5 * ctx.ReadValue<Vector2>())
                .Subscribe(devil.SetDesiredVelocity)
                .AddTo(_disposables);

            _playerInput.Move.OnEnded
                .Select(_ => Vector2.zero)
                .Subscribe(devil.SetDesiredVelocity)
                .AddTo(_disposables);
        }
    }
}
