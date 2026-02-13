using UnityEngine;
using GameEnums;
using System.Collections;
using System.Collections.Generic;
using Zenject;
using Unity.Cinemachine;

public class CombatSystem : MonoBehaviour
{
    [Inject] private ConcentrationSystem _concentrationSystem;
    [Inject] private IAnimationService _animationService;
    [Inject] private Animator _animator;
    // I've made the system where everyone can damage everyone by design
    [Header("Variables")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float baseDamage;
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private float succsessHitDelay = 1f;
    [Range(10f, 80f)][SerializeField] private float reduceConcentrationAmount = 25f;
    [Range(1f, 20f)][SerializeField] private float baseRestoreConcentrationAmount = 10f;
    [SerializeField] private int successHitCounter;
    [SerializeField] private int maxAttackNum;
    
    [Header("Raycast settings")]
    [SerializeField] private LayerMask mask;
    [SerializeField] private float raySize;
    [SerializeField] private Vector2 rayOffset;

    [Header("Debug")]
    [SerializeField] private bool needToDrawGizmos;
    [SerializeField] private SlashType debugPreviewSlashType = SlashType.Slash;

    // for enemies or objects those already being hit
    private HashSet<IDestructable> _enemiesSet = new HashSet<IDestructable>();
    private Coroutine _resetCounterRoutine;

    //pre compile vectors hash set
    private Dictionary<SlashType, Vector2[]> _slashVectors = new Dictionary<SlashType, Vector2[]>()
    {
       { SlashType.Pierce, new Vector2[] { new Vector2(1, 0) } },
       { SlashType.Slash, new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)} },
       { SlashType.SemiCircleSlash, new Vector2[] { new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)} },
       { SlashType.Circle, new Vector2[] {new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1)}},
    };

    public void Hit(SlashType slashType)
    {
        _enemiesSet.Clear();
        if (_slashVectors.TryGetValue(slashType, out Vector2[] vectors))
        {
            if (vectors.Length > 0)
            {
                for (int i = 0; i < vectors.Length; i++)
                {
                    RaycastHit2D hit = Raycast(vectors[i]);
                    if (hit.collider != null)
                    {
                        IDestructable destructable = hit.transform.GetComponent<IDestructable>();
                        if (destructable != null && !_enemiesSet.Contains(destructable))
                        {
                            _enemiesSet.Add(destructable);

                            bool isCrit = successHitCounter > 0 && Random.Range(0, 10) < (2 + successHitCounter);

                            DamageData damData = new DamageData(GetDamage(isCrit), (int)Mathf.Sign(transform.localScale.x), isCrit, weaponType);

                            if (hit.transform.TryGetComponent<IHealth>(out IHealth health))
                            {
                               // first bool if hit registered , second if hit was parried
                               (bool, bool) hitResult = health.TakeDamage(damData);
                               if (hitResult.Item2) // parried
                               {
                                  _animationService.SetState(_animator, AnimStates.Staggered, true);
                                  _concentrationSystem.ReduceConcentration(reduceConcentrationAmount);
                                  successHitCounter = 0;
                                  return;
                               }
                               else if (hitResult.Item1 && successHitCounter < 5)
                                {
                                    _concentrationSystem.RestoreConcentration(baseRestoreConcentrationAmount * successHitCounter);
                                    successHitCounter++;
                                }
                               else successHitCounter = 0;                  
                            }
                            else destructable.Destruct(damData);
                        }
                    }
                }
            }
        }
        else Debug.LogWarning("No value in slashVectors");
        
        // Start/reset counter reset timer
        if (_resetCounterRoutine != null) StopCoroutine(_resetCounterRoutine);
        _resetCounterRoutine = StartCoroutine(ResetSuccsessHitCounter());
    }

    private IEnumerator ResetSuccsessHitCounter()
    {
        yield return new WaitForSeconds(succsessHitDelay);
        successHitCounter = 0;
        _resetCounterRoutine = null;
    }

    private RaycastHit2D Raycast(Vector2 rayDirection)
    {
       float facing = Mathf.Sign(transform.localScale.x);
       Vector2 worldDir = new Vector2(rayDirection.x * facing, rayDirection.y);
       Vector3 origin = transform.position + transform.right * (rayOffset.x * facing) + transform.up * rayOffset.y;
    
       return Physics2D.Raycast(origin, worldDir, raySize, mask);
    }

    private float GetDamage(bool isCrit)
    {
        multiplier = 1f + successHitCounter / 10f;
        float randomizer = Random.Range(0.8f, 1.1f);

        return isCrit ? baseDamage * 2f * randomizer : baseDamage * multiplier * randomizer;
    }

    // vusual feedback if parry registerd
    private void SetParryFeedback()
    {
        
    }

    // for animator event
    public void GetNextAttackNum() => Random.Range(0, maxAttackNum);
    
    // debug
    private void OnDrawGizmos()
    {
        if (!needToDrawGizmos) return;
        DrawSlashGizmos(debugPreviewSlashType);
    }

    private void DrawSlashGizmos(SlashType slashType)
    {
        if (!_slashVectors.TryGetValue(slashType, out Vector2[] vectors)) return;
        
        float facing = Mathf.Sign(transform.localScale.x);
        Vector3 origin = transform.position + transform.right * (rayOffset.x * facing) + transform.up * rayOffset.y;
        
        Gizmos.color = Color.cyan;
        for (int i = 0; i < vectors.Length; i++)
        {
            Vector2 worldDir = new Vector2(vectors[i].x * facing, vectors[i].y).normalized;
            Vector3 endPoint = origin + (Vector3)(worldDir * raySize);
            Gizmos.DrawLine(origin, endPoint);
        }
    }
}
