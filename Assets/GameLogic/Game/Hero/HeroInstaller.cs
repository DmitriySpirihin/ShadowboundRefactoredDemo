using Zenject;
using UnityEngine;
using GameEnums;

[RequireComponent(typeof(HeroController))]
[RequireComponent(typeof(HeroHealth))]
[RequireComponent(typeof(CombatSystem))]

public class HeroInstaller : MonoInstaller<HeroInstaller>
{
    // === CONFIGURATION ===
    [Header("Core Configs")]
    [SerializeField] private PlayerHealthConfigSO _healthConfig;
    [SerializeField] private HeroHealth heroHealth;
    [SerializeField] private CharacterVFXManager vFXManager;

    public override void InstallBindings()
    {
        BindConfigs();
    }

    private void BindConfigs()
    {
       Container.BindInstance(_healthConfig).WhenInjectedInto<HeroHealth>();
       Container.Bind<IVfxService>().To<CharacterVFXManager>().FromInstance(vFXManager);
       Container.Bind<IHealth>().To<HeroHealth>().FromInstance(heroHealth);
    }

    
}


