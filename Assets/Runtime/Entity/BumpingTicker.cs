using System;
using R3;
using UnityEngine;

namespace Runtime.Entity
{
    public class BumpingTicker : IDisposable
    {
        private readonly IMovableAgentReader _agent;
        private readonly ReactiveProperty<float> _durationX = new();
        private readonly ReactiveProperty<float> _durationY = new();
        private Vector2 _lastPosition;

        public BumpingTicker(IMovableAgentReader agent)
        {
            _agent = agent;
        }

        public Vector2Int DirectionX { get; private set; }
        public Vector2Int DirectionY { get; private set; }
        public ReadOnlyReactiveProperty<float> DurationX => _durationX;
        public ReadOnlyReactiveProperty<float> DurationY => _durationY;

        public void Dispose()
        {
            _durationX.Dispose();
            _durationY.Dispose();
        }

        public void Tick(float deltaTime)
        {
            var position = _agent.Position.CurrentValue;
            var velocity = _agent.Velocity.CurrentValue;

            if (Mathf.Approximately(position.x, _lastPosition.x) && velocity.x != 0)
            {
                DirectionX = velocity.x < 0 ? Vector2Int.left : Vector2Int.right;
                _durationX.Value += deltaTime;
            }
            else
            {
                _durationX.Value = 0;
            }

            if (Mathf.Approximately(position.y, _lastPosition.y) && velocity.y != 0)
            {
                DirectionY = velocity.y < 0 ? Vector2Int.down : Vector2Int.up;
                _durationY.Value += deltaTime;
            }
            else
            {
                _durationY.Value = 0;
            }

            _lastPosition = position;
        }
    }
}
