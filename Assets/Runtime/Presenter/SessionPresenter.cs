using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.UseCase;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class SessionPresenter
    {
        private readonly DevilBehaviour _devilBehaviourPrefab;
        private readonly DevilPresenter _devilPresenter;
        private readonly DicePushing _dicePushing;
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
            DicePushing dicePushing,
            FieldBehaviour fieldBehaviourPrefab,
            FieldPresenter fieldPresenter,
            PlayerDevilController playerDevilController,
            TransformConverter transformConverter)
        {
            _devilBehaviourPrefab = devilBehaviourPrefab;
            _devilPresenter = devilPresenter;
            _dicePushing = dicePushing;
            _fieldBehaviourPrefab = fieldBehaviourPrefab;
            _fieldPresenter = fieldPresenter;
            _playerDevilController = playerDevilController;
            _transformConverter = transformConverter;
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

        public async UniTask BindAsync(Session session, CancellationToken cancellation)
        {
            _dicePushing.SetField(session.Field);

            _transformConverter.SetFieldSize(session.Field.Width, session.Field.Height);

            var config = new FieldBehaviour.Config
            {
                CellSize = 1,
                FieldWidth = session.Field.Width,
                FieldHeight = session.Field.Height,
            };

            await _fieldBehaviour.SetupAsync(config, cancellation);
            _fieldPresenter.Initialize(session.Field);

            _devilPresenter.Bind(session.Player, _devilBehaviour);
            _playerDevilController.Initialize(session.Player);

            session.Start();
        }
    }
}
