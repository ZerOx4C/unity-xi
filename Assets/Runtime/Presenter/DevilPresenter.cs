using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using VContainer;

namespace Runtime.Presenter
{
    public class DevilPresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DevilPresenter(ITransformConverter transformConverter)
        {
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Devil devil, DevilBehaviour behaviour)
        {
            devil.Direction.CombineLatest(devil.Speed, (direction, speed) => (direction, speed))
                .Subscribe(v =>
                {
                    var velocity = v.speed * _transformConverter.ToView(v.direction).normalized;
                    behaviour.SetVelocity(velocity);
                })
                .AddTo(_disposables);
        }
    }
}
