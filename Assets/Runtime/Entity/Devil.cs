using System;
using R3;
using Runtime.DomainService;
using UnityEngine;

namespace Runtime.Entity
{
    public class Devil : ICellBoundedMovementOwner, IMovableAgentReader, IDisposable
    {
        private const float PushThreshold = 0.1f;
        private const float MovableHeightGap = 0.55f;
        private readonly BumpingTicker _bumpingTicker;
        private readonly CellBoundedMovement _cellBoundedMovement;
        private readonly DirectionalMovement _directionalMovement;
        private readonly CompositeDisposable _disposables = new();
        private readonly IFieldReader _fieldReader;
        private readonly ReactiveProperty<float> _height = new(0);

        public Devil(IDevilPushDiceService pushDiceService, IFieldReader fieldReader, Vector2 initialForward, Vector2 initialPosition)
        {
            _fieldReader = fieldReader;
            _bumpingTicker = new BumpingTicker(this);
            _cellBoundedMovement = new CellBoundedMovement(this, initialPosition);
            _directionalMovement = new DirectionalMovement(initialForward);

            _bumpingTicker.DurationX
                .Where(v => PushThreshold < v)
                .Where(v => !_fieldReader.TryGetDice(DiscretePosition.CurrentValue, out _))
                .SubscribeAwait(
                    (_, token) => pushDiceService.PushDiceAsync(this, _bumpingTicker.DirectionX, token),
                    AwaitOperation.Drop)
                .AddTo(_disposables);

            _bumpingTicker.DurationY
                .Where(v => PushThreshold < v)
                .Where(v => !_fieldReader.TryGetDice(DiscretePosition.CurrentValue, out _))
                .SubscribeAwait(
                    (_, token) => pushDiceService.PushDiceAsync(this, _bumpingTicker.DirectionY, token),
                    AwaitOperation.Drop)
                .AddTo(_disposables);
        }

        public float MaxDirectionSpeed
        {
            get => _directionalMovement.MaxDirectionSpeed;
            set => _directionalMovement.MaxDirectionSpeed = value;
        }

        public float MaxAcceleration
        {
            get => _directionalMovement.MaxAcceleration;
            set => _directionalMovement.MaxAcceleration = value;
        }

        public ReadOnlyReactiveProperty<Vector2Int> DiscretePosition => _cellBoundedMovement.DiscretePosition;
        public ReadOnlyReactiveProperty<Vector2> Forward => _directionalMovement.Forward;
        public ReadOnlyReactiveProperty<float> Height => _height;

        bool ICellBoundedMovementOwner.CanMove(Vector2Int from, Vector2Int to)
        {
            if (!_fieldReader.IsValidPosition(to))
            {
                return false;
            }

            var fromHeight = _fieldReader.GetHeight(from);
            var toHeight = _fieldReader.GetHeight(to);
            if (MovableHeightGap < Mathf.Abs(fromHeight - toHeight))
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _bumpingTicker.Dispose();
            _cellBoundedMovement.Dispose();
            _directionalMovement.Dispose();
            _height.Dispose();
            _disposables.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2> Position => _cellBoundedMovement.Position;
        public ReadOnlyReactiveProperty<Vector2> Velocity => _directionalMovement.Velocity;

        public void SetDesiredVelocity(Vector2 velocity)
        {
            _directionalMovement.SetDesiredVelocity(velocity);
        }

        public void Tick(float deltaTime)
        {
            _directionalMovement.Tick(deltaTime);

            var position = _cellBoundedMovement.Position.CurrentValue;
            var velocity = _directionalMovement.Velocity.CurrentValue;
            _cellBoundedMovement.Simulate(position + deltaTime * velocity);

            _height.Value = _fieldReader.GetHeight(DiscretePosition.CurrentValue);

            _bumpingTicker.Tick(deltaTime);
        }
    }
}
