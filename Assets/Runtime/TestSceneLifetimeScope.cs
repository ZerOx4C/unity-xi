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
        public UIBehaviour uiPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            // Behaviour
            builder.RegisterInstance(devilPrefab);
            builder.RegisterInstance(dicePrefab);
            builder.RegisterInstance(floorPrefab);
            builder.RegisterInstance(uiPrefab);

            // Controller
            builder.Register<PlayerDevilController>(Lifetime.Singleton);

            // Entity
            builder.Register<Session>(Lifetime.Singleton);

            // Input
            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();

            // Presenter
            builder.Register<DebugUIPresenter>(Lifetime.Singleton);
            builder.Register<DevilPresenter>(Lifetime.Singleton);
            builder.Register<DicePresenter>(Lifetime.Singleton);
            builder.Register<FieldPresenter>(Lifetime.Singleton);

            // UseCase
            builder.Register<DicePushing>(Lifetime.Singleton);
            builder.Register<FloorInitialization>(Lifetime.Singleton);
            builder.Register<PlayerInitialization>(Lifetime.Singleton);
            builder.Register<UIInitialization>(Lifetime.Singleton);

            // Utility
            builder.Register<DiceBehaviourRepository>(Lifetime.Singleton);
            builder.Register<TransformConverter>(Lifetime.Singleton)
                .As<ITransformConverter>()
                .AsSelf();

            // EntryPoint
            builder.RegisterEntryPoint<TestEntryPoint>();
        }
    }
}
