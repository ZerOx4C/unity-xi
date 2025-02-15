using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;

namespace Runtime.UseCase
{
    public class FloorInitialization
    {
        private readonly FloorBehaviour _floorBehaviourPrefab;
        private readonly Session _session;

        [Inject]
        public FloorInitialization(
            FloorBehaviour floorBehaviourPrefab,
            Session session)
        {
            _floorBehaviourPrefab = floorBehaviourPrefab;
            _session = session;
        }

        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            var floorBehaviour = await Instantiator.Create(_floorBehaviourPrefab)
                .InstantiateAsync(cancellation).First;

            var config = new FloorBehaviour.Config
            {
                CellSize = 1,
                FieldWidth = _session.Field.Width,
                FieldHeight = _session.Field.Height,
            };

            await floorBehaviour.SetupAsync(config, cancellation);
        }
    }
}
