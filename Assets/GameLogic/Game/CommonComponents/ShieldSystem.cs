using UnityEngine;
using UniRx;

[RequireComponent(typeof(Animator))]
public class ShieldSystem : MonoBehaviour
{
    private readonly ReactiveProperty<bool> _isShielded = new ReactiveProperty<bool>(); 
    public IReadOnlyReactiveProperty<bool> IsShielded => _isShielded;

    public void LiftShield(bool value) => _isShielded.Value = value;
}
