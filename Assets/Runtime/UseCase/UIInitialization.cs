using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Presenter;
using Runtime.Utility;
using VContainer;

namespace Runtime.UseCase
{
    public class UIInitialization
    {
        private readonly DebugUIPresenter _debugUIPresenter;
        private readonly Instantiator.Config<UIBehaviour> _uiBehaviourInstantiator;

        [Inject]
        public UIInitialization(
            DebugUIPresenter debugUIPresenter,
            UIBehaviour uiBehaviourPrefab)
        {
            _debugUIPresenter = debugUIPresenter;
            _uiBehaviourInstantiator = Instantiator.Create(uiBehaviourPrefab);
        }

        public async UniTask PerformAsync(CancellationToken cancellation)
        {
            var ui = await _uiBehaviourInstantiator.InstantiateAsync(cancellation).First;

            _debugUIPresenter.Initialize(ui.debugUI);
        }
    }
}
