using UnityEngine;

namespace Runtime.Behaviour
{
    public class DevilBehaviour : MonoBehaviour
    {
        // TODO: モデル設定的なものとしてモデルに持たせる
        private const float RunAnimationSpeedFactor = 0.36f;
        private static readonly int ParamRunning = Animator.StringToHash("Running");
        private static readonly int ParamRunAnimationSpeed = Animator.StringToHash("RunAnimationSpeed");

        public Animator animator;

        public void SetSpeed(float speed)
        {
            animator.SetBool(ParamRunning, speed != 0);
            animator.SetFloat(ParamRunAnimationSpeed, speed * RunAnimationSpeedFactor);
        }
    }
}
