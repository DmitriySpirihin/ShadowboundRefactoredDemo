using UnityEngine;
using UniRx;

[RequireComponent(typeof(Animator))]
public class ParrySystem : MonoBehaviour
{
    private readonly ReactiveProperty<bool> _isParrying = new ReactiveProperty<bool>(); 
    public IReadOnlyReactiveProperty<bool> IsParrying => _isParrying;

    public void EnableParry() => _isParrying.Value = true;
    public void DisableParry() => _isParrying.Value = false;
}