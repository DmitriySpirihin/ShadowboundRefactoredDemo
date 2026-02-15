using UnityEngine;
using UniRx;
using System;

public class HeroDetector : MonoBehaviour 
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask _heroLayer;
    
    [Header("Raycast Parameters")]
    [SerializeField][Range(-4f, 10f)] private float _rayOffsetX;
    [SerializeField][Range(-4f, 10f)] private float _rayOffsetY;
    [SerializeField][Range(-4f, 10f)] private float _attackOffsetX;
    [SerializeField][Range(-4f, 10f)] private float _attackOffsetY;
    [SerializeField][Range(0.1f, 10f)] private float _attackRaySize;
    
    [Header("Debug")]
    [SerializeField] private bool _drawDebugRays = true;

    // Reactive state
    private readonly ReactiveProperty<bool> _isHeroDetected = new ReactiveProperty<bool>(false);
    private readonly ReactiveProperty<bool> _canAttack = new ReactiveProperty<bool>(false);
    
    public IReadOnlyReactiveProperty<bool> IsHeroDetected => _isHeroDetected;
    public IReadOnlyReactiveProperty<bool> CanAttack => _canAttack;

    private Animator _animator;
    private Transform _parentTransform;
    private RaycastHit2D _attackHit;

    private void Awake()
    {
        _parentTransform = transform.parent;
        _animator = _parentTransform.GetComponent<Animator>();
        _isHeroDetected.DistinctUntilChanged().Subscribe(value => _animator.SetBool("SeeHero", value)).AddTo(this); 
    }

    private void FixedUpdate()
    {
        if (_isHeroDetected.Value) PerformAttackRaycast();
        else  _canAttack.Value = false;

        if (_drawDebugRays) DebugDrawRays();
    }

    private void PerformAttackRaycast()
    {
        Vector2 rayOrigin = (Vector2)_parentTransform.position + new Vector2(_attackOffsetX * _parentTransform.localScale.x, _attackOffsetY);
        Vector2 rayDirection = Vector2.right * _parentTransform.localScale.x;
        
        _attackHit = Physics2D.Raycast(rayOrigin, rayDirection, _attackRaySize * Mathf.Abs(_parentTransform.localScale.x), _heroLayer);
        _canAttack.Value = _attackHit;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _isHeroDetected.Value = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _isHeroDetected.Value = false;
        _canAttack.Value = false;
    }


    public void SetAttackRaySize(float size)
    {
        _attackRaySize = size;
        _attackOffsetX = -size / 2f;
    }

    #if UNITY_EDITOR
    private void DebugDrawRays()
    {
        if (_isHeroDetected.Value)
        {
            Vector2 attackOrigin = (Vector2)_parentTransform.position + new Vector2(_attackOffsetX * _parentTransform.localScale.x, _attackOffsetY);
            Vector2 direction = Vector2.right * _parentTransform.localScale.x;
            
            Debug.DrawRay(attackOrigin, direction * _attackRaySize, _attackHit ? Color.green : Color.red);
        }
    }
    #endif
}
