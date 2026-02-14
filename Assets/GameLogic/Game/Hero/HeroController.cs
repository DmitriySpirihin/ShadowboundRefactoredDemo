using UniRx;
using UnityEngine;
using Zenject;
using GameEnums;

public class HeroController : MonoBehaviour
{
    // DI
    [Inject] private HeroMovementConfigSO _config;
    [Inject] private IAnimationService _animService;
    [Inject] private IInputMove _input;
    [Inject] private Rigidbody2D _rigidbody;
    [Inject] private EnvironmentDetector _environmentDetecter;
    [Inject] private Animator _animator;
    [Inject] private ShieldSystem _shieldSystem;
    [Inject] private ParrySystem _parrySystem;
    [Inject] private IStamina _stamina;
    [Inject] private IHealth _health;

    // Local state
    private float _coyoteTimer;
    private float _moveLockTimer;
    private float _attackComboTimer;
    private int _attackNum;
    private bool _canDoubleJump = true;
    private bool _canDash = true;
    private bool _isAttacking;
    private bool _canSuperAttack;
    private float _superAttackTimer;

    private readonly CompositeDisposable _disposables = new();

    [Inject]
    private void Construct()
    {
        _health.State.Where(state => state == HealthState.Dead).Subscribe(_ => enabled = false).AddTo(_disposables);
        // remove jump if grounded
        _environmentDetecter.IsGrounded.Where(grounded => grounded).Subscribe(_ => ResetJumpState()).AddTo(_disposables);
    }

    private void ResetJumpState()
    {
        _coyoteTimer = _config.CoyoteTime;
        _canDoubleJump = true;
        _canDash = true;
    }

    private void FixedUpdate()
    {
        if (_health.State.Value == HealthState.Dead) return;

        UpdateTimers(Time.fixedDeltaTime);
        HandleMovement();
        ApplylinearVelocityConstraints();
        UpdateAnimator();
    }

    private void UpdateTimers(float dt)
    {
        _coyoteTimer = _environmentDetecter.IsGrounded.Value ? _config.CoyoteTime : Mathf.Max(0f, _coyoteTimer - dt);
        _moveLockTimer = Mathf.Max(0f, _moveLockTimer - dt);
        _attackComboTimer = Mathf.Max(0f, _attackComboTimer - dt);
        if (_attackComboTimer <= 0f && _attackNum > 0) _attackNum = 0;
        _superAttackTimer = Mathf.Max(0f, _superAttackTimer - dt);
        if (_superAttackTimer <= 0f && _canSuperAttack) _canSuperAttack = false;
    }

