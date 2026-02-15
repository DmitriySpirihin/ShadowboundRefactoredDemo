using System;
using UniRx;
using UnityEngine;
using Zenject;
using GameEnums;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public abstract class BaseEnemyAI : MonoBehaviour
{
    // === Dependencies ===
    [Inject] protected IAnimationService _animService;
    
    // === Serialized References ===
    [Header("Core Components")]
    [SerializeField] protected EnemyMovementConfigSO _config;
    [SerializeField] protected Rigidbody2D _rigidbody;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected EnvironmentDetector _environmentDetector;
    [SerializeField] protected HeroDetector _heroDetector;
    
    [Header("Targeting")]
    [SerializeField] protected Transform _heroTransform;
    
    [Header("Health & Stamina")]
    [SerializeField] protected EnemyHealth _health;
    [SerializeField] protected StaminaCommon _stamina;
    
    // Reactive State
    private readonly ReactiveProperty<EnemyState> _currentState = new(EnemyState.Patrol);
    protected IReadOnlyReactiveProperty<EnemyState> CurrentState => _currentState;
    
    private readonly ReactiveProperty<bool> _canMove = new(true);
    protected IReadOnlyReactiveProperty<bool> CanMove => _canMove;
    
    //Internal State
    private float _moveLockTimer;
    private float _stateTransitionCooldown;
    private Vector2 _patrolTarget;
    private bool _isInitialized;
    
    private readonly CompositeDisposable _disposables = new();

    // Lifecycle 
    protected virtual void Awake()
    {
        CacheDependencies();
        InitializeStateMachine();
        InitializeReactiveBindings();
    }

    private void CacheDependencies()
    {
        _isInitialized = _rigidbody != null && 
                         _animator != null && 
                         _heroTransform != null &&
                         _heroDetector != null;
        
        if (!_isInitialized)
            Debug.LogError($"{name}: Missing required dependencies!", this);
    }

    private void InitializeStateMachine()
    {
        _currentState.Value = EnemyState.Patrol;
        SetPatrolTarget();
    }

    private void InitializeReactiveBindings()
    {
        // Смерть → финальное состояние
        _health.State
            .Where(state => state == HealthState.Dead)
            .Subscribe(_ => TransitionToState(EnemyState.Dead))
            .AddTo(_disposables);
        
        // Обнаружение героя → переход в погоню/атаку
        _heroDetector.IsHeroDetected
            .DistinctUntilChanged()
            .Subscribe(detected =>
            {
                if (detected && _currentState.Value != EnemyState.Dead)
                    TransitionToState(EnemyState.Chase);
                else if (!detected && _currentState.Value != EnemyState.Dead)
                    TransitionToState(EnemyState.Patrol);
            })
            .AddTo(_disposables);
        
        // Доступность атаки → автоматический переход
        Observable.CombineLatest(
            _heroDetector.CanAttack,
            _stamina.CurrentStamina.Value > 40f,
            _currentState,
            (canAttack, staminaOk, state) => canAttack && staminaOk && (state == EnemyState.Chase || state == EnemyState.Attack)
        )
        .DistinctUntilChanged()
        .Where(can => can)
        .Subscribe(_ => TransitionToState(EnemyState.Attack))
        .AddTo(_disposables);
    }

    // === Main Loop ===
    protected virtual void FixedUpdate()
    {
        if (!_isInitialized || _currentState.Value == EnemyState.Dead) return;
        
        UpdateTimers(Time.fixedDeltaTime);
        ExecuteCurrentState();
        ApplyPhysicsConstraints();
        UpdateAnimator();
    }

    private void UpdateTimers(float dt)
    {
        _moveLockTimer = Mathf.Max(0f, _moveLockTimer - dt);
        _stateTransitionCooldown = Mathf.Max(0f, _stateTransitionCooldown - dt);
    }

    private void ExecuteCurrentState()
    {
        switch (_currentState.Value)
        {
            case EnemyState.Idle:
                OnStateIdle();
                break;
            case EnemyState.Patrol:
                OnStatePatrol();
                break;
            case EnemyState.Chase:
                OnStateChase();
                break;
            case EnemyState.Attack:
                OnStateAttack();
                break;
            case EnemyState.Stunned:
                OnStateStunned();
                break;
        }
    }

    // === State Transitions ===
    protected void TransitionToState(EnemyState newState)
    {
        if (_stateTransitionCooldown > 0f || _currentState.Value == newState) return;
        
        ExitCurrentState();
        _currentState.Value = newState;
        EnterNewState();
        _stateTransitionCooldown = _config.StateTransitionDelay;
        
        _animService.SetState(_animator, AnimStates.EnemyState, (int)newState);
    }

    private void ExitCurrentState()
    {
        switch (_currentState.Value)
        {
            case EnemyState.Attack:
                _animService.SetTrigger(_animator, AnimTriggers.AttackEnd);
                break;
        }
    }

    private void EnterNewState()
    {
        switch (_currentState.Value)
        {
            case EnemyState.Patrol:
                SetPatrolTarget();
                break;
            case EnemyState.Attack:
                _moveLockTimer = _config.AttackLockDuration;
                break;
        }
    }

    // === State Implementations (виртуальные для кастомизации) ===
    protected virtual void OnStateIdle() => StopMovement();
    
    protected virtual void OnStatePatrol()
    {
        if (Vector2.Distance(transform.position, _patrolTarget) < _config.PatrolArrivalThreshold)
            SetPatrolTarget();
        
        MoveTowardsTarget(_patrolTarget, _config.PatrolSpeed);
        HandlePatrolObstacles();
    }
    
    protected virtual void OnStateChase()
    {
        MoveTowardsTarget(_heroTransform.position, _config.ChaseSpeed);
        HandleChaseObstacles();
    }
    
    protected virtual void OnStateAttack()
    {
        // Анимация и эффекты управляются через события аниматора
        // Физика заморожена во время атаки
        StopMovement();
    }
    
    protected virtual void OnStateStunned()
    {
        StopMovement();
        _rigidbody.velocity = Vector2.zero;
    }

    // === Core Movement Utilities ===
    protected void MoveTowardsTarget(Vector2 target, float speed)
    {
        if (!_canMove.Value || _moveLockTimer > 0f) return;
        
        float direction = Mathf.Sign(target.x - transform.position.x);
        if (Mathf.Abs(direction) > 0.1f)
            transform.localScale = new Vector3(direction, 1f, 1f);
        
        _rigidbody.linearVelocity = new Vector2(direction * speed, _rigidbody.linearVelocity.y);
    }

    protected void StopMovement() => 
        _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);

    protected void ApplyImpulse(Vector2 force)
    {
        _rigidbody.AddForce(force, ForceMode2D.Impulse);
        TransitionToState(EnemyState.Stunned);
        Observable.Timer(TimeSpan.FromSeconds(_config.StunDuration))
            .Subscribe(_ => TransitionToState(EnemyState.Patrol))
            .AddTo(_disposables);
    }

    // === Patrol Logic ===
    private void SetPatrolTarget()
    {
        float patrolRadius = _config.PatrolRadius * Random.Range(0.7f, 1.3f);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        _patrolTarget = (Vector2)transform.position + new Vector2(
            Mathf.Cos(angle) * patrolRadius,
            Mathf.Sin(angle) * patrolRadius
        );
    }

    protected virtual void HandlePatrolObstacles()
    {
        bool isLedge = !Physics2D.Raycast(
            (Vector2)transform.position + new Vector2(transform.localScale.x * _config.LedgeCheckOffset, 0f),
            Vector2.down,
            _config.LedgeCheckDistance,
            _config.GroundLayer
        );
        
        bool isWall = Physics2D.Raycast(
            (Vector2)transform.position,
            Vector2.right * transform.localScale.x,
            _config.WallCheckDistance,
            _config.GroundLayer
        );

        if (isLedge || isWall)
            transform.localScale = new Vector3(-transform.localScale.x, 1f, 1f);
    }

    protected virtual void HandleChaseObstacles()
    {
        // Базовая реализация — наследники могут расширять
        HandlePatrolObstacles();
    }

    // === Physics & Animation ===
    private void ApplyPhysicsConstraints()
    {
        // Clamp fall speed
        if (_rigidbody.linearVelocity.y < -_config.MaxFallSpeed)
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, -_config.MaxFallSpeed);
        
        // Clamp horizontal speed during dash-like states
        if (Mathf.Abs(_rigidbody.linearVelocity.x) > _config.MaxDashSpeed)
            _rigidbody.linearVelocity = new Vector2(
                Mathf.Sign(_rigidbody.linearVelocity.x) * _config.MaxDashSpeed,
                _rigidbody.linearVelocity.y
            );
    }

    private void UpdateAnimator()
    {
        _animService.SetValue(_animator, AnimValues.VelocityX, Mathf.Abs(_rigidbody.linearVelocity.x));
        _animService.SetValue(_animator, AnimValues.VelocityY, _rigidbody.linearVelocity.y);
        _animService.SetState(_animator, AnimStates.Grounded, _environmentDetector.IsGrounded.Value);
        _animService.SetState(_animator, AnimStates.CanMove, _canMove.Value && _moveLockTimer <= 0f);
    }

    // === Public API для триггеров из аниматора/других систем ===
    public void OnAttackStarted() => _isAttacking = true;
    public void OnAttackEnded() => _isAttacking = false;
    public void OnTakeDamage() => TransitionToState(EnemyState.Stunned);
    public void SetCanMove(bool value) => _canMove.Value = value;

    private void OnDestroy() => _disposables.Dispose();
}
