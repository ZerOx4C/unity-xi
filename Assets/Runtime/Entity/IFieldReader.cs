using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Entity
{
    public interface IFieldReader
    {
        IEnumerable<Dice> Dices { get; }
        int Width { get; }
        int Height { get; }

        IEnumerable<Vector2Int> GetEmptyPositions();
        bool IsValidPosition(Vector2Int position);
        bool TryGetDice(Vector2Int position, out Dice dice);
    }
}
