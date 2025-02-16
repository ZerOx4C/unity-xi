using UnityEngine;
using UnityEngine.Assertions;

namespace Runtime.Utility
{
    public static class AssertUtility
    {
        public static void IsValidDirection(Vector2Int direction)
        {
            Assert.IsTrue(direction == Vector2Int.left ||
                          direction == Vector2Int.right ||
                          direction == Vector2Int.up ||
                          direction == Vector2Int.down);
        }
    }
}
