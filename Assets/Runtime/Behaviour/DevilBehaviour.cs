using UnityEngine;

namespace Runtime.Behaviour
{
    public class DevilBehaviour : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");

        public Animator animator;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.linearVelocity = velocity;

            animator.SetFloat(SpeedParameter, velocity.magnitude);
        }

        public void SetDirection(Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
