using Runtime.Behaviour;
using Runtime.Entity;
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

        public void Bind(Game game, UIBehaviour behaviour)
        {
            _debugUIPresenter.Bind(game, behaviour.debugUI);
        }
    }
}
