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

        public Field Field { get; } = new(10, 10);

        public void Dispose()
        {
            Player.Dispose();
        }

        public void Tick(float deltaTime)
        {
            Player.Tick(deltaTime);
        }
    }
}
