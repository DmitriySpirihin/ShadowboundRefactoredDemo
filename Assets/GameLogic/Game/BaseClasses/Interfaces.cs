using GameEnums;
using UniRx;

public interface IHealth
{
    IReadOnlyReactiveProperty<float> CurrentHealth { get; }
    IReadOnlyReactiveProperty<float> MaxHealth { get; }
    IReadOnlyReactiveProperty<float> CurrentTempHealth { get; }
    IReadOnlyReactiveProperty<HealthState> State { get; }
    IReadOnlyReactiveProperty<bool> IsLowHealth { get; }
    
    bool TakeDamage(DamageData damageData);
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
