using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Behaviour
{
    public class FieldBehaviour : MonoBehaviour
    {
        public Transform cellPrefab;
        public Transform wallPositiveX;
        public Transform wallNegativeX;
        public Transform wallPositiveZ;
        public Transform wallNegativeZ;
        public Transform floor;

        private CancellationDisposable _cancellationDisposable;
        private Transform[] _cells = { };
        private ReactiveProperty<bool> _isReady;

        public ReadOnlyReactiveProperty<bool> IsReady => _isReady;

        private void Awake()
        {
            _isReady = new ReactiveProperty<bool>(false);
            _isReady.AddTo(this);
        }

        public void Clear()
        {
            foreach (var cell in _cells)
            {
                Destroy(cell.gameObject);
            }

            _cells = Array.Empty<Transform>();
        }

        public void BeginSetup(Config config)
        {
            _isReady.Value = false;

            Clear();

            var viewWidth = config.CellSize * config.FieldWidth;
            var viewHeight = config.CellSize * config.FieldHeight;
            SetupColliderSize(viewWidth, viewHeight);

            var cellCount = config.FieldWidth * config.FieldHeight;
            var offset = -0.5f * config.CellSize *
                         new Vector3(config.FieldWidth - 1, 0, config.FieldHeight - 1);

            var positions = new List<Vector3>(cellCount);
            var fieldBounds = new RectInt(0, 0, config.FieldWidth, config.FieldHeight);
            foreach (var p in fieldBounds.allPositionsWithin)
            {
                positions.Add(config.CellSize * new Vector3(p.x, 0, p.y) + offset);
            }

            var rotations = new Quaternion[cellCount];
            Array.Fill(rotations, Quaternion.identity);


            UniTask.Void(async token =>
            {
                _cells = await Instantiator.Create(cellPrefab)
                    .SetParent(transform)
                    .SetTransforms(positions, rotations)
                    .InstantiateAsync(token).All;

                _isReady.Value = true;
            }, ObtainCancellation());
        }

        private void SetupColliderSize(float xLength, float zLength)
        {
            wallPositiveX.position = new Vector3(xLength / 2, 0, 0);
            wallNegativeX.position = new Vector3(-xLength / 2, 0, 0);
            wallPositiveZ.position = new Vector3(0, 0, zLength / 2);
            wallNegativeZ.position = new Vector3(0, 0, -zLength / 2);

            wallPositiveX.localScale = new Vector3(1, 1, zLength);
            wallNegativeX.localScale = new Vector3(1, 1, zLength);
            wallPositiveZ.localScale = new Vector3(xLength, 1, 1);
            wallNegativeZ.localScale = new Vector3(xLength, 1, 1);

            floor.localScale = new Vector3(xLength, 1, zLength);
        }

        private CancellationToken ObtainCancellation()
        {
            _cancellationDisposable?.Dispose();
            _cancellationDisposable = new CancellationDisposable();

            return CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationDisposable.Token,
                destroyCancellationToken).Token;
        }

        public class Config
        {
            public float CellSize;
            public int FieldHeight;
            public int FieldWidth;
        }
    }
}
