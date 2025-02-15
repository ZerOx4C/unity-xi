using UnityEngine;
using VContainer;

namespace Runtime.Utility
{
    public class TransformConverter : ITransformConverter
    {
        private Vector2 _toEntityOffset;
        private Vector3 _toViewOffset;

        [Inject]
        public TransformConverter()
        {
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

        public void SetFieldSize(int width, int height)
        {
            _toViewOffset = -0.5f * new Vector3(width - 1, 0, height - 1);
            _toEntityOffset = 0.5f * new Vector2(width - 1, height - 1);
        }
    }
}
