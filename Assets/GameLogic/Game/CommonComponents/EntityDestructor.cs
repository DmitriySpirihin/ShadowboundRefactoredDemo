using UnityEngine;
using UniRx;
using Zenject;

[RequireComponent(typeof(SpriteRenderer))]
public class EntityDestructor : MonoBehaviour
{
    [Inject] IHealth _health;
    [Header("transform in childen that holds parts")]
    [SerializeField] private Transform _partsHolderTr;
    [Header("random forces ranges")]
    [SerializeField] private float minForceX;
    [SerializeField][Range(10f, 100f)] private float maxForceX, minForceY, maxForceY, minTorque, maxTorque;

    private bool wasDestructed;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        if (_health == null || _partsHolderTr == null)
        {
            Debug.LogWarning("IHealth component wasn't injected or holder Transform wasn't set, component is disabled");
            enabled = false;
            return;
        }
        _health.State.Where(v => v == GameEnums.HealthState.Dead).Subscribe(_ => Destruct()).AddTo(_disposables);
    }
    
    private void Destruct()
    {
        if( _partsHolderTr == null || _partsHolderTr.childCount == 0 || wasDestructed) return;
        float direction = transform.localScale.x;

        GetComponent<SpriteRenderer>().enabled = false;
        for (int i = 0; i < _partsHolderTr.childCount; i++)
        {
            Rigidbody2D rigidbody =  _partsHolderTr.GetChild(i).GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
               rigidbody.AddForce(new Vector2(Mathf.Sign(direction) * Random.Range(minForceX,maxForceX), Random.Range(minForceY,maxForceY)));
               rigidbody.AddTorque(Random.Range(minTorque,maxTorque));
            }
        }
        wasDestructed = true;
    }

    void OnDestroy()
    {
        _disposables.Dispose();
    }
    
}
