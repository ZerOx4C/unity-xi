using System;
using R3;
using UnityEngine.InputSystem;

namespace Runtime.Input
{
    public class InputSubject : IInputObservable, IDisposable
    {
        private readonly Subject<InputAction.CallbackContext> _onBegan = new();
        private readonly Subject<InputAction.CallbackContext> _onEnded = new();
        private readonly Subject<InputAction.CallbackContext> _onProgress = new();
        private InputAction.CallbackContext? _progressContext;

        public void Dispose()
        {
            _onBegan.Dispose();
            _onEnded.Dispose();
            _onProgress.Dispose();
        }

        public Observable<InputAction.CallbackContext> OnBegan => _onBegan;
        public Observable<InputAction.CallbackContext> OnEnded => _onEnded;
        public Observable<InputAction.CallbackContext> OnProgress => _onProgress;

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _progressContext = context;
            }
            else if (context.started)
            {
                _onBegan.OnNext(context);
                _progressContext = context;
            }
            else if (context.canceled)
            {
                _onEnded.OnNext(context);
                _progressContext = null;
            }
        }

        public void Tick()
        {
            if (!_progressContext.HasValue)
            {
                return;
            }

            _onProgress.OnNext(_progressContext.Value);
        }
    }
}
