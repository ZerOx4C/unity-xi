using UnityEngine;

namespace Runtime.Utility
{
    public static class MiscUtility
    {
        public static Vector3 Convert(Vector2 direction)
        {
            return new Vector3(direction.x, 0, direction.y);
        }
    }
}
