using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Runtime.Utility
{
    public static class Instantiator
    {
        public static Config<T> Create<T>(T prefab) where T : Object
        {
            return new Config<T>(prefab);
        }

        public readonly struct Result<T> where T : Object
        {
            public Result(T[] instances)
            {
                All = instances;
                First = All[0];
            }

            public T[] All { get; }
            public T First { get; }
        }

        public readonly struct AsyncResult<T> where T : Object
        {
            public AsyncResult(UniTask<T[]> task)
            {
                var lazyTask = task.ToAsyncLazy();
                All = UniTask.Create(async () => await lazyTask);
                First = UniTask.Create(async () => (await lazyTask)[0]);
            }

            public UniTask<T[]> All { get; }
            public UniTask<T> First { get; }
        }

        public class Config<T> where T : Object
        {
            private readonly T _prefab;
            private int _count;
            private Transform _parent;
            private Vector3[] _positions;
            private Quaternion[] _rotations;

            public Config(T prefab)
            {
                _prefab = prefab;
                SetTransforms(1, Vector3.zero, Quaternion.identity);
            }

            public Config<T> SetParent(Transform parent)
            {
                _parent = parent;
                return this;
            }

            public Config<T> SetTransforms(Vector3 position, Quaternion rotation)
            {
                return SetTransforms(1, position, rotation);
            }

            public Config<T> SetTransforms(Transform transform)
            {
                return SetTransforms(1, transform);
            }

            public Config<T> SetTransforms(int count, Vector3 position, Quaternion rotation)
            {
                _count = count;
                _positions = new[] { position };
                _rotations = new[] { rotation };
                return this;
            }

            public Config<T> SetTransforms(int count, Transform transform)
            {
                transform.GetPositionAndRotation(out var position, out var rotation);
                return SetTransforms(count, position, rotation);
            }

            public Config<T> SetTransforms(IReadOnlyList<Vector3> positions, IReadOnlyList<Quaternion> rotations)
            {
                Assert.AreEqual(positions.Count, rotations.Count);
                _count = positions.Count;
                _positions = positions.ToArray();
                _rotations = rotations.ToArray();
                return this;
            }

            public Config<T> SetTransforms(IReadOnlyList<Transform> transforms)
            {
                _count = transforms.Count;
                _positions = new Vector3[transforms.Count];
                _rotations = new Quaternion[transforms.Count];

                for (var i = 0; i < transforms.Count; i++)
                {
                    transforms[i].GetPositionAndRotation(out _positions[i], out _rotations[i]);
                }

                return this;
            }

            public Result<T> Instantiate()
            {
                var instances = new T[_count];

                if (1 < _positions.Length)
                {
                    for (var i = 0; i < _count; i++)
                    {
                        instances[i] = Object.Instantiate(_prefab, _positions[i], _rotations[i], _parent);
                    }
                }
                else
                {
                    for (var i = 0; i < _count; i++)
                    {
                        instances[i] = Object.Instantiate(_prefab, _positions[0], _rotations[0], _parent);
                    }
                }

                return new Result<T>(instances);
            }

            public AsyncResult<T> InstantiateAsync(CancellationToken cancellation)
            {
                if (1 < _positions.Length)
                {
                    var task = Object.InstantiateAsync(_prefab, _count, _parent, _positions, _rotations, cancellation)
                        .ToUniTask(cancellationToken: cancellation);

                    return new AsyncResult<T>(task);
                }
                else
                {
                    var task = Object.InstantiateAsync(_prefab, _count, _parent, _positions[0], _rotations[0], cancellation)
                        .ToUniTask(cancellationToken: cancellation);

                    return new AsyncResult<T>(task);
                }
            }
        }
    }
}
