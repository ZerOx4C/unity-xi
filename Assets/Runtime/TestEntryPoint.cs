using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestEntryPoint : IAsyncStartable, ITickable, IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly PlayerInputSubject _playerInput;

        private bool _readyToMove;

        [Inject]
        public TestEntryPoint(PlayerInputSubject playerInput)
        {
            _playerInput = playerInput;
        }

        public UniTask StartAsync(CancellationToken cancellation)
        {
            _playerInput.Move.OnBegan
                .Subscribe(c => Debug.Log(c.ReadValue<Vector2>()))
                .AddTo(_disposables);

            _playerInput.Enable();
            _readyToMove = true;

            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Tick()
        {
            if (!_readyToMove)
            {
                return;
            }

            _playerInput.Tick();
        }
    }
}
