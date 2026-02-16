using UnityEngine;
using GameEnums;
using UniRx;
using Zenject;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
[RequireComponent(typeof(CharacterVFXManager), typeof(AudioSource))]
public abstract class BaseHealth : MonoBehaviour, IHealth, IDestructable
{
    //reactive
    protected readonly ReactiveProperty<float> _currentHealth = new ReactiveProperty<float>();
    protected readonly ReactiveProperty<float> _maxHealth = new ReactiveProperty<float>();
    protected readonly ReactiveProperty<float> _currentTempHealth = new ReactiveProperty<float>();
    protected readonly ReactiveProperty<HealthState> _state = new ReactiveProperty<HealthState>(HealthState.Alive);
    protected readonly ReactiveProperty<bool> _isLowHealth = new ReactiveProperty<bool>(false);
    
    public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
    public IReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;
    public IReadOnlyReactiveProperty<float> CurrentTempHealth => _currentTempHealth;
    public IReadOnlyReactiveProperty<HealthState> State => _state;
    public IReadOnlyReactiveProperty<bool> IsLowHealth => _isLowHealth;
    
    // dependencies
    [SerializeField] protected HealthConfigSO _config;
    [Inject] protected IAnimationService _animationService;
    [Inject] protected GameData _gameData;
    [Inject] protected CameraMoovement _cameraMoovement;
    [Inject] protected BloodEffectsPoolManager _bloodEffectsPoolManager;
    [Inject] protected IAudioService _audioManager;
    
    // references
    protected Animator _animator;
    protected Rigidbody2D _rigidbody;
    protected AudioSource _audioSource;
    protected CharacterVFXManager _vFXManager;
    protected CompositeDisposable _disposables = new CompositeDisposable();
    
    protected float _bleedSpeed;
    protected bool _isDestructing;
    
    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody =  GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>(); 
        _vFXManager = GetComponent<CharacterVFXManager>();
        // Reactive subscriptions
        _currentHealth.Subscribe(OnHealthChanged).AddTo(_disposables);
        _state.Subscribe(OnStateChanged).AddTo(_disposables);
        _isLowHealth.Subscribe(OnLowHealthChanged).AddTo(_disposables);

        _maxHealth.Value = _config.baseMaxHealth;
        _currentHealth.Value = _maxHealth.Value;
        _currentTempHealth.Value = _maxHealth.Value;
        _bleedSpeed = _config.baseBleedSpeed;
    }
    
    protected virtual void Update()
    {
        HandleTempHealthBleeding();
    }
    
    public virtual (bool, bool) TakeDamage(DamageData damageData) // return (hit regisrered , parry registered)
    {
        if (_state.Value != HealthState.Alive || _isDestructing) return (false, false);
        
        float effectiveDamage = Mathf.Max(damageData.BaseDamage, 1f);
        // Apply to temp health first
        float previousTemp = _currentTempHealth.Value;
        _currentTempHealth.Value = Mathf.Max(0, _currentTempHealth.Value - effectiveDamage);
        float remainingDamage = Mathf.Max(0, effectiveDamage - (previousTemp - _currentTempHealth.Value));
        
        if (remainingDamage > 0)
        {
            int finalDamage = Mathf.CeilToInt(remainingDamage);
            _currentHealth.Value = Mathf.Max(0, _currentHealth.Value - finalDamage);
            _animationService.SetTrigger(_animator, AnimTriggers.Hit);
            // death check
            if (_currentHealth.Value <= 0 && !_isDestructing)
            {
                Destruct(damageData);
                return (true, false);
            }

            OnDamageTaken(finalDamage, damageData);
        }
        
        TriggerDamageFeedback(damageData, effectiveDamage);
        return (true, false);
    }
    
    protected virtual void OnDamageTaken(int damageAmount, DamageData damageData) { }
    
    protected virtual void TriggerDamageFeedback(DamageData damData, float effectiveDamage)
    {
        if (_gameData.NeedBlood.Value && damData.BaseDamage > 5f) _bloodEffectsPoolManager.SpawnBlood(transform.position,damData);
        _vFXManager.ActivateVfx(VfxName.DamageSlash);
       // numbers vfx
        if (_gameData.NeedDamageNums.Value) _bloodEffectsPoolManager.SpawnDamageText(transform.position, damData);
        
        // if powerful enough shake camera for good feedback
        if (effectiveDamage > _maxHealth.Value * 0.2f)  _cameraMoovement.ShakeEffect(Mathf.CeilToInt(damData.BaseDamage / 2));
        // and finally play sound
        _audioManager.PlayHitSound(_audioSource, damData.WeaponType, 0.7f);
    }
    
    public virtual void Heal(int amount)
    {
        if (_state.Value != HealthState.Alive) return;
        
        _currentHealth.Value = Mathf.Min(_currentHealth.Value + amount, _maxHealth.Value);
        _currentTempHealth.Value = Mathf.Min(_currentTempHealth.Value + (amount * 0.3f), _maxHealth.Value);
    }
    
    //base destructor for game characters after death
    public virtual void Destruct(DamageData damData)
    {
        if (_isDestructing) return;
        _isDestructing = true;
        _state.Value = HealthState.Dead;
         _animationService.SetState(_animator, AnimStates.CanMove, false);
        _animationService.SetState(_animator, AnimStates.isDead, true);
        _animationService.SetState(_animator, AnimStates.Destruct, true);
        
        OnDeath(damData);
        
        // dissable collisiison
        gameObject.layer = LayerMask.NameToLayer("Dead");
        
        Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe(_ => Destroy(gameObject)).AddTo(_disposables);
    }
    
    protected abstract void OnDeath(DamageData damData);
    
    protected virtual void HandleTempHealthBleeding()
    {
        if (_currentTempHealth.Value <= _currentHealth.Value) return;
        
        float speed = (_currentTempHealth.Value - _currentHealth.Value) < _config.slowBleedThreshold  ? _config.slowBleedSpeed  : _bleedSpeed;
            
        _currentTempHealth.Value = Mathf.Max( _currentTempHealth.Value - (speed * Time.deltaTime), _currentHealth.Value);
    }
    
    protected virtual void OnHealthChanged(float newHealth)
    {
        _isLowHealth.Value = newHealth < Mathf.Max(10, _maxHealth.Value * 0.2f);
    }
    
    protected virtual void OnStateChanged(HealthState newState)
    {
        if (newState == HealthState.Dead)enabled = false;
    }
    
    protected virtual void OnLowHealthChanged(bool isLow) { }
    
    protected virtual void OnDestroy()
    {
        _disposables.Dispose();
    }
}


