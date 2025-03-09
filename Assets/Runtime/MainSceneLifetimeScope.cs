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
    public class MainSceneLifetimeScope : LifetimeScope
    {
        public DevilBehaviour devilPrefab;
        public DiceBehaviour dicePrefab;
        public FieldBehaviour fieldPrefab;
        public UIBehaviour uiPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            // Behaviour
            builder.RegisterInstance(devilPrefab);
            builder.RegisterInstance(dicePrefab);
            builder.RegisterInstance(fieldPrefab);
            builder.RegisterInstance(uiPrefab);

            // Controller
            builder.Register<PlayerDevilController>(Lifetime.Singleton);

            // Entity
            builder.Register<Game>(Lifetime.Singleton);

            // Input
            builder.Register<PlayerInputSubject>(Lifetime.Singleton)
                .As<IPlayerInputObservable>()
                .AsSelf();

            // Presenter
            builder.Register<DebugUIPresenter>(Lifetime.Singleton);
            builder.Register<DevilPresenter>(Lifetime.Singleton);
            builder.Register<DicePresenter>(Lifetime.Singleton);
            builder.Register<FieldPresenter>(Lifetime.Singleton);
            builder.Register<GamePresenter>(Lifetime.Singleton);
            builder.Register<SessionPresenter>(Lifetime.Singleton);
            builder.Register<UIPresenter>(Lifetime.Singleton);

            // UseCase
            builder.Register<DicePushing>(Lifetime.Singleton);

            // Utility
            builder.Register<DiceBehaviourProvider>(Lifetime.Singleton);
            builder.Register<TransformConverter>(Lifetime.Singleton)
                .As<ITransformConverter>()
                .AsSelf();

            // EntryPoint
            builder.RegisterEntryPoint<MainEntryPoint>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            builder.RegisterEntryPoint<DebugLogPresenter>();
#endif
        }
    }
}
