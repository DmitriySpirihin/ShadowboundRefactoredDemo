using UnityEngine;
using System;
using UniRx;
using Zenject;
using GameEnums;

public class HeroHealth : BaseHealth<PlayerHealthConfigSO>
{
    [Inject] private ShieldSystem _shieldSystem;   
    private ReactiveProperty<bool> _isStaggered = new ReactiveProperty<bool>(false);
    private ReactiveProperty<float> _concentration = new ReactiveProperty<float>(0f);
    private float _staggerTimer;
    
    public IReadOnlyReactiveProperty<bool> IsStaggered => _isStaggered;
    public IReadOnlyReactiveProperty<float> Concentration => _concentration;
    
    protected override void Awake()
    {
        base.Awake();
        
       // _concentration.Subscribe(OnConcentrationChanged).AddTo(_disposables);
    }
    
    public override bool TakeDamage(DamageData damageData)
    {
        
        base.TakeDamage(damageData);
        return false;
    }
    
    protected override void OnDamageTaken(int damageAmount, DamageData damageData)
    {
        // Check for stagger
        if (damageAmount > _maxHealth.Value * 0.3f || (_isStaggered.Value && damageAmount > 0))
        {
            TriggerStagger();
        }
        
        // Reset concentration on significant hits
        if (damageAmount > _maxHealth.Value * 0.15f)
        {
            _concentration.Value = 0f;
        }
    }
    
    private void TriggerStagger()
    {
        _isStaggered.Value = true;
        _staggerTimer = _config.staggerDuration;
        _animationService.SetState(_animator, AnimStates.Staggered, true);
        
        _cameraMoovement.SlowMotionEffect(false,transform , _config.slowMoPower , _config.cameraSize);
    }
    
    protected override void Update()
    {
        base.Update();
        HandleStaggerTimer();
        HandleConcentrationDecay();
    }
    
    private void HandleStaggerTimer()
    {
        if (!_isStaggered.Value) return;
        
        _staggerTimer -= Time.deltaTime;
        if (_staggerTimer <= 0)
        {
            _isStaggered.Value = false;
            _animationService.SetState(_animator, AnimStates.Staggered, false);
        }
    }
    
    private void HandleConcentrationDecay()
    {
        if (_concentration.Value > 0 && _concentration.Value < 99f)
        {
            _concentration.Value -= _config.concentrationDecaySpeed * Time.deltaTime;
        }
    }
    
    public void ApplyConcentration(float amount)
    {
        _concentration.Value = Mathf.Clamp(_concentration.Value + amount, 0f, 100f);
        
        if (_concentration.Value >= _config.concentrationStaggerThreshold)
        {
            TriggerStagger();
            _concentration.Value = 0f;
            
            _cameraMoovement.SlowMotionEffect(false,this.transform , _config.slowMoPower , _config.cameraSize);
        }
    }
    
    protected override void OnDeath(DamageData cause)
    {
        _cameraMoovement.SlowMotionEffect(false,this.transform , _config.slowMoPower , _config.cameraSize);
    }
    
    public override void Destruct(DamageData cause)
    {
        // Player doesn't auto-destroy - handled by death menu system
        _isDestructing = true;
        _state.Value = HealthState.Dead;
        _animationService.SetState(_animator, AnimStates.isDead, true);
        _animationService.SetState(_animator, AnimStates.Destruct, true);
        gameObject.layer = LayerMask.NameToLayer("Dead");
        enabled = false;
        
        OnDeath(cause);
    }
    
    private void OnConcentrationChanged(float value)
    {
    
    }
    
}