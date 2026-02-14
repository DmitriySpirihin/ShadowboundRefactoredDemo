using UnityEngine;
using GameEnums;
using UniRx;

public interface IHealth
{
    IReadOnlyReactiveProperty<float> CurrentHealth { get; }
    IReadOnlyReactiveProperty<float> MaxHealth { get; }
    IReadOnlyReactiveProperty<float> CurrentTempHealth { get; }
    IReadOnlyReactiveProperty<HealthState> State { get; }
    IReadOnlyReactiveProperty<bool> IsLowHealth { get; }
    
    (bool, bool) TakeDamage(DamageData damageData);
    void Heal(int amount);
}

public interface IDestructable
{
    void Destruct(DamageData damData);
}

public interface IStamina
{
    public IReadOnlyReactiveProperty<float> CurrentStamina{get; }
    public void ReduceStamina(float amount);
}


public interface IAnimationService
{
    void SetState(Animator animator, AnimStates state, bool value);
    void SetTrigger(Animator animator, AnimTriggers trigger);
    public void SetValue(Animator animator, AnimValues state, float value, int num);
}