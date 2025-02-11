using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestSceneLifetimeScope : LifetimeScope
    {
        public DevilBehaviour devilPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterInstance(devilPrefab);

            builder.Register<Session>(Lifetime.Singleton);

            builder.Register<DevilPresenter>(Lifetime.Singleton);
            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();
            builder.Register<PlayerDevilController>(Lifetime.Singleton);

            builder.RegisterEntryPoint<TestEntryPoint>();
        }
    }
}
