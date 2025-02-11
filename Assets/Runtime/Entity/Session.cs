using System;
using VContainer;

namespace Runtime.Entity
{
    public class Session : IDisposable
    {
        [Inject]
        public Session()
        {
        }

        public Devil Player { get; } = new();

        public Dice TestDice { get; } = new();

        public void Dispose()
        {
            Player.Dispose();
            TestDice.Dispose();
        }

        public void Tick(float deltaTime)
        {
            Player.Tick(deltaTime);
        }
    }
}
