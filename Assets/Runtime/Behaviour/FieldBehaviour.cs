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

        public void BeginSetup(float cellSize, int width, int height)
        {
            _isReady.Value = false;

            UniTask.Void(async token =>
            {
                Clear();
                SetupColliders(cellSize * width, cellSize * height);
                await SetupFloorCells(cellSize, width, height, token);
                _isReady.Value = true;
            }, ObtainCancellation());
        }

        private async UniTask SetupFloorCells(float cellSize, int width, int height, CancellationToken cancellation)
        {
            var cellCount = width * height;
            var offset = -0.5f * cellSize * new Vector3(width - 1, 0, height - 1);

            var positions = new List<Vector3>(cellCount);
            var fieldBounds = new RectInt(0, 0, width, height);
            foreach (var p in fieldBounds.allPositionsWithin)
            {
                positions.Add(cellSize * new Vector3(p.x, 0, p.y) + offset);
            }

            var rotations = new Quaternion[cellCount];
            Array.Fill(rotations, Quaternion.identity);

            _cells = await Instantiator.Create(cellPrefab)
                .SetParent(transform)
                .SetTransforms(positions, rotations)
                .InstantiateAsync(cancellation).All;
        }

        private void SetupColliders(float width, float height)
        {
            wallPositiveX.position = new Vector3(width / 2, 0, 0);
            wallNegativeX.position = new Vector3(-width / 2, 0, 0);
            wallPositiveZ.position = new Vector3(0, 0, height / 2);
            wallNegativeZ.position = new Vector3(0, 0, -height / 2);

            wallPositiveX.localScale = new Vector3(1, 1, height);
            wallNegativeX.localScale = new Vector3(1, 1, height);
            wallPositiveZ.localScale = new Vector3(width, 1, 1);
            wallNegativeZ.localScale = new Vector3(width, 1, 1);

            floor.localScale = new Vector3(width, 1, height);
        }

        private CancellationToken ObtainCancellation()
        {
            _cancellationDisposable?.Dispose();
            _cancellationDisposable = new CancellationDisposable();

            return CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationDisposable.Token,
                destroyCancellationToken).Token;
        }
    }
}
