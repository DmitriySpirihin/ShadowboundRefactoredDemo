using UnityEngine;
using UniRx;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class ParrySystem : MonoBehaviour
{
    private readonly ReactiveProperty<bool> _isParrying = new ReactiveProperty<bool>(); 
    public IReadOnlyReactiveProperty<bool> IsParrying => _isParrying;

    private Coroutine _stopParryRoutine;

    public void EnableParry() => _isParrying.Value = true;
    public void DisableParry() => _isParrying.Value = false;
    public void StartParry(float duration)
    {
        _isParrying.Value = true;
        if (_stopParryRoutine != null) StopCoroutine(_stopParryRoutine);
        _stopParryRoutine = StartCoroutine(StopParryRoutine(duration));
    }

    private IEnumerator StopParryRoutine(float durastion)
    {
        yield return new WaitForSeconds(durastion);
        _isParrying.Value = false;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}