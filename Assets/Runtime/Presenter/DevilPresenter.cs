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

        [Inject]
        public DevilPresenter()
        {
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Devil devil, DevilBehaviour behaviour)
        {
            devil.Direction.CombineLatest(devil.Speed, (direction, speed) => (direction, speed))
                .Subscribe(v => behaviour.SetVelocity(MiscUtility.Convert(v.speed * v.direction.normalized)))
                .AddTo(_disposables);
        }
    }
}
