using UnityEngine;

namespace Runtime.Tester
{
    public class ModelTester : MonoBehaviour
    {
        private static readonly int ParamRunning = Animator.StringToHash("Running");
        private static readonly int ParamRunAnimationSpeed = Animator.StringToHash("RunAnimationSpeed");

        public Animator modelAnimator;
        public Transform floorTransform;
        public bool running;
        [Range(0, 10)] public float floorSpeed = 1;
        [Range(0, 10)] public float animationSpeed = 1;

        private void Update()
        {
            if (modelAnimator)
            {
                modelAnimator.SetBool(ParamRunning, running);
                modelAnimator.SetFloat(ParamRunAnimationSpeed, animationSpeed);
            }

            if (running)
            {
                var position = floorTransform.position;
                position.z = (position.z - Time.deltaTime * floorSpeed) % 1;
                floorTransform.position = position;
            }
        }
    }
}
