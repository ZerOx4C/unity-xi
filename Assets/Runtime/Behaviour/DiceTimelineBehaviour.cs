using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Playables;

namespace Runtime.Behaviour
{
    public class DiceTimelineBehaviour : MonoBehaviour
    {
        public Transform diceAnchor;

        private PlayableDirector _director;
        private bool _stopped;

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            _director.stopped += OnDirectorStopped;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!diceAnchor)
            {
                return;
            }

            Gizmos.color = new Color(1, 1, 1, 0.5f);
            Gizmos.matrix = diceAnchor.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
#endif

        public async UniTask PlayAsync(Transform diceBody, CancellationToken cancellation)
        {
            cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation, destroyCancellationToken).Token;

            diceBody.GetPositionAndRotation(out var initialPosition, out var initialRotation);
            diceAnchor.rotation = initialRotation;
            _stopped = false;

            var subscriber = Observable.EveryUpdate(cancellation)
                .Subscribe(_ =>
                {
                    diceAnchor.GetPositionAndRotation(out var position, out var rotation);
                    diceBody.SetPositionAndRotation(position, rotation);
                })
                .RegisterTo(cancellation);

            _director.Play();

            await UniTask.WaitUntil(() => _stopped, cancellationToken: cancellation);
            await subscriber.DisposeAsync().AsUniTask();

            diceBody.SetPositionAndRotation(initialPosition, initialRotation);
        }

        private void OnDirectorStopped(PlayableDirector sender)
        {
            _stopped = true;
        }
    }
}
