using GameEnums;
using UniRx;
using System;

public class GameData : IDisposable
{
    // основные настр.
    public readonly ReactiveProperty<int> Localization = new ReactiveProperty<int>();
    public readonly ReactiveProperty<float> SoundVolume = new ReactiveProperty<float>();
    public readonly ReactiveProperty<float> MusicVolume = new ReactiveProperty<float>();
    //настройки UX
    public readonly ReactiveProperty<bool> NeedVibration = new ReactiveProperty<bool>();
    public readonly ReactiveProperty<bool> NeedCameraShake = new ReactiveProperty<bool>();
    public readonly ReactiveProperty<bool> NeedSlowMo = new ReactiveProperty<bool>();
    public readonly ReactiveProperty<bool> NeedBlood = new ReactiveProperty<bool>();
    public readonly ReactiveProperty<bool> NeedDamageNums = new ReactiveProperty<bool>();
    public readonly ReactiveProperty<int> Difficulty = new ReactiveProperty<int>();
    //уровень
    public readonly ReactiveProperty<int> CurrentLevel = new ReactiveProperty<int>();
    //снаряжение
    public readonly ReactiveProperty<int> Coins = new ReactiveProperty<int>();
    public readonly ReactiveProperty<int> SwordLevel = new ReactiveProperty<int>();
    public readonly ReactiveProperty<int> ArmorLevel = new ReactiveProperty<int>();
    //статистика
    public readonly ReactiveProperty<int> TotalHits = new ReactiveProperty<int>();
    public readonly ReactiveProperty<int> TotalDamageDealt = new ReactiveProperty<int>();
    public readonly ReactiveProperty<int> TotalDamageTaken = new ReactiveProperty<int>();
    

    // просто на всякий случай , в основной игре нет паузы
    public readonly ReactiveProperty<GameState> Gamestate = new ReactiveProperty<GameState>(GameState.Running);
    
    
    //отписки
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    //конструктор
    public GameData(GameConfigSO config)
    {
        Initialize(config);
        
       //можно загрузить данные из сейва
    }
    
    private void Initialize(GameConfigSO config)
    {
        Localization.Value = config.defaultLocalization;
        SoundVolume.Value = config.defaultSoundVolume;
        MusicVolume.Value = config.defaultMusicVolume;
        NeedVibration.Value = config.defaultNeedVibration;
        NeedCameraShake.Value = config.defaultNeedCameraShake;
        NeedSlowMo.Value = config.defaultNeedSlowMo;
        NeedBlood.Value = config.defaultNeedBlood;
        Difficulty.Value = config.defaultDifficulty;
        CurrentLevel.Value = config.defaultStartingLevel;
        Coins.Value = config.defaultStartingCoins;
        SwordLevel.Value = config.defaultSwordLevel;
        ArmorLevel.Value = config.defaultArmorLevel;
    }
    
    private void LoadSave()
    {
      
    }
    public void SaveGame()
    {
      
    }
    
    
    public SaveData ToSaveData()
    {
        return new SaveData
        {
            localization = Localization.Value,
        };
    }
    
    public void Dispose() => _disposables.Dispose();
}

// для сейва
[System.Serializable]
public class SaveData
{
    public int? localization;
    // ......
}