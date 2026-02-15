using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;
using GameEnums;

[RequireComponent(typeof(EnemyWaveConfigSO))]
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
    private Dictionary<EnemyType, WaveConfigSO.WaveEntry> _waveSettings = new();
    
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
        // Создаём пул для каждого типа из конфига
        foreach (var entry in _waveConfig.Waves)
        {
            var pool = new Queue<GameObject>(_poolSizePerType);
            _pools[entry.EnemyType] = pool;
            _activeCounts[entry.EnemyType] = 0;
            _waveSettings[entry.EnemyType] = entry;

            // Предварительное создание объектов БЕЗ инстанцирования в рантайме
            for (int i = 0; i < _poolSizePerType; i++)
            {
                GameObject instance = Instantiate(entry.Prefab, transform);
                instance.name = $"{entry.EnemyType}_Pooled_{i}";
                instance.SetActive(false);
                
                // Регистрация в контейнере для DI (Zenject)
                _container.Inject(instance);
                
                pool.Enqueue(instance);
            }
        }
    }

    private void SubscribeToAutoSpawn()
    {
        if (!_needAutoSpawn) return;

        // Автоспавн стартует после задержки
        Observable.Timer(System.TimeSpan.FromSeconds(_waveConfig.InitialSpawnDelay))
            .Subscribe(_ => StartAutoSpawnLoop())
            .AddTo(_disposables);
    }

    private void StartAutoSpawnLoop()
    {
        Observable.EveryUpdate()
            .Where(_ => _isSpawningAllowed && CanSpawnNextEnemy())
            .First() // Спавним ОДИН враг за раз
            .Subscribe(_ => SpawnNextFromWave())
            .AddTo(_disposables);
    }

    private bool CanSpawnNextEnemy()
    {
        if (_waveConfig.Waves.Count == 0) return false;
        
        var currentWave = _waveConfig.Waves[_currentWaveIndex];
        int active = _activeCounts.GetValueOrDefault(currentWave.EnemyType, 0);
        
        // Спавним ТОЛЬКО если есть свободные слоты в пуле
        return active < currentWave.MaxAliveAtOnce && _pools[currentWave.EnemyType].Count > 0;
    }

    private void SpawnNextFromWave()
    {
        var currentWave = _waveConfig.Waves[_currentWaveIndex];
        SpawnEnemy(currentWave.EnemyType, _spawnPoint.position);
        
        // После спавна ждём смерти + задержку перед следующим
        _isSpawningAllowed = false;
        Observable.Timer(System.TimeSpan.FromSeconds(currentWave.SpawnDelayAfterDeath))
            .Subscribe(_ => _isSpawningAllowed = true)
            .AddTo(_disposables);
    }

    // === Публичный API ===

    /// <summary>
    /// Ручной спавн врага в указанной позиции
    /// </summary>
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
        
        // Подписка на смерть для возврата в пул
        var health = instance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.State
                .Where(state => state == HealthState.Dead)
                .First() // Только один раз
                .Subscribe(_ => ReturnToPool(instance, type))
                .AddTo(instance); // Отписка при уничтожении объекта
        }

        _activeCounts[type] = _activeCounts.GetValueOrDefault(type, 0) + 1;
        return instance;
    }

    /// <summary>
    /// Спавн врага с рандомной позицией вокруг спавнпоинта
    /// </summary>
    public GameObject SpawnEnemy(EnemyType type, float radius = 3f)
    {
        Vector3 randomOffset = Random.insideUnitCircle * radius;
        randomOffset.z = 0f; // 2D
        return SpawnEnemy(type, _spawnPoint.position + randomOffset);
    }

    // === Возврат в пул ===

    private void ReturnToPool(GameObject enemy, EnemyType type)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(transform); // Возврат в иерархию спавнера
        
        _pools[type].Enqueue(enemy);
        _activeCounts[type]--;
        
        // Если автоспавн активен — продолжаем цикл
        if (_needAutoSpawn && _isSpawningAllowed)
            StartAutoSpawnLoop();
    }

    // === Управление волнами ===

    /// <summary>
    /// Принудительно завершить текущую волну и перейти к следующей
    /// </summary>
    public void AdvanceToNextWave()
    {
        _currentWaveIndex = (_currentWaveIndex + 1) % _waveConfig.Waves.Count;
        _isSpawningAllowed = true;
        StartAutoSpawnLoop();
    }

    /// <summary>
    /// Остановить автоспавн (например, при паузе)
    /// </summary>
    public void PauseAutoSpawn() => _isSpawningAllowed = false;

    /// <summary>
    /// Возобновить автоспавн
    /// </summary>
    public void ResumeAutoSpawn() => _isSpawningAllowed = true;

    // === Отладка ===

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
