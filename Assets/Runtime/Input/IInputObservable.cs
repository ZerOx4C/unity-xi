using R3;
using UnityEngine.InputSystem;

namespace Runtime.Input
{
    public interface IInputObservable
    {
        Observable<InputAction.CallbackContext> OnBegan { get; }
        Observable<InputAction.CallbackContext> OnEnded { get; }
        Observable<InputAction.CallbackContext> OnProgress { get; }
    }
}
