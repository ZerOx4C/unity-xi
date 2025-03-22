using Runtime.Entity;
using UnityEngine;

namespace Runtime.DomainService
{
    public interface IDevilPushDiceService
    {
        void PushDice(Devil devil, Vector2Int direction);
    }
}
