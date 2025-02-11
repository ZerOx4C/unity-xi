using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using Runtime.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestEntryPoint : IAsyncStartable, ITickable
    {
        private readonly DevilBehaviour _devilBehaviourPrefab;
        private readonly DevilPresenter _devilPresenter;
        private readonly PlayerDevilController _playerDevilController;
        private readonly PlayerInputSubject _playerInput;
        private readonly Session _session;

        private bool _readyToMove;

        [Inject]
        public TestEntryPoint(
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

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var playerDevilBehaviour = await Instantiator.Create(_devilBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            _devilPresenter.Bind(_session.Player, playerDevilBehaviour);
            _playerDevilController.Initialize(_session.Player);

            _playerInput.Enable();
            _readyToMove = true;
        }

        public void Tick()
        {
            if (!_readyToMove)
            {
                return;
            }

            _playerInput.Tick();
            _session.Tick(Time.deltaTime);
        }
    }
}
