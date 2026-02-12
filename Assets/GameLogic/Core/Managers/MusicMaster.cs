using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniRx;
using Zenject;

public class MusicMaster : MonoBehaviour, IDisposable
{
    [SerializeField] private AssetReference[] musicRefs;
    [SerializeField] private float crossfadeDuration = 1f;
    
    private AudioSource _source;
    [SerializeField] private AudioClip _currentClip;
    private Coroutine _crossfadeRoutine,_waitForEndRoutine;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    [Inject] private GameData _gameData;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.loop = true;
        // Подписка на изменение громкости музыки
        _gameData.MusicVolume
            .Subscribe(OnMusicVolumeChanged)
            .AddTo(_disposables);
       
        StartCoroutine(LoadAndPlayClip(musicRefs[UnityEngine.Random.Range(0,musicRefs.Length)]));
    }
    // Плавное изменение громкости без лишних проверок в Update
    private void OnMusicVolumeChanged(float targetVolume)
    {
        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);
        _crossfadeRoutine = StartCoroutine(CrossfadeVolume(targetVolume));
    }
    
    private IEnumerator CrossfadeVolume(float targetVolume)
    {
        float startVolume = _source.volume;
        float elapsed = 0f;
        
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);
            _source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        _source.volume = targetVolume;
 
        if (targetVolume <= 0 && _source.isPlaying)
            _source.Pause();
        else if (targetVolume > 0f && !_source.isPlaying && _currentClip != null)
            _source.Play();
    }
    
    private IEnumerator LoadAndPlayClip(AssetReference clipRef)
    {
        // Отмена предыдущей загрузки
        if (_currentClip != null)
        {
            Addressables.Release(_currentClip);
            _currentClip = null;
        }
        
        // Загрузка нового клипа
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(clipRef);
        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _currentClip = handle.Result;
            _source.clip = _currentClip;
            
            // Плавный кроссфейд к новому треку
            float targetVolume = _gameData.MusicVolume.Value;
            _source.volume = 0f;
            _source.Play();
            
            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            if (_waitForEndRoutine != null) StopCoroutine(_waitForEndRoutine);
            
            _crossfadeRoutine = StartCoroutine(CrossfadeVolume(targetVolume));
            _waitForEndRoutine = StartCoroutine(WaitForClipEnd(_currentClip));
        }
        else Addressables.Release(handle);
        
    }

    private IEnumerator WaitForClipEnd(AudioClip clip)
    {
        // ждём до конца трека с небольшим буфером
        yield return new WaitForSeconds(clip.length - 0.1f);
        
        while (_source.isPlaying && _source.time >= clip.length - 0.1f * 2f)
        {
            yield return null;
        }
        
        if (_source.isPlaying == false) StartCoroutine(LoadAndPlayClip(musicRefs[UnityEngine.Random.Range(0,musicRefs.Length)]));
        
    }

    
    // выгрузка ресурсов 
    private void OnDestroy()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
        
        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);
        
       if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);
        
        _source.Stop();
        
        if (_currentClip != null)
        {
            Addressables.Release(_currentClip);
            _currentClip = null;
        }
        
        _source = null;
        _gameData = null;
    }
}