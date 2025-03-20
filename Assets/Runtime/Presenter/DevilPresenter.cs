using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.UseCase;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class DevilPresenter : IDisposable
    {
        private const float BumpingThreshold = 0.1f;
        private readonly DicePushing _dicePushing;
        private readonly CompositeDisposable _disposables = new();
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DevilPresenter(
            DicePushing dicePushing,
            ITransformConverter transformConverter)
        {
            _dicePushing = dicePushing;
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Devil devil, DevilBehaviour behaviour)
        {
            _disposables.Clear();

            devil.Velocity
                .Select(_transformConverter.ToView)
                .Subscribe(behaviour.SetVelocity)
                .AddTo(_disposables);

            devil.Forward
                .Select(_transformConverter.ToView)
                .Subscribe(behaviour.SetDirection)
                .AddTo(_disposables);

            devil.BumpingTime
                .Where(v => BumpingThreshold < v.x)
                .SubscribeAwait((_, token) => _dicePushing.PerformPushXAsync(devil, token), AwaitOperation.Drop)
                .AddTo(_disposables);

            devil.BumpingTime
                .Where(v => BumpingThreshold < v.y)
                .SubscribeAwait((_, token) => _dicePushing.PerformPushYAsync(devil, token), AwaitOperation.Drop)
                .AddTo(_disposables);

            Observable.EveryValueChanged(behaviour.transform, t => t.position)
                .Skip(1)
                .Select(_transformConverter.ToEntityPosition)
                .Subscribe(devil.SetDesiredPosition)
                .AddTo(_disposables);

            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    var position = devil.Position.CurrentValue;
                    var height = devil.Height.CurrentValue;
                    behaviour.transform.position = _transformConverter.ToDevilViewPosition(position, height);
                })
                .AddTo(_disposables);
        }
    }
}
