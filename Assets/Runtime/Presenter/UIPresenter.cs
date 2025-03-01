using Runtime.Behaviour;
using VContainer;

namespace Runtime.Presenter
{
    public class UIPresenter
    {
        private readonly DebugUIPresenter _debugUIPresenter;

        [Inject]
        public UIPresenter(DebugUIPresenter debugUIPresenter)
        {
            _debugUIPresenter = debugUIPresenter;
        }

        public void Initialize(UIBehaviour behaviour)
        {
            _debugUIPresenter.Initialize(behaviour.debugUI);
        }
    }
}
