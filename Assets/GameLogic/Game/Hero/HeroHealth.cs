using UnityEngine;
using System;
using UniRx;
using Zenject;
using GameEnums;

[RequireComponent (typeof(Animator))]
public class HeroHealth : BaseHealth<PlayerHealthConfigSO>
{
    [Inject] private ShieldSystem _shieldSystem;   
    [Inject] private ParrySystem _parrySystem; 
    [Inject] private IStamina _staminaSystem;
    private ReactiveProperty<bool> _isStaggered = new ReactiveProperty<bool>(false);
    private float _staggerTimer;
    private bool isInvincible;
    
    public IReadOnlyReactiveProperty<bool> IsStaggered => _isStaggered;
    
    public override (bool, bool) TakeDamage(DamageData damageData)
    {
        if (_parrySystem.IsParrying.Value)
        {
            
             return (false, true);
        }
        if (_shieldSystem.IsShielded.Value)
        {
            
            return (false, false);
        }
        
        base.TakeDamage(damageData);
        return (true, false);
    }
    
    protected override void OnDeath(DamageData cause)
    {
        _cameraMoovement.SlowMotionEffect(false,this.transform , _config.slowMoPower , _config.cameraSize);
    }
    
    public override void Destruct(DamageData cause)
    {
        _isDestructing = true;
        _state.Value = HealthState.Dead;
        _animationService.SetState(_animator, AnimStates.isDead, true);
        _animationService.SetState(_animator, AnimStates.Destruct, true);
        gameObject.layer = LayerMask.NameToLayer("Dead");
        enabled = false;
        
        OnDeath(cause);
    }
    
}