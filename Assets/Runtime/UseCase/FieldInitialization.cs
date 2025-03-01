using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;

namespace Runtime.UseCase
{
    public class FieldInitialization
    {
        private readonly FieldBehaviour _fieldBehaviourPrefab;
        private readonly Session _session;

        [Inject]
        public FieldInitialization(
            FieldBehaviour fieldBehaviourPrefab,
            Session session)
        {
            _fieldBehaviourPrefab = fieldBehaviourPrefab;
            _session = session;
        }

        public async UniTask PerformAsync(CancellationToken cancellation)
        {
            var fieldBehaviour = await Instantiator.Create(_fieldBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            var config = new FieldBehaviour.Config
            {
                CellSize = 1,
                FieldWidth = _session.Field.Width,
                FieldHeight = _session.Field.Height,
            };

            await fieldBehaviour.SetupAsync(config, cancellation);
        }
    }
}
