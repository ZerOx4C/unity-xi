using System;
using VContainer;

namespace Runtime.Entity
{
    public class Session : IDisposable
    {
        private readonly Spawner _spawner;

        [Inject]
        public Session()
        {
            Field = new Field(10, 10);

            Player = new Devil();
            Player.MaxDirectionSpeed = 720;
            Player.MaxAcceleration = 80;

            _spawner = new Spawner(Field);
        }

        public Field Field { get; }
        public Devil Player { get; }
        public bool Started { get; private set; }

        public void Dispose()
        {
            Field.Dispose();
            Player.Dispose();
        }

        public void Start()
        {
            if (Started)
            {
                throw new InvalidOperationException("Already started.");
            }

            _spawner.SpawnInitialDices(0.2f);

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
