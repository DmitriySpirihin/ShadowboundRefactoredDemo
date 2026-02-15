using UnityEngine;
using System;
using UniRx;
using Zenject;
using GameEnums;
using UnityEngine.Assertions.Must;

[RequireComponent (typeof(ShieldSystem))]
[RequireComponent (typeof(ParrySystem))]
public class HeroHealth : BaseHealth
{
    public float maxH;
    private ShieldSystem _shieldSystem;   
    private ParrySystem _parrySystem; 
    private ReactiveProperty<bool> _isStaggered = new ReactiveProperty<bool>(false);
    private bool isInvincible;
    
    public IReadOnlyReactiveProperty<bool> IsStaggered => _isStaggered;

    protected override void Awake()
    {
        _shieldSystem = GetComponent<ShieldSystem>();
        _parrySystem = GetComponent<ParrySystem>();
        _maxHealth.Value = _gameData.HealthLevel.Value * 25f;
        _currentHealth.Value = _maxHealth.Value;
        _currentTempHealth.Value = _maxHealth.Value;
        maxH = _maxHealth.Value;
    }

    public override (bool, bool) TakeDamage(DamageData damageData)
    {
        if(isInvincible) return (false, false);
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

    public void ChangeInvincibility(bool value) => isInvincible = value;
    
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