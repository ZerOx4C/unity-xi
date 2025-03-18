using System;
using R3;
using VContainer;

namespace Runtime.Entity
{
    public class Session : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly Spawner _spawner;
        private readonly Vanisher _vanisher;

        [Inject]
        public Session()
        {
            Field = new Field(10, 10);

            Player = new Devil(Field);
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

            _spawner.OnSpawn
                .Subscribe(dice => Field.AddDice(dice))
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
    }
}
