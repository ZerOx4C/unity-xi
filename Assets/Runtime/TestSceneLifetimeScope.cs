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

            // Behaviour
            builder.RegisterInstance(devilPrefab);
            builder.RegisterInstance(dicePrefab);
            builder.RegisterInstance(floorPrefab);

            // Controller
            builder.Register<PlayerDevilController>(Lifetime.Singleton);

            // Entity
            builder.Register<Session>(Lifetime.Singleton);

            // Input
            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();

            // Presenter
            builder.Register<DevilPresenter>(Lifetime.Singleton);
            builder.Register<DicePresenter>(Lifetime.Singleton);

            // UseCase
            builder.Register<FloorInitialization>(Lifetime.Singleton);
            builder.Register<PlayerInitialization>(Lifetime.Singleton);

            // Utility
            builder.Register<TransformConverter>(Lifetime.Singleton)
                .As<ITransformConverter>()
                .AsSelf();

            // EntryPoint
            builder.RegisterEntryPoint<TestEntryPoint>();
        }
    }
}
