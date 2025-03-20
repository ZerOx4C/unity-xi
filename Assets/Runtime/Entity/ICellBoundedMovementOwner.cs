using UnityEngine;

namespace Runtime.Entity
{
    public interface ICellBoundedMovementOwner
    {
        bool CanMove(Vector2Int from, Vector2Int to);
        // TODO: 後で衝突通知もできるようにする
    }
}
