using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public interface IMovableAgentReader
    {
        ReadOnlyReactiveProperty<Vector2> Position { get; }
        ReadOnlyReactiveProperty<Vector2> Velocity { get; }
    }
}
