using GameEnums;
using UnityEngine;
using Zenject;


public class HeroController : MonoBehaviour
{
    // Global servises
    [Inject] private HeroMovementConfigSO _comfig; // the same variables
    [Inject] private AnimationMapper _animationMapper;  // SetState , SetTrigger , SetValue (animator, AnimState , value)
    [Inject] private GameData _gameData;
    [Inject] private IInputMove _inputMove;

    // Components
    [Inject] private Rigidbody2D _rigidbody;
    [Inject] private EnvironmentDetector _environmentDetector; // IsGrounded ,IsTouchingWall , IsLedgeBlocked  Value
    [Inject] Animator _animator;
    [Inject] ShieldSystem _shieldSystem;  //  IsShielded.Value
    [Inject] ParrySystem _parrySystem;  //  IsParrying.Value
    [Inject] IStamina _stamina;  //  CurrentStamina.Value
    [Inject] IHealth _health;  // State.Value   HealthState.IsAlive , Dead
    [Inject] CombatSystem _combatSystem; // Hit(SlashType slashType) ->  Slash, Slash, WideSlash, Pierce 
 
    private void FixedUpdate()
    {
        
    }

    public void OnJump()
    {
        
    }

    public void OnRoll()
    {
        
    }

    public void OnShieldUp()
    {
        
    }

    public void OnShieldDown()
    {
        
    }

    public void OnParry()
    {
        
    }

    public void OnAttack()
    {
        
    }

    private void OnMove()
    {
        
    }
}
