using System;
using System.Collections.Generic;
using R3;
using Runtime.DomainService;
using UnityEngine;
using VContainer;

namespace Runtime.Entity
{
    public class Session : IDisposable
    {
        private readonly Dictionary<Dice, IDisposable> _diceDisposableTable = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly Spawner _spawner;
        private readonly Vanisher _vanisher;

        [Inject]
        public Session()
        {
            Field = new Field(9, 9);

            var devilPushDiceService = new DevilPushDiceService(Field);
            var devilDriveDiceService = new DevilDriveDiceService(Field);
            Player = new Devil(devilPushDiceService, devilDriveDiceService, Field, Vector2.down, new Vector2(4, 4));
            Player.MaxDirectionSpeed = 720;
            Player.MaxAcceleration = 80;

            _spawner = new Spawner(Field);
            _vanisher = new Vanisher(Field);
        }

        public Field Field { get; }
        public Devil Player { get; }
        public bool Started { get; private set; }

        public void Dispose()
        {
            _disposables.Dispose();
            Field.Dispose();
            Player.Dispose();
        }

        public void Start()
        {
            if (Started)
            {
                throw new InvalidOperationException("Already started.");
            }

            Field.OnDiceMove
                .Subscribe(_vanisher.Evaluate)
                .AddTo(_disposables);

            Field.OnDiceRemove
                .Subscribe(dice =>
                {
                    _diceDisposableTable.Remove(dice, out var disposable);
                    disposable.Dispose();
                    dice.Dispose();
                })
                .AddTo(_disposables);

            _spawner.OnSpawn
                .Subscribe(OnSpawn)
                .AddTo(_disposables);

            _spawner.FillRandomly(0.2f);

            Started = true;
        }

        public void Tick(float deltaTime)
        {
            if (!Started)
            {
                throw new InvalidOperationException("Not started.");
            }

            _spawner.Tick(deltaTime);

            foreach (var dice in Field.Dices)
            {
                dice.Tick(deltaTime);
            }

            Player.Tick(deltaTime);
        }

        private void OnSpawn(Vector2Int position)
        {
            var dice = new Dice();
            dice.Randomize();

            var disposables = new CompositeDisposable();
            _diceDisposableTable.Add(dice, disposables);

            dice.State
                .Where(v => v == DiceState.Vanished)
                .Subscribe(_ => Field.RemoveDice(dice))
                .AddTo(disposables);

            Field.AddDice(dice, position);
        }
    }
}
