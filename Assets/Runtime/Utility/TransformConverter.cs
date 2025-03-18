using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Runtime.Utility
{
    public class TransformConverter : ITransformConverter
    {
        private readonly IReadOnlyDictionary<int, Vector3> _diceValueVectorTable;

        private Vector2 _toEntityOffset;
        private Vector3 _toViewOffset;

        [Inject]
        public TransformConverter()
        {
            var dictionary = new Dictionary<int, Vector3>
            {
                { 1, Vector3.up },
                { 3, Vector3.back },
                { 5, Vector3.right },
                { 6, Vector3.down },
                { 4, Vector3.forward },
                { 2, Vector3.left },
            };
            _diceValueVectorTable = dictionary;
        }

        public Vector3 ToViewPosition(Vector2 entityPosition)
        {
            return ToView(entityPosition) + _toViewOffset;
        }

        public Vector2 ToEntityPosition(Vector3 viewPosition)
        {
            return ToEntity(viewPosition) + _toEntityOffset;
        }

        public Vector3 ToView(Vector2 entityDirection)
        {
            return new Vector3(entityDirection.x, 0, entityDirection.y);
        }

        public Vector2 ToEntity(Vector3 viewDirection)
        {
            return new Vector2(viewDirection.x, viewDirection.z);
        }

        public Quaternion ToDiceRotation(int top, int front)
        {
            var initialTopVector = _diceValueVectorTable[top];
            var initialFrontVector = _diceValueVectorTable[front];
            var adjustTopRotation = Quaternion.FromToRotation(initialTopVector, Vector3.up);
            var adjustFrontAngle = Vector3.SignedAngle(adjustTopRotation * initialFrontVector, Vector3.back, Vector3.up);
            var adjustFrontRotation = Quaternion.AngleAxis(adjustFrontAngle, Vector3.up);
            return adjustFrontRotation * adjustTopRotation;
        }

        public Vector3 ToDevilViewPosition(Vector2 entityPosition, float height)
        {
            return ToViewPosition(entityPosition) + height * Vector3.up;
        }

        public void SetFieldSize(int width, int height)
        {
            _toViewOffset = -0.5f * new Vector3(width - 1, 0, height - 1);
            _toEntityOffset = 0.5f * new Vector2(width - 1, height - 1);
        }
    }
}
