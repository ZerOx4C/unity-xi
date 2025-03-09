using R3;
using Runtime.Behaviour;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Tester
{
    public class DiceRollingTester : MonoBehaviour
    {
        public DiceBehaviour diceBehaviour;

        private readonly TransformConverter _transformConverter = new();

        private Dice _dice;
        private PlayerInputSubject _playerInput;

        private void Awake()
        {
            _dice = new Dice().AddTo(this);
            _playerInput = new PlayerInputSubject().AddTo(this);
        }

        private void Start()
        {
            _playerInput.Move.OnBegan
                .Select(v => v.ReadValue<Vector2>())
                .Select(Vector2Int.RoundToInt)
                .Subscribe(_dice.Roll)
                .AddTo(this);

            _dice.FaceValues
                .Subscribe(v => Debug.Log($"(top, front, right) = {v}"))
                .AddTo(this);

            _dice.FaceValues
                .Subscribe(v => diceBehaviour.SetRotation(_transformConverter.ToDiceRotation(v.top, v.front)))
                .AddTo(this);

            _playerInput.Enable();
        }

        private void Update()
        {
            _playerInput.Tick();
        }
    }
}
