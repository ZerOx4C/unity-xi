using System;
using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class DiceTestEntryPoint : IStartable, ITickable, IDisposable
    {
        private readonly Dice _dice = new();
        private readonly DiceBehaviour _diceBehaviourPrefab;
        private readonly CompositeDisposable _disposables = new();
        private readonly PlayerInputSubject _playerInput;
        private readonly ITransformConverter _transformConverter;

        [Inject]
        public DiceTestEntryPoint(
            DiceBehaviour diceBehaviourPrefab,
            PlayerInputSubject playerInput,
            ITransformConverter transformConverter)
        {
            _diceBehaviourPrefab = diceBehaviourPrefab;
            _playerInput = playerInput;
            _transformConverter = transformConverter;
        }

        public void Dispose()
        {
            _dice.Dispose();
        }

        public void Start()
        {
            var diceBehaviour = Instantiator.Create(_diceBehaviourPrefab).Instantiate().First;

            _playerInput.Move.OnBegan
                .Select(v => v.ReadValue<Vector2>())
                .Select(Vector2Int.RoundToInt)
                .Subscribe(_dice.Roll)
                .AddTo(_disposables);

            _dice.FaceValues
                .Subscribe(v => Debug.Log($"(top, front, right) = {v}"))
                .AddTo(_disposables);

            _dice.FaceValues
                .Subscribe(v => diceBehaviour.SetRotation(_transformConverter.ToDiceRotation(v.top, v.front)))
                .AddTo(_disposables);

            _playerInput.Enable();
        }

        public void Tick()
        {
            _playerInput.Tick();
        }
    }
}
