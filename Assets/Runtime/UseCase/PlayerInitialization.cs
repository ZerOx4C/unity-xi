using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using Runtime.Utility;
using VContainer;

namespace Runtime.UseCase
{
    public class PlayerInitialization
    {
        private readonly DevilBehaviour _devilBehaviourPrefab;
        private readonly DevilPresenter _devilPresenter;
        private readonly PlayerDevilController _playerDevilController;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;

        [Inject]
        public PlayerInitialization(
            DevilBehaviour devilBehaviourPrefab,
            DevilPresenter devilPresenter,
            PlayerDevilController playerDevilController,
            PlayerInputSubject playerInput,
            Session session)
        {
            _devilBehaviourPrefab = devilBehaviourPrefab;
            _devilPresenter = devilPresenter;
            _playerDevilController = playerDevilController;
            _playerInput = playerInput;
            _session = session;
        }

        public async UniTask PerformAsync(CancellationToken cancellation)
        {
            var devilBehaviour = await Instantiator.Create(_devilBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            _devilPresenter.Bind(_session.Player, devilBehaviour);
            _playerDevilController.Initialize(_session.Player);

            _playerInput.Enable();
        }
    }
}
