using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Behaviour
{
    public class DiceBehaviour : MonoBehaviour
    {
        public async UniTask PerformPush(Vector3 path, CancellationToken cancellation)
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
    }
}
