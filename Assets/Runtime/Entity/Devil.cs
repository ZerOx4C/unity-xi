using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : IDisposable
    {
        public ReactiveProperty<Vector2> MoveDirection { get; } = new();
        public ReactiveProperty<float> MoveSpeed { get; } = new();

        public void Dispose()
        {
            MoveDirection.Dispose();
            MoveSpeed.Dispose();
        }
    }
}
