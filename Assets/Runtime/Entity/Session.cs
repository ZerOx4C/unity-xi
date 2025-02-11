using VContainer;

namespace Runtime.Entity
{
    public class Session
    {
        [Inject]
        public Session()
        {
        }

        public Devil Player { get; } = new();

        public void Tick(float deltaTime)
        {
            Player.Tick(deltaTime);
        }
    }
}
