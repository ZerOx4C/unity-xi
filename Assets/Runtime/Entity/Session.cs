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
    }
}
