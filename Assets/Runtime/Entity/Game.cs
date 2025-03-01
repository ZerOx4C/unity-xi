using System;
using R3;
using VContainer;

namespace Runtime.Entity
{
    public class Game : IDisposable
    {
        private readonly ReactiveProperty<Session> _session = new();

        [Inject]
        public Game()
        {
            Reset();
        }

        public ReadOnlyReactiveProperty<Session> Session => _session;

        public void Dispose()
        {
            _session.Dispose();
        }

        public void Reset()
        {
            _session.Value = new Session();
        }

        public void Tick(float deltaTime)
        {
            if (!_session.Value.Started)
            {
                return;
            }

            _session.Value.Tick(deltaTime);
        }
    }
}
