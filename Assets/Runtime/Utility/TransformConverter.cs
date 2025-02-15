using UnityEngine;
using VContainer;

namespace Runtime.Utility
{
    public class TransformConverter : ITransformConverter
    {
        private int _fieldHeight;
        private int _fieldWidth;

        [Inject]
        public TransformConverter()
        {
        }

        public Vector2Int FromView(Vector3 viewPosition)
        {
            return new Vector2Int(
                (int)(viewPosition.z + (_fieldWidth - 1) * 0.5f),
                (int)(-viewPosition.x - (_fieldHeight - 1) * 0.5f));
        }

        public Vector3 ToView(Vector2Int entityPosition)
        {
            return new Vector3(
                -entityPosition.y + (_fieldHeight - 1) * 0.5f,
                0,
                entityPosition.x - (_fieldWidth - 1) * 0.5f);
        }

        public void SetFieldSize(int width, int height)
        {
            _fieldWidth = width;
            _fieldHeight = height;
        }
    }
}
