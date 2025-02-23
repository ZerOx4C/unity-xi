using UnityEngine;

namespace Runtime.Utility
{
    public interface ITransformConverter
    {
        Vector2 ToEntityPosition(Vector3 viewPosition);
        Vector3 ToViewPosition(Vector2 entityPosition);
        Vector3 ToView(Vector2 entityDirection);
        Vector2 ToEntity(Vector3 viewDirection);
        Quaternion ToDiceRotation(int top, int front);
    }
}
