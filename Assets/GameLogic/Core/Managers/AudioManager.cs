using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniRx;
using Zenject;
using GameEnums;
using Cysharp.Threading.Tasks;

public class AudioManager :  MonoBehaviour, IAudioService, IDisposable
{
    // weapon sounds
    [System.Serializable]
    public class SoundBank
    {
        [Tooltip("Weapon type")]
        public WeaponType key;
        
        [Tooltip("Audio clips array")]
        public AssetReference[] sounds;
    }

    [Header("Hit Sounds")]
    [SerializeField] private SoundBank[] soundBanks;
    private Dictionary<WeaponType, AssetReference[]> _soundDictionary = new Dictionary<WeaponType, AssetReference[]>();
    private Dictionary<WeaponType, AudioClip[]> _loadedClips  = new Dictionary<WeaponType, AudioClip[]>();
    private readonly HashSet<WeaponType> _loadingLock = new HashSet<WeaponType>();
    
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private GameData _gameData;

    [Inject]
    public void Construct(GameData gameData)
    {
       _gameData = gameData ?? throw new ArgumentNullException(nameof(gameData));
    }
    
    private void Awake()
    {
        BuildSoundDictionary();
    }

    public void PlayHitSound(AudioSource source, WeaponType weaponType, float volumeMultiplier = 1f)
    {
       LoadAndPlayClip(source, weaponType, volumeMultiplier).Forget();
    }

    public void PlayCustomSound(AudioSource source, AudioClip clip, float volumeMultiplier)
    {
        source.PlayOneShot(clip, _gameData.SoundVolume.Value * Mathf.Min(volumeMultiplier, 1f));
    }
    
    private async UniTaskVoid LoadAndPlayClip(AudioSource source, WeaponType weaponType, float volumeMultiplier)
    {
       if (source == null) return;
       if (!_soundDictionary.TryGetValue(weaponType, out var assetRefs)) return;
    
      if (!_loadedClips.TryGetValue(weaponType, out var loadedClips))
      {
        // prevent parralell loads
        if (_loadingLock.Contains(weaponType)) return;
        _loadingLock.Add(weaponType);
        
          try
          {
             loadedClips = await LoadSoundBankAsync(assetRefs);
             if (loadedClips == null || loadedClips.Length == 0) return;
             _loadedClips[weaponType] = loadedClips;
          }
          finally
          {
            _loadingLock.Remove(weaponType);
          }
       }
    
      // play random sound
       if (loadedClips != null && loadedClips.Length > 0)
       {
          AudioClip clip = loadedClips[UnityEngine.Random.Range(0, loadedClips.Length)];
          source.PlayOneShot(clip, _gameData.SoundVolume.Value * Mathf.Min(volumeMultiplier, 1f));
       }
    }

    private async UniTask<AudioClip[]> LoadSoundBankAsync(AssetReference[] assetReferences)
    {
        if (assetReferences == null || assetReferences.Length == 0) return Array.Empty<AudioClip>();
    
        var handles = new List<AsyncOperationHandle<AudioClip>>();
        var clips = new List<AudioClip>();
        
        try
        {
            foreach (var assetRef in assetReferences)
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(assetRef);
                handles.Add(handle);
                await handle.Task;
                
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null) 
                    clips.Add(handle.Result); 
                else 
                    Debug.LogWarning($"Failed to load sound: {assetRef.RuntimeKey}");
            }
            
            return clips.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Loading failed: {ex.Message}");
            foreach (var handle in handles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            return Array.Empty<AudioClip>();
        }
    }

    private void BuildSoundDictionary()
    {
        _soundDictionary = new Dictionary<WeaponType, AssetReference[]>();
        
        foreach (var entry in soundBanks)
        {
            if (_soundDictionary.ContainsKey(entry.key)) continue;
            _soundDictionary[entry.key] = entry.sounds;
        }
    }
    
    private void OnDestroy()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
        
        if (_loadedClips != null)
        {
            foreach (var clips in _loadedClips.Values)
            { 
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null) Addressables.Release(clips[i]);
                }
            }
            _loadedClips.Clear();
        }
        
        // cancel activ loading
        foreach (var weaponType in _loadingLock)
        {
            if (_soundDictionary.TryGetValue(weaponType, out var refs))
            {
                foreach (var assetRef in refs)
                {
                    Addressables.Release(assetRef);
                }
            }
        }
        _loadingLock.Clear();
    
        _gameData = null;
    }
}

public interface IAudioService
{
    public void PlayHitSound(AudioSource source, WeaponType weaponType, float volumeMultiplier = 1f);
    public void PlayCustomSound(AudioSource source, AudioClip clip, float volumeMultiplier);
}
