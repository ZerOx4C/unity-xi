using System;
using VContainer;

namespace Runtime.Entity
{
    public class Session : IDisposable
    {
        [Inject]
        public Session()
        {
            Field = new Field(10, 10);

            Player = new Devil();
            Player.MaxDirectionSpeed = 720;
            Player.MaxAcceleration = 80;
        }

        public Field Field { get; }
        public Devil Player { get; }

        public void Dispose()
        {
            Field.Dispose();
            Player.Dispose();
        }

        public void Tick(float deltaTime)
        {
            foreach (var dice in Field.Dices)
            {
                dice.Tick(deltaTime);
            }

            Player.Tick(deltaTime);
        }
    }
}
