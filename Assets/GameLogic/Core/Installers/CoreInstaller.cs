using UnityEngine;
using Zenject;


  public class CoreInstaller : MonoInstaller
  {
    [Header("Configuration")]
    [SerializeField] private HeroMovementConfigSO _movementConfig;
    [SerializeField] private GameConfigSO gameConfig;
    [Header("Dependencies")]
    [SerializeField] private GameObject hero;
    [SerializeField] private CameraMoovement cameraMoovement;
    [SerializeField] private BloodEffectsPoolManager _poolManager;
    [SerializeField] private AudioManager _audioManager;

    [Header("Flags")]
    [SerializeField] private bool neeedStick;
 
    public override void InstallBindings()
    {
        Container.Bind<GameConfigSO>().FromInstance(gameConfig).AsSingle().NonLazy();
        Container.Bind<HeroMovementConfigSO>().FromInstance(_movementConfig).AsSingle();
        Container.Bind<Transform>().WithId("HeroTr").FromInstance(hero.transform).AsSingle();
        Container.Bind<IHealth>().To<HeroHealth>().FromComponentOn(hero).AsSingle();
        Container.Bind<IStamina>().To<StaminaWithShield>().FromComponentOn(hero).AsSingle();
        Container.Bind<HeroController>().FromComponentOn(hero).AsSingle();
        Container.Bind<GameData>().AsSingle().NonLazy();
        Container.Bind<IAudioService>().To<AudioManager>().FromInstance(_audioManager).AsSingle().NonLazy();
        Container.Bind<BloodEffectsPoolManager>().FromInstance(_poolManager).AsSingle().NonLazy();
        Container.Bind<CameraMoovement>().FromInstance(cameraMoovement).AsSingle().NonLazy();
        Container.Bind<IAnimationService>().To<AnimationMapper>().AsSingle();
        // inputs 
        if(!neeedStick) Container.Bind<IInputMove>().To<InputKeyboard>().AsSingle();
        else Container.Bind<IInputMove>().To<InputOnScreenStick>().AsSingle();
        
    }
  }
