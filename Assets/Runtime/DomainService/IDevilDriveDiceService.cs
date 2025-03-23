using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Entity;
using UnityEngine;

namespace Runtime.DomainService
{
    public interface IDevilDriveDiceService
    {
        UniTask DriveDiceAsync(Devil devil, Vector2Int direction, CancellationToken cancellation);
    }
}
