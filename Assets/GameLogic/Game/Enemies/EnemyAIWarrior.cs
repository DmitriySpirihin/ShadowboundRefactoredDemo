using UnityEngine;
using GameEnums;

[RequireComponent(typeof(MeeleAttack))]
public class EnemyAIWarrior : BaseEnemyAI
{
    [Header("Warrior Specific")]
    [SerializeField] private MeeleAttack _meleeAttack;
    [SerializeField] private float _aggroRange = 8f;
    
    private bool _isAttacking;

    protected override void Awake()
    {
        base.Awake();
        _meleeAttack.OnAttackHit += OnMeleeHit;
    }

    protected override void OnStateChase()
    {
        float distanceToHero = Vector2.Distance(transform.position, _heroTransform.position);
        
        // Если герой в зоне атаки — немедленно атакуем
        if (distanceToHero < _config.AttackRange && _heroDetector.CanAttack.Value && _stamina.CanAttack.Value)
        {
            TransitionToState(EnemyState.Attack);
            return;
        }
        
        // Иначе преследуем
        MoveTowardsTarget(_heroTransform.position, _config.ChaseSpeed);
        HandleChaseObstacles();
    }

    protected override void OnStateAttack()
    {
        if (_isAttacking) return;
        
        _isAttacking = true;
        _animService.SetTrigger(_animator, AnimTriggers.Attack);
        _moveLockTimer = _config.AttackLockDuration;
        
        // Сброс через таймаут на случай промаха
        Observable.Timer(TimeSpan.FromSeconds(_config.AttackDuration))
            .Subscribe(_ => EndAttack())
            .AddTo(this);
    }

    private void OnMeleeHit()
    {
        _stamina.ReduceStamina(_config.AttackStaminaCost);
        EndAttack();
    }

    private void EndAttack()
    {
        _isAttacking = false;
        if (_heroDetector.IsHeroDetected.Value)
            TransitionToState(EnemyState.Chase);
        else
            TransitionToState(EnemyState.Patrol);
    }

    protected override void HandleChaseObstacles()
    {
        // Воин игнорирует некоторые препятствия при преследовании
        bool isWall = Physics2D.Raycast(
            (Vector2)transform.position,
            Vector2.right * transform.localScale.x,
            _config.WallCheckDistance * 0.7f, // Более агрессивный проход сквозь узкие проходы
            _config.GroundLayer
        );

        if (isWall)
            TryJumpOverObstacle();
        else
            base.HandleChaseObstacles();
    }

    private void TryJumpOverObstacle()
    {
        if (_environmentDetector.IsGrounded.Value && _stamina.CurrentStamina.Value > _config.JumpStaminaCost)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _config.JumpHeight * 0.8f);
            _stamina.ReduceStamina(_config.JumpStaminaCost);
        }
    }

    private void OnDestroy()
    {
        if (_meleeAttack != null)
            _meleeAttack.OnAttackHit -= OnMeleeHit;
    }
}
