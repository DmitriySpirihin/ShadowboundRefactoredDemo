using System.Collections;
using UnityEngine;
using UniRx;
using Zenject;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteBloodStained : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites = new Sprite[4];
    [SerializeField] private float damageThreshold = 5f;
    [SerializeField] private float minRecoveryTime = 4f;
    [SerializeField] private float maxRecoveryTime = 9f;
    [Inject] private SpriteRenderer _spriteRenderer;
    [Inject] private IHealth _health;

    private int currentIndex;
    private float lastHealthValue;
    private float currentIndexer;
    private Coroutine countBackRoutine;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        if (_health == null || sprites.Length == 0)
        {
            Debug.LogWarning("Ihealth was not injected or sprites array is empty , component is disabled");
            this.enabled = false;
            return;
        }
        lastHealthValue = _health.MaxHealth.Value;
        _health.CurrentHealth.Subscribe(val => HandleHit(val)).AddTo(_disposables);
    }

    void HandleHit(float hp)
    {
        if (lastHealthValue - hp < damageThreshold)
        {
            lastHealthValue = hp;
            return;
        }
        lastHealthValue = hp;
        currentIndex = currentIndex < sprites.Length - 1 ? ++currentIndex : sprites.Length - 1;
        currentIndexer = Random.Range(minRecoveryTime, maxRecoveryTime);
        _spriteRenderer.sprite = sprites[currentIndex];
        if(countBackRoutine != null) StopCoroutine(countBackRoutine);
        countBackRoutine = StartCoroutine(CountBackRoutine());
    }
    
    private IEnumerator CountBackRoutine()
    {
        yield return new WaitForSeconds(currentIndexer);
        currentIndex = currentIndex > 0 ? --currentIndex : 0;
        _spriteRenderer.sprite = sprites[currentIndex];
        if (currentIndex > 0)
        {
            currentIndexer = Random.Range(minRecoveryTime, maxRecoveryTime);
            countBackRoutine = StartCoroutine(CountBackRoutine());
            yield break;
        }
    }

    void OnDestroy()
    {
        _disposables.Dispose();
        StopAllCoroutines();
    }
}
