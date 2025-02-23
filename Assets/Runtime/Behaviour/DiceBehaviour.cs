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

        private Instantiator.Config<DiceTimelineBehaviour> _rollTimelineInstantiator;

        private void Awake()
        {
            _rollTimelineInstantiator = Instantiator.Create(rollTimelinePrefab).SetParent(transform);
        }

        public async UniTask PerformSlideAsync(Vector3 path, CancellationToken cancellation)
        {
            const float duration = 0.5f;
            var elapsed = 0f;
            var startPosition = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                var progress = Mathf.Clamp01(elapsed / duration);
                var factor = 1 - Mathf.Exp(-10 * progress);
                transform.position = startPosition + factor * path;

                await UniTask.DelayFrame(1, cancellationToken: cancellation);
            }
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
