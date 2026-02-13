using Zenject;
using UnityEngine;

public class HeroInstaller : MonoInstaller
{
    [Header("Configuration")]
    [SerializeField] private PlayerHealthConfigSO _healthConfig;

    public override void InstallBindings()
    {
        BindConfigs();
        BindCoreSystems();
    }

    private void BindConfigs()
    {
        Container.Bind<PlayerHealthConfigSO>().FromInstance(_healthConfig).WhenInjectedInto<HeroHealth>();
    }

    private void BindCoreSystems()
    {
        Container.Bind<IHealth>().To<HeroHealth>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<IStamina>().To<StaminaWithShield>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<HeroController>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<CombatSystem>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<ParrySystem>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<EnvironmentDetector>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<ShieldSystem>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<Rigidbody2D>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<Animator>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<SpriteRenderer>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<IVfxService>().To<CharacterVFXManager>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<ConcentrationSystem>().FromComponentOn(gameObject).AsSingle();
    }
}


