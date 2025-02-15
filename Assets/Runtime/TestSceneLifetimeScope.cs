using Runtime.Behaviour;
using Runtime.Controller;
using Runtime.Entity;
using Runtime.Input;
using Runtime.Presenter;
using Runtime.UseCase;
using Runtime.Utility;
using VContainer;
using VContainer.Unity;

namespace Runtime
{
    public class TestSceneLifetimeScope : LifetimeScope
    {
        public DevilBehaviour devilPrefab;
        public DiceBehaviour dicePrefab;
        public FloorBehaviour floorPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterInstance(devilPrefab);
            builder.RegisterInstance(dicePrefab);
            builder.RegisterInstance(floorPrefab);

            builder.Register<Session>(Lifetime.Singleton);

            builder.Register<FloorInitialization>(Lifetime.Singleton);
            builder.Register<PlayerInitialization>(Lifetime.Singleton);
            builder.Register<TransformConverter>(Lifetime.Singleton)
                .As<ITransformConverter>()
                .AsSelf();
            builder.Register<DevilPresenter>(Lifetime.Singleton);
            builder.Register<DicePresenter>(Lifetime.Singleton);
            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();
            builder.Register<PlayerDevilController>(Lifetime.Singleton);

            builder.RegisterEntryPoint<TestEntryPoint>();
        }
    }
}
