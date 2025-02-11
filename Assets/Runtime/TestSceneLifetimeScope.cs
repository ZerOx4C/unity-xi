using Runtime.Input;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();

            builder.RegisterEntryPoint<TestEntryPoint>();
        }
    }
}
