using UnityEngine;

namespace Runtime.Utility
{
    public interface ITransformConverter
    {
        Vector2Int FromView(Vector3 viewPosition);
        Vector3 ToView(Vector2Int entityPosition);
    }
}
