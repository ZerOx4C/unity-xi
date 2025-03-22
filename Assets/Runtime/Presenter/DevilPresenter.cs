using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Utility;
using UnityEngine;
using VContainer;

namespace Runtime.Presenter
{
    public class DevilPresenter : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DevilPresenter(
            ITransformConverter transformConverter)
        {
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Bind(Devil devil, DevilBehaviour behaviour)
        {
            _disposables.Clear();

            devil.Position.CombineLatest(devil.Height, (p, h) => _transformConverter.ToDevilViewPosition(p, h))
                .Subscribe(v => behaviour.transform.position = v)
                .AddTo(_disposables);

            devil.Velocity
                .Select(_transformConverter.ToView)
                .Select(Vector3.Magnitude)
                .Subscribe(behaviour.SetSpeed)
                .AddTo(_disposables);

            devil.Forward
                .Select(_transformConverter.ToView)
                .Select(Quaternion.LookRotation)
                .Subscribe(v => behaviour.transform.rotation = v)
                .AddTo(_disposables);
        }
    }
}
