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
    }
}
