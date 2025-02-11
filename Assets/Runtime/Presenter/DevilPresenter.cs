using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using UnityEngine;
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
                .Subscribe(v =>
                {
                    var velocity = v.speed * v.direction.normalized;
                    behaviour.SetVelocity(new Vector3(-velocity.y, 0, velocity.x));
                })
                .AddTo(_disposables);
        }
    }
}
