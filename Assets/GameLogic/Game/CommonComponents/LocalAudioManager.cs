using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LocalAudioManager : MonoBehaviour
{
    [SerializeField] private AudioData[] _audioBank = new AudioData[0];
    private Dictionary<string, AudioData> _hashedBank = new Dictionary<string, AudioData>();

    [Inject] private AudioManager _audioManager;
    [Inject] private AudioSource _audioSource;

    void Awake()
    {
       if (_audioBank.Length == 0) return;
       
       for (int i = 0; i < _audioBank.Length; i++) _hashedBank.Add(_audioBank[i].Name, _audioBank[i]);
    }

    public void PlaySound(string name)
    {
        if (_hashedBank.Count == 0) return;
        if (_hashedBank.TryGetValue(name, out AudioData data))
        {
            _audioManager.PlayCustomSound(_audioSource, data.Clip, data.Volume);
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