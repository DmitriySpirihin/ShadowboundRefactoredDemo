[CreateAssetMenu(fileName = "EnemyMovementConfig", menuName = "Configs/EnemyMovement")]
public class EnemyMovementConfigSO : ScriptableObject
{
    // Patrol
    public float PatrolSpeed = 2.5f;
    public float PatrolRadius = 4f;
    public float PatrolArrivalThreshold = 0.8f;
    
    // Chase
    public float ChaseSpeed = 4f;
    public float AggroRange = 10f;
    public float AttackRange = 2.5f;
    
    // Physics
    public float MaxFallSpeed = 15f;
    public float MaxDashSpeed = 12f;
    public float JumpHeight = 8f;
    public float JumpStaminaCost = 15f;
    
    // Obstacle detection
    public float LedgeCheckOffset = 0.7f;
    public float LedgeCheckDistance = 0.8f;
    public float WallCheckDistance = 0.9f;
    public LayerMask GroundLayer;
    
    // Combat
    public float AttackStaminaCost = 20f;
    public float AttackDuration = 0.6f;
    public float AttackLockDuration = 0.4f;
    
    // State machine
    public float StateTransitionDelay = 0.2f;
    public float StunDuration = 1f;
    
    // Recovery
    public float ImpulseRecoveryTime = 0.5f;
}