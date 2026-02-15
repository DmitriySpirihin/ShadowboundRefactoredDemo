using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LocalAudioManager : MonoBehaviour
{
    [SerializeField] private AudioData[] _audioBank = new AudioData[0];
    private Dictionary<string, AudioData> _hashedBank = new Dictionary<string, AudioData>();
    [Inject] private GameData _gameData;
    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
       if (_audioBank.Length == 0) return;
       
       for (int i = 0; i < _audioBank.Length; i++) _hashedBank.Add(_audioBank[i].Name, _audioBank[i]);
    }

    public void PlaySound(string name)
    {
        if (_hashedBank.Count == 0) return;
        if (_hashedBank.TryGetValue(name, out AudioData data))
        {
            _audioSource.PlayOneShot(data.Clip, data.Volume * _gameData.SoundVolume.Value);
        }
    }
}
[System.Serializable]
public class AudioData
{
    public string Name;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 0.7f;
}