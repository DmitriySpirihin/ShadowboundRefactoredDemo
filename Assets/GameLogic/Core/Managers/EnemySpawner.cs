using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;
using GameEnums;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyWaveConfigSO _waveConfig;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _needAutoSpawn = true;
    
    [Header("Pool Setup (drag prefabs in editor)")]
    [SerializeField] private List<GameObject> _initialPoolPrefabs = new();
    [SerializeField] private int _poolSizePerType = 5;

    // Runtime state
    private Dictionary<EnemyType, Queue<GameObject>> _pools = new();
    private Dictionary<EnemyType, int> _activeCounts = new();
    private Dictionary<EnemyType, EnemyWaveConfigSO.WaveEntry> _waveSettings = new();
    
    private int _currentWaveIndex = 0;
    private bool _isSpawningAllowed = true;
    private readonly CompositeDisposable _disposables = new();

    // Zenject DI
    [Inject] private DiContainer _container;

    private void Awake()
    {
        if (_spawnPoint == null)
            _spawnPoint = transform;

        InitializePools();
        SubscribeToAutoSpawn();
    }

    private void InitializePools()
    {
        // Pool for every enemy type 
        foreach (var entry in _waveConfig.Waves)
        {
            var pool = new Queue<GameObject>(_poolSizePerType);
            _pools[entry.EnemyType] = pool;
            _activeCounts[entry.EnemyType] = 0;
            _waveSettings[entry.EnemyType] = entry;

            // Preload objects without run time instantiations
            for (int i = 0; i < _poolSizePerType; i++)
            {
                GameObject instance = Instantiate(entry.Prefab, transform);
                instance.name = $"{entry.EnemyType}_Pooled_{i}";
                instance.SetActive(false);
                
                // Dependancy injection
                _container.Inject(instance);
                
                pool.Enqueue(instance);
            }
        }
    }

    private void SubscribeToAutoSpawn()
    {
        if (!_needAutoSpawn) return;

        // Auto spawn after delay
        Observable.Timer(System.TimeSpan.FromSeconds(_waveConfig.InitialSpawnDelay)).Subscribe(_ => StartAutoSpawnLoop()).AddTo(_disposables);
    }

    private void StartAutoSpawnLoop()
    {
        Observable.EveryUpdate().Where(_ => _isSpawningAllowed && CanSpawnNextEnemy()).First().Subscribe(_ => SpawnNextFromWave()).AddTo(_disposables);
    }

    private bool CanSpawnNextEnemy()
    {
        if (_waveConfig.Waves.Count == 0) return false;
        
        var currentWave = _waveConfig.Waves[_currentWaveIndex];
        int active = _activeCounts.GetValueOrDefault(currentWave.EnemyType, 0);
        
        return active < currentWave.MaxAliveAtOnce && _pools[currentWave.EnemyType].Count > 0;
    }

    private void SpawnNextFromWave()
    {
        var currentWave = _waveConfig.Waves[_currentWaveIndex];
        SpawnEnemy(currentWave.EnemyType, _spawnPoint.position);
        
        // After spawn await enemy's dead with delay
        _isSpawningAllowed = false;
        Observable.Timer(System.TimeSpan.FromSeconds(currentWave.SpawnDelayAfterDeath))
            .Subscribe(_ => _isSpawningAllowed = true)
            .AddTo(_disposables);
    }

    // Manual pooling
    public GameObject SpawnEnemy(EnemyType type, Vector3 position)
    {
        if (!_pools.TryGetValue(type, out var pool) || pool.Count == 0)
        {
            Debug.LogWarning($"No available {type} in pool! Pool size: {_poolSizePerType}, Active: {_activeCounts.GetValueOrDefault(type, 0)}");
            return null;
        }
        GameObject instance = pool.Dequeue();
        instance.transform.position = position;
        instance.SetActive(true);
        
        // Subscription to return in pool when is dead
        var health = instance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.State.Where(state => state == HealthState.Dead).First().Subscribe(_ => ReturnToPool(instance, type)).AddTo(instance);
        }

        _activeCounts[type] = _activeCounts.GetValueOrDefault(type, 0) + 1;
        return instance;
    }

    public GameObject SpawnEnemy(EnemyType type, float radius = 3f)
    {
        return SpawnEnemy(type, _spawnPoint.position);
    }

    private void ReturnToPool(GameObject enemy, EnemyType type)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(transform);
        
        _pools[type].Enqueue(enemy);
        _activeCounts[type]--;
        
        // if spawn is active resume
        if (_needAutoSpawn && _isSpawningAllowed) StartAutoSpawnLoop();
    }

    public void PauseAutoSpawn() => _isSpawningAllowed = false;

    public void ResumeAutoSpawn() => _isSpawningAllowed = true;

    [ContextMenu("Debug Pool Status")]
    private void DebugPoolStatus()
    {
        Debug.Log("=== ENEMY POOL STATUS ===");
        foreach (var kvp in _pools)
        {
            Debug.Log($"{kvp.Key}: Active={_activeCounts[kvp.Key]}, Available={kvp.Value.Count}/{_poolSizePerType}");
        }
    }

    private void OnDestroy() => _disposables.Clear();
}
