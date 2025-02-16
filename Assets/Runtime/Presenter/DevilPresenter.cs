using System;
using R3;
using R3.Triggers;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.UseCase;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class DevilPresenter : IDisposable
    {
        private const float PushingThreshold = 0.1f;
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
            devil.PushingTime
                .Where(v => PushingThreshold < v.x)
                .SubscribeAwait((_, token) => _dicePushing.PerformPushXAsync(devil, token))
                .AddTo(_disposables);

            devil.PushingTime
                .Where(v => PushingThreshold < v.y)
                .SubscribeAwait((_, token) => _dicePushing.PerformPushYAsync(devil, token))
                .AddTo(_disposables);

            behaviour.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    devil.Position.Value = _transformConverter.ToEntityPosition(behaviour.transform.position);
                    behaviour.SetVelocity(_transformConverter.ToView(devil.Velocity.CurrentValue));
                })
                .AddTo(_disposables);
        }
    }
}
