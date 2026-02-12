using UnityEngine;

[CreateAssetMenu(fileName = "HealthConfig", menuName = "Configs/Health")]
public abstract class HealthConfigSO : ScriptableObject
{
    [Header("Health")]
    public int baseMaxHealth = 50;
    public int healthPerLevel = 25;
    public int maxHealthCap = 175;
    
    [Header("Temp Health")]
    public float baseBleedSpeed = 18f;
    public float slowBleedThreshold = 2f;
    public float slowBleedSpeed = 0.5f;
    
    [Header("Death & Feedback")]
    public float deathSlowMoDuration = 0.2f;
    public float deathSlowMoIntensity = 0.2f;
    public float slowMoPower = 0.8f;
    public float cameraSize = 1.2f;
    public int shakeDuration = 6;
    public float staggerDuration = 2.2f;
}

[CreateAssetMenu(fileName = "PlayerHealthConfig", menuName = "Configs/Health/Player", order = 0)]
public class PlayerHealthConfigSO : HealthConfigSO
{
    [Header("Player Settings")]
    public float concentrationDecaySpeed = 15f;
    public float concentrationStaggerThreshold = 100f;
    public float shieldBasePower = 0.3f;
    public float shieldReductionPerLevel = 0.05f;
    public float minShieldPower = 0.1f;
    public float armoredShieldReduction = 0.4f;
}

[CreateAssetMenu(fileName = "EnemyHealthConfig", menuName = "Configs/Health/Enemy", order = 1)]
public class EnemyHealthConfigSO : HealthConfigSO
{
    [Header("Enemy Settings")]
    public float staggerThresholdMultiplier = 0.3f; 
    public bool canBeExecuted = true;
    public float executeThreshold = 0.2f; 
    public ParticleSystem deathParticles;
}