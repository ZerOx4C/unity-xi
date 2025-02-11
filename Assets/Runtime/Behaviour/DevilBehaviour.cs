using UnityEngine;

namespace Runtime.Behaviour
{
    public class DevilBehaviour : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.linearVelocity = velocity;

            if (velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(velocity);
            }
        }
    }
}
