using System.Collections.Generic;
using UnityEngine;
using GameEnums;

[CreateAssetMenu(fileName = "EnemyWaveConfig", menuName = "Configs/EnemyWave")]
public class EnemyWaveConfigSO : ScriptableObject
{
    [System.Serializable]
    public struct WaveEntry
    {
        public EnemyType EnemyType;
        public GameObject Prefab;
        public int MaxAliveAtOnce;
        public float SpawnDelayAfterDeath; // Задержка после смерти перед новым спавном
        [Range(0f, 5f)] public float RandomDelayVariance;
    }

    public List<WaveEntry> Waves = new();
    public bool LoopWaves = true;
    public float InitialSpawnDelay = 2f;
}