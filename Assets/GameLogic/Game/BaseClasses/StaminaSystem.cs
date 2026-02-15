using System.Collections;
using UnityEngine;
using Zenject;
using UniRx;


public abstract class BaseStamina : MonoBehaviour, IStamina
{
   [SerializeField] protected float maxCooldown;
   [SerializeField] protected float regenerationSpeed;
   protected bool isCooldown;
   protected Coroutine _restoreRoutine;
   protected Coroutine _cooldownRoutine;

   protected readonly ReactiveProperty<float> _currentStamina = new ReactiveProperty<float>(); 
   public IReadOnlyReactiveProperty<float> CurrentStamina => _currentStamina;
   protected readonly ReactiveProperty<float> _maxStamina = new ReactiveProperty<float>(); 
   public IReadOnlyReactiveProperty<float> MaxStamina => _maxStamina; 
 
   public virtual void ReduceStamina(float amount)
   {
     float nextValue = Mathf.Max(0, _currentStamina.Value - amount);
     _currentStamina.Value = nextValue;
    
      if (_restoreRoutine != null) StopCoroutine(_restoreRoutine);
    
      if (nextValue == 0 && !isCooldown)
      {
        isCooldown = true;
        _cooldownRoutine = StartCoroutine(CooldownRoutine());
      }
      else if (nextValue > 0 && !isCooldown)
      {
        _restoreRoutine = StartCoroutine(RestoreRoutine());
      }
   }
   
    protected IEnumerator CooldownRoutine()
    {
       yield return new WaitForSeconds(maxCooldown);
       isCooldown = false;
    }

    protected virtual IEnumerator RestoreRoutine()
    {
       while (_currentStamina.Value < 100f)
       {
          yield return new WaitForSeconds(0.05f);
          if(!isCooldown) _currentStamina.Value += regenerationSpeed;
       }
       yield break;
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }
}


