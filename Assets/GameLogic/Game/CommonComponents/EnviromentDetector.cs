using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class EnvironmentDetector : MonoBehaviour
{
    [SerializeField] private LayerMask masks;
    [SerializeField] private bool needToCheckLedge;
    [SerializeField] private bool needToDrawGizmos;

    [Header("Raycast Settings")]
    [SerializeField] private float groundCheckDist = 0.25f;
    [SerializeField] private float wallCheckDist = 0.05f;
    [SerializeField] private Vector2 bottomOffset = Vector2.zero;

    // Reactive Properties
    private readonly BoolReactiveProperty _isGrounded = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _isTouchingWall = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _isLedgeBlocked = new BoolReactiveProperty();

    public IReadOnlyReactiveProperty<bool> IsGrounded => _isGrounded;
    public IReadOnlyReactiveProperty<bool> IsTouchingWall => _isTouchingWall;
    public IReadOnlyReactiveProperty<bool> IsLedgeBlocked => _isLedgeBlocked;

    public bool CanClimbLedge => _isTouchingWall.Value && !_isLedgeBlocked.Value;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private void Start()
    {
        this.FixedUpdateAsObservable()
            .Where(_ => isActiveAndEnabled)
            .Subscribe(_ => CheckEnvironment())
            .AddTo(_disposables);
    }

    private void CheckEnvironment()
    {
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 pos = (Vector2)transform.position;
        // Ground Check
        bool groundhit1 = Physics2D.Raycast(pos + bottomOffset, Vector2.down, groundCheckDist, masks);
        _isGrounded.Value = groundhit1;
        // Wall Check
        bool wallhit = Physics2D.Raycast(pos + new Vector2(0.09f * dir, -0.2f), Vector2.right * dir, wallCheckDist, masks);
        bool wall2hit = Physics2D.Raycast(pos + new Vector2(0.09f * dir, 0.11f), Vector2.right * dir, wallCheckDist, masks);
        _isTouchingWall.Value = wallhit || wall2hit;
        // Ledge Check
        if (needToCheckLedge)
        {
            bool hit = Physics2D.Raycast(pos + new Vector2(0.05f * dir, 0.25f), Vector2.right * dir, 0.2f, masks);
            _isLedgeBlocked.Value = hit;
        }
        else
        {
            _isLedgeBlocked.Value = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!needToDrawGizmos) return;

        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 pos = (Vector2)transform.position;

        Gizmos.color = _isGrounded.Value ? Color.green : Color.red;
        Gizmos.DrawRay(pos + bottomOffset , Vector2.down * groundCheckDist);

        Gizmos.color = _isTouchingWall.Value ? Color.green : Color.blue;
        Gizmos.DrawRay(pos + new Vector2(0.09f * dir, -0.2f), Vector2.right * dir * wallCheckDist);
        Gizmos.DrawRay(pos + new Vector2(0.09f * dir, 0.11f), Vector2.right * dir * wallCheckDist);

        if (needToCheckLedge)
        {
            Gizmos.color = _isLedgeBlocked.Value ? Color.red : Color.yellow;
            Gizmos.DrawRay(pos + new Vector2(0.05f * dir, 0.25f), Vector2.right * dir * 0.2f);
        }
    }
    void OnDestroy()
    {
        _disposables.Dispose();
    }
}

