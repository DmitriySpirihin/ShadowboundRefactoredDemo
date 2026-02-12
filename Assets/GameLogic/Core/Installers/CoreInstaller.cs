using System.ComponentModel;
using UnityEngine;
using Zenject;


  public class CoreInstaller : MonoInstaller
  {
    [Header("Dependencies")]
    [SerializeField] private GameObject hero;
    [SerializeField] private CameraMoovement cameraMoovement;
    [SerializeField] private GameConfigSO gameConfig;
    [SerializeField] private BloodEffectsPoolManager _poolManager;

    [Header("Flags")]
    [SerializeField] private bool neeedStick;
 
    public override void InstallBindings()
    {
        Container.Bind<DiContainer>().FromInstance(Container).AsSingle();

        Container.Bind<GameConfigSO>().FromInstance(gameConfig).AsSingle();
        Container.Bind<GameData>().AsSingle().NonLazy();
        Container.Bind<IAudioService>().To<AudioManager>().AsSingle();
        Container.Bind<BloodEffectsPoolManager>().FromInstance(_poolManager).AsSingle().NonLazy();
        Container.Bind<CameraMoovement>().FromInstance(cameraMoovement).AsSingle().NonLazy();
        Container.Bind<IAnimationService>().To<AnimationMapper>().AsSingle();
        Container.Bind<Transform>().WithId("HeroTr").FromInstance(hero.transform).AsSingle();
        Container.Bind<Animator>().WithId("HeroAnim").FromComponentOn(hero).AsSingle();
        Container.Bind<HeroController>().FromComponentOn(hero).AsSingle();
        // inputs 
        if(!neeedStick) Container.Bind<IInputMove>().To<InputKeyboard>().AsSingle();
        else Container.Bind<IInputMove>().To<InputOnScreenStick>().AsSingle();
        
    }
  }
