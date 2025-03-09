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
        [Range(0, 5)] public float floorSpeed = 1;
        [Range(0, 5)] public float animationSpeed = 1;
        [Range(0, 3)] public float globalSpeed = 1;

        [Header("設定状況")] public float actualSpeed;
        public float floorToAnimationSpeedRatio;

        private void Update()
        {
            if (modelAnimator)
            {
                modelAnimator.SetBool(ParamRunning, running);
                modelAnimator.SetFloat(ParamRunAnimationSpeed, animationSpeed * globalSpeed);
            }

            if (running)
            {
                var position = floorTransform.position;
                position.z = (position.z - Time.deltaTime * floorSpeed * globalSpeed) % 1;
                floorTransform.position = position;
            }

            actualSpeed = floorSpeed * globalSpeed;
            floorToAnimationSpeedRatio = animationSpeed / floorSpeed;
        }
    }
}
