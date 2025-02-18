using System;
using UnityEngine.InputSystem;
using VContainer;

namespace Runtime.Input
{
    public class PlayerInputSubject :
        IPlayerInputObservable,
        InputSystem_Actions.IPlayerActions,
        IDisposable
    {
        private readonly InputSystem_Actions _inputActions = new();
        private readonly InputSubject _move = new();

        [Inject]
        public PlayerInputSubject()
        {
            _inputActions.Player.AddCallbacks(this);
        }

        public void Dispose()
        {
            _inputActions.Player.RemoveCallbacks(this);
            _move.Dispose();
        }

        void InputSystem_Actions.IPlayerActions.OnMove(InputAction.CallbackContext context)
        {
            _move.OnNext(context);
        }

        public IInputObservable Move => _move;

        public void Enable()
        {
            _inputActions.Player.Enable();
        }

        public void Disable()
        {
            _inputActions.Player.Disable();
        }

        public void Tick()
        {
            _move.Tick();
        }
    }
}
