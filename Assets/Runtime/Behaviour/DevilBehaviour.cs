using UnityEngine;

namespace Runtime.Behaviour
{
    public class DevilBehaviour : MonoBehaviour
    {
        private static readonly int ParamRunning = Animator.StringToHash("Running");
        private static readonly int ParamRunAnimationSpeed = Animator.StringToHash("RunAnimationSpeed");

        // TODO: モデル設定的なものとしてモデルに持たせる
        private const float RunAnimationSpeedFactor = 0.36f;

        public Animator animator;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.linearVelocity = velocity;

            animator.SetBool(ParamRunning, velocity != Vector3.zero);
            animator.SetFloat(ParamRunAnimationSpeed, velocity.magnitude * RunAnimationSpeedFactor);
        }

        public void SetDirection(Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
