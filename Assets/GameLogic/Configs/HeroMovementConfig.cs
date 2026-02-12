using UnityEngine;

[CreateAssetMenu(fileName = "HeroMovementConfig", menuName = "Scriptable Objects/HeroMovementConfig")]
public class HeroMovementConfig : ScriptableObject
{
    [Header("Movement settings")]
    public float speed = 5f;
    public float shieldedSpeedMultiplier = 0.3f;
    public float moveDelay = 0.2f;

    [Header("Jump settings")]
    public float jumpHeight = 12f;
    public float doubleJumpHeight = 10f;
    public float wallSlideSpeed = -2f;

    [Header("Abilities")]
    public float dashDistance = 7f;
    public float rollDistance = 5f;
}
