using Runtime.Behaviour;
using Runtime.Input;
using Runtime.Utility;
using VContainer;
using VContainer.Unity;

namespace Runtime.Tester
{
    public class DiceTesterLifetimeScope : LifetimeScope
    {
        public DiceBehaviour dicePrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            // Behaviour
            builder.RegisterInstance(dicePrefab);

            // Input
            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();

            // Utility
            builder.Register<ITransformConverter, TransformConverter>(Lifetime.Singleton);

            // EntryPoint
            builder.RegisterEntryPoint<DiceTesterEntryPoint>();
        }
    }
}
