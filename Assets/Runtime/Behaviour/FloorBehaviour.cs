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

        private BoxCollider _boxCollider;
        private Transform[] _cells = { };

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
        }

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

            var cellCount = config.FieldWidth * config.FieldHeight;
            var offset = -0.5f * config.CellSize *
                         new Vector3(config.FieldHeight - 1, 0, config.FieldWidth - 1);

            var positions = new List<Vector3>(cellCount);
            var fieldBounds = new RectInt(0, 0, config.FieldWidth, config.FieldHeight);
            foreach (var p in fieldBounds.allPositionsWithin)
            {
                positions.Add(config.CellSize * new Vector3(p.y, 0, p.x) + offset);
            }

            var rotations = new Quaternion[cellCount];
            Array.Fill(rotations, Quaternion.identity);

            _boxCollider.size = new Vector3(config.FieldWidth, 1, config.FieldHeight);
            _cells = await Instantiator.Create(cellPrefab)
                .SetParent(transform)
                .SetTransforms(positions, rotations)
                .InstantiateAsync(cancellation).All;
        }

        public class Config
        {
            public float CellSize;
            public int FieldHeight;
            public int FieldWidth;
        }
    }
}
