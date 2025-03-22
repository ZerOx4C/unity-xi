using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class SessionPresenter : IDisposable
    {
        private readonly DevilBehaviour _devilBehaviourPrefab;
        private readonly DevilPresenter _devilPresenter;
        private readonly CompositeDisposable _disposables = new();
        private readonly FieldBehaviour _fieldBehaviourPrefab;
        private readonly FieldPresenter _fieldPresenter;
        private readonly PlayerDevilController _playerDevilController;
        private readonly TransformConverter _transformConverter;

        private DevilBehaviour _devilBehaviour;
        private FieldBehaviour _fieldBehaviour;

        [Inject]
        public SessionPresenter(
            DevilBehaviour devilBehaviourPrefab,
            DevilPresenter devilPresenter,
            FieldBehaviour fieldBehaviourPrefab,
            FieldPresenter fieldPresenter,
            PlayerDevilController playerDevilController,
            TransformConverter transformConverter)
        {
            _devilBehaviourPrefab = devilBehaviourPrefab;
            _devilPresenter = devilPresenter;
            _fieldBehaviourPrefab = fieldBehaviourPrefab;
            _fieldPresenter = fieldPresenter;
            _playerDevilController = playerDevilController;
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            var devilBehaviourTask = Instantiator.Create(_devilBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            var fieldBehaviourTask = Instantiator.Create(_fieldBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            _devilBehaviour = await devilBehaviourTask;
            _fieldBehaviour = await fieldBehaviourTask;
        }

        public void Bind(Session session)
        {
            _disposables.Clear();

            _transformConverter.SetFieldSize(session.Field.Width, session.Field.Height);

            _fieldPresenter.Bind(session.Field, _fieldBehaviour);
            _devilPresenter.Bind(session.Player, _devilBehaviour);
            _playerDevilController.Initialize(session.Player);

            _fieldPresenter.IsReady
                .Where(v => v)
                .Subscribe(_ => session.Start())
                .AddTo(_disposables);
        }
    }
}
