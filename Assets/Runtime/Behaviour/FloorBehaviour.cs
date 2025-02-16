using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Behaviour
{
    public class FloorBehaviour : MonoBehaviour
    {
        public Transform cellPrefab;
        public Transform wallPositiveX;
        public Transform wallNegativeX;
        public Transform wallPositiveZ;
        public Transform wallNegativeZ;
        public Transform floor;

        private Transform[] _cells = { };

        public void Clear()
        {
            foreach (var cell in _cells)
            {
                Destroy(cell.gameObject);
            }

            _cells = Array.Empty<Transform>();
        }

        public async UniTask SetupAsync(Config config, CancellationToken cancellation)
        {
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

            _cells = await Instantiator.Create(cellPrefab)
                .SetParent(transform)
                .SetTransforms(positions, rotations)
                .InstantiateAsync(cancellation).All;
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

        public class Config
        {
            public float CellSize;
            public int FieldHeight;
            public int FieldWidth;
        }
    }
}
