using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Behaviour
{
    public class DiceBehaviour : MonoBehaviour
    {
        public Transform cube;
        public DiceTimelineBehaviour rollTimelinePrefab;
        public DiceTimelineBehaviour slideTimelinePrefab;

        private Instantiator.Config<DiceTimelineBehaviour> _rollTimelineInstantiator;
        private Instantiator.Config<DiceTimelineBehaviour> _slideTimelineInstantiator;

        private void Awake()
        {
            _rollTimelineInstantiator = Instantiator.Create(rollTimelinePrefab).SetParent(transform);
            _slideTimelineInstantiator = Instantiator.Create(slideTimelinePrefab).SetParent(transform);
        }

        public async UniTask PerformSlideAsync(Vector3 path, CancellationToken cancellation)
        {
            cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation, destroyCancellationToken).Token;

            var timeline = await _slideTimelineInstantiator
                .SetTransforms(1, transform.position, Quaternion.LookRotation(path, Vector3.up))
                .InstantiateAsync(cancellation).First;

            await timeline.PlayAsync(cube, cancellation);
            Destroy(timeline.gameObject);
        }

        public async UniTask PerformRollAsync(Vector3 path, CancellationToken cancellation)
        {
            cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation, destroyCancellationToken).Token;

            var timeline = await _rollTimelineInstantiator
                .SetTransforms(1, transform.position, Quaternion.LookRotation(path, Vector3.up))
                .InstantiateAsync(cancellation).First;

            await timeline.PlayAsync(cube, cancellation);
            Destroy(timeline.gameObject);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            cube.localRotation = rotation;
        }
    }
}
