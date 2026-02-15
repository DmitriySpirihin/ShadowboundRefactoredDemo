using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig", order = 0)]
public class GameConfigSO : ScriptableObject
{
    [Header("Audio")]
    public float defaultSoundVolume = 1f;
    public float defaultMusicVolume = 1f;
    
    [Header("Feedback")]
    public bool defaultNeedVibration = true;
    public bool defaultNeedCameraShake = true;
    public bool defaultNeedSlowMo = true;
    public bool defaultNeedBlood = true; 
    
    [Header("Gameplay")]
    public int defaultLocalization = 1;
    public int defaultDifficulty = 2;
    public int defaultStartingLevel = 1;
    public int defaultStartingCoins = 0;
    
    [Header("Progression")]
    public int defaultSwordLevel = 0;
    public int defaultArmorLevel = 0;
    public int defaultHealthLevel = 5;
    public int defaultStaminaLevel = 3;
    
    private void OnValidate()
    {
        defaultSoundVolume = Mathf.Clamp01(defaultSoundVolume);
        defaultMusicVolume = Mathf.Clamp01(defaultMusicVolume);
        defaultDifficulty = Mathf.Clamp(defaultDifficulty, 1, 3);
    }
}