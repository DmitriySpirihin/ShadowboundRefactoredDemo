using UnityEngine;


[CreateAssetMenu(fileName = "HeroMovementConfig", menuName = "Scriptable Objects/HeroMovementConfig")]
public class HeroMovementConfigSO : ScriptableObject
{
    [Header("CORE MOVEMENT")]
    [Range(3f, 15f)]public float BaseSpeed = 5f;
    [Range(0.1f, 1f)] public float ShieldedSpeedMultiplier = 0.3f;
    [Range(0.05f, 0.5f)] public float MoveDelay = 0.2f;
    [Range(8f, 25f)] public float MaxDashSpeed = 12f;
    [Range(5f, 15f)] public float MaxFallSpeed = 7f;

    [Header("JUMP & AIR CONTROL")]
    [Range(8f, 20f)] public float JumpHeight = 12f;
    [Range(6f, 16f)] public float DoubleJumpHeight = 10f;
    [Range(-10f, 0f)] public float WallSlideSpeed = -2f;
    [Range(0.1f, 0.3f)] public float CoyoteTime = 0.2f;

    [Header("DASH & ROLL")]
    [Range(5f, 15f)] public float DashDistance = 7f;
    [Range(4f, 10f)] public float RollDistance = 5f;


    [Header("COMBAT PARAMETERS")]
    [Range(0.3f, 1.5f)] public float AttackComboDelay = 0.8f;
    [Range(2, 5)] public int MaxComboAttacks = 3;
    [Range(1f, 5f)] public float SuperAttackDuration = 3f;
    [Range(0.2f, 1f)] public float ParryDuration = 0.4f;

    [Header("STAMINA COSTS")]
    [Range(0f, 5f)] public float JumpStamina = 1f;
    [Range(3f, 10f)] public float DoubleJumpStamina = 6f;
    [Range(2f, 8f)] public float WallJumpStamina = 3f;
    [Range(2f, 8f)] public float RollStamina = 4f;
    [Range(2f, 10f)] public float ParryStamina = 3f;
    
    public float GetActualShieldedSpeed() => BaseSpeed * ShieldedSpeedMultiplier;
}
