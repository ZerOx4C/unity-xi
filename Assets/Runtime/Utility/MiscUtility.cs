using UnityEngine;

namespace Runtime.Utility
{
    public static class MiscUtility
    {
        public static Vector3 Convert(Vector2 direction)
        {
            return new Vector3(-direction.y, 0, direction.x);
        }
    }
}
