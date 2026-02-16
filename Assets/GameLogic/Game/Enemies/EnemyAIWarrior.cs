using UnityEngine;
using GameEnums;
using UniRx;
using System;

[RequireComponent(typeof(CombatSystem))]
public class EnemyAIWarrior : BaseEnemyAI
{
    [Header("Warrior Specific")]
    [SerializeField] private CombatSystem _combatSystem;
    [SerializeField] private float _aggroRange = 8f;

    protected override void OnStateChase()
    {
        float distanceToHero = Vector2.Distance(transform.position, _heroTransform.position);
        
        // If hero in attack zone and stamina > min stamina threshold attack immediately
        if (distanceToHero < _config.AttackRange && _heroDetector.CanAttack.Value && _stamina.CurrentStamina.Value > _config.MinStaminaForAttack)
        {
            TransitionToState(EnemyState.Attack);
            return;
        }
        
        // Or chase the hero
        MoveTowardsTarget(_heroTransform.position, _config.ChaseSpeed);
        HandleChaseObstacles();
    }

    protected override void OnStateAttack()
    {
        if (_isAttacking) return;
        
        _isAttacking = true;
        _animService.SetTrigger(_animator, AnimTriggers.Attack);
        _moveLockTimer = _config.AttackLockDuration;
        
        Observable.Timer(TimeSpan.FromSeconds(_config.AttackDuration)).Subscribe(_ => EndAttack()).AddTo(this);
    }

    private void OnMeleeHit()
    {
        _stamina.ReduceStamina(_config.AttackStaminaCost);
        EndAttack();
    }

    private void EndAttack()
    {
        _isAttacking = false;
        if (_heroDetector.IsHeroDetected.Value) TransitionToState(EnemyState.Chase);
        else TransitionToState(EnemyState.Patrol);
    }

}