    private void HandleMovement()
    {
        bool canMove = _moveLockTimer <= 0f;
        bool isGrounded = _environmentDetecter.IsGrounded.Value;
        bool isWall = _environmentDetecter.IsTouchingWall.Value;
        bool canClimb = _environmentDetecter.CanClimbLedge;

        // Horizontal movement
        if (canMove && !_shieldSystem.IsShielded.Value)
        {
            float speed = isGrounded && _shieldSystem.IsShielded.Value ? _config.GetActualShieldedSpeed() : _config.BaseSpeed;

            // Flip 
            if (_input.GetDirection().x != 0f && !(_shieldSystem.IsShielded.Value && _input.GetDirection().x * transform.localScale.x < 0f))
                transform.localScale = new Vector3(Mathf.Sign(_input.GetDirection().x), 1f, 1f);

            _rigidbody.linearVelocity = new Vector2(_input.GetDirection().x * speed, _rigidbody.linearVelocity.y);
        }

        if (isWall)
        {
            if (canClimb && _input.GetDirection().y > -1f)
            {
                _animService.SetState(_animator, AnimStates.Cling, true);

                if (_rigidbody.linearVelocity.y > -0.1f)
                {
                    if (Mathf.Approximately(_input.GetDirection().y, 0f)) _rigidbody.linearVelocity = Vector2.zero;
                    else if (_input.GetDirection().y < 0f) 
                    {
                        _animService.SetState(_animator, AnimStates.Cling, false);
                        _rigidbody.gravityScale = 1f;
                    }
                    else if (_input.GetDirection().y > 0f)
                    {
                        _animService.SetTrigger(_animator, AnimTriggers.Climb);
                        _moveLockTimer = _config.MoveDelay;
                    }
                }
                else
                {
                    _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _config.WallSlideSpeed);
                }
            }
            else
            {
                _animService.SetState(_animator, AnimStates.Slide, !isGrounded);
                if (!isGrounded) _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _config.WallSlideSpeed);
            }
        }
        else
        {
            _animService.SetState(_animator, AnimStates.Slide, false);
            _animService.SetState(_animator, AnimStates.Cling, false);
        }
    }

    private void ApplylinearVelocityConstraints()
    {
        // Clamp for dash
        if (Mathf.Abs(_rigidbody.linearVelocity.x) > _config.MaxDashSpeed) _rigidbody.linearVelocity = new Vector2(Mathf.Sign(_rigidbody.linearVelocity.x) * _config.MaxDashSpeed, _rigidbody.linearVelocity.y);

        // Clamp for fall speed
        if (_rigidbody.linearVelocity.y < -_config.MaxFallSpeed) _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, -_config.MaxFallSpeed);
    }

    private void UpdateAnimator()
    {
        _animService.SetValue(_animator, AnimValues.VelocityX, Mathf.Abs(_rigidbody.linearVelocity.x), 0);
        _animService.SetValue(_animator, AnimValues.VelocityY, _rigidbody.linearVelocity.y, 0);
        _animService.SetState(_animator, AnimStates.Grounded, _environmentDetecter.IsGrounded.Value);
        _animService.SetState(_animator, AnimStates.Shielded, _shieldSystem.IsShielded.Value);
        _animService.SetState(_animator, AnimStates.MoovingBack, _shieldSystem.IsShielded.Value && (_input.GetDirection().x * transform.localScale.x < 0f));
        _animService.SetValue(_animator, AnimValues.AttackNum, _attackNum, _attackNum);
        _animService.SetState(_animator, AnimStates.CanSuperAttack, _canSuperAttack);
        _animService.SetState(_animator, AnimStates.CanMove, _moveLockTimer <= 0f);
    }

    // Jump
    public void OnJump()
    {
        if (_parrySystem.IsParrying.Value || _isAttacking || _moveLockTimer > 0f) return;
        bool isWall = _environmentDetecter.IsTouchingWall.Value;

        // Ground jump with coyote time
        if ((_environmentDetecter.IsGrounded.Value || _coyoteTimer > 0f) && !isWall && TryConsumeStamina(_config.JumpStamina))
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _config.JumpHeight);
            _animService.SetTrigger(_animator, AnimTriggers.Jump);
            return;
        }

        // Double jump in the air
        if (!_environmentDetecter.IsGrounded.Value && _canDoubleJump && !isWall && TryConsumeStamina(_config.DoubleJumpStamina))
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _config.DoubleJumpHeight);
            _animService.SetTrigger(_animator, AnimTriggers.DoubleJump);
            _canDoubleJump = false;
            return;
        }

        // Wall jump
        if (isWall && TryConsumeStamina(_config.WallJumpStamina))
        {
            float wallDir = -transform.localScale.x;
            float coeff = ((wallDir > 0f && _input.GetDirection().x < 0f) || (wallDir < 0f && _input.GetDirection().x > 0f)) ? 0.4f : 0.9f;

            _rigidbody.gravityScale = 1f;
            _rigidbody.linearVelocity = new Vector2(_config.BaseSpeed * wallDir * coeff, _config.JumpHeight);
            _animService.SetTrigger(_animator, AnimTriggers.WallJump);
            _moveLockTimer = _config.MoveDelay;
        }
    }

    public void OnRoll()
    {
        if (_parrySystem.IsParrying.Value || _isAttacking || _moveLockTimer > 0f || 
            _shieldSystem.IsShielded.Value || !TryConsumeStamina(_config.RollStamina)) return;

        _animService.SetTrigger(_animator, AnimTriggers.Dash);
        _moveLockTimer = _config.MoveDelay;
        bool isWall = _environmentDetecter.IsTouchingWall.Value;

        if (_environmentDetecter.IsGrounded.Value && !isWall)
        {
            _rigidbody.linearVelocity = new Vector2(transform.localScale.x * _config.RollDistance, _rigidbody.linearVelocity.y);
            _canDash = false;
        }
        else if (!_environmentDetecter.IsGrounded.Value && !isWall && _canDash)
        {
            _rigidbody.linearVelocity = new Vector2(transform.localScale.x * _config.DashDistance, 1.5f);
            _canDash = false;
        }
    }

    public void OnAttack()
    {
        if (_parrySystem.IsParrying.Value || _moveLockTimer > 0f || _isAttacking) return;

        if (_environmentDetecter.IsGrounded.Value)
        {
            if (_canSuperAttack)
            {
                _animService.SetTrigger(_animator, AnimTriggers.Attack);
                _isAttacking = true;
                _moveLockTimer = _config.MoveDelay * 2f;
                _canSuperAttack = false;
                Invoke(nameof(ResetAttackState), 0.3f);
            }
            else
            {
                _attackNum = (_attackNum % _config.MaxComboAttacks) + 1;
                _attackComboTimer = _config.AttackComboDelay;
                _animService.SetTrigger(_animator, AnimTriggers.Attack);
                _isAttacking = true;
                _moveLockTimer = _config.MoveDelay * 2f;
                Invoke(nameof(ResetAttackState), 0.2f);
            }
        }
        else
        {
            _animService.SetTrigger(_animator, AnimTriggers.AirAttack);
            _isAttacking = true;
            Invoke(nameof(ResetAttackState), 0.3f);
        }
    }

    public void OnParry()
    {
        if (_environmentDetecter.IsTouchingWall.Value || _moveLockTimer > 0f || !TryConsumeStamina(_config.ParryStamina)) return;

        _animService.SetTrigger(_animator, AnimTriggers.Parry);
        _parrySystem.StartParry(_config.ParryDuration);
    }

    public void OnShieldUp() => _shieldSystem.LiftShield(true);
    public void OnShieldDown() => _shieldSystem.LiftShield(false);

    public void SetSuperAttack()
    {
        _canSuperAttack = true;
        _superAttackTimer = _config.SuperAttackDuration;
        _animService.SetState(_animator, AnimStates.CanSuperAttack, true);
    }

    // ====== Вспомогательные методы ======
    private bool TryConsumeStamina(float amount)
    {
        if (_stamina.CurrentStamina.Value < amount) return false;
        _stamina.ReduceStamina(amount);
        return true;
    }

    private void ResetAttackState() => _isAttacking = false;

    private void OnDestroy() => _disposables.Dispose();
}