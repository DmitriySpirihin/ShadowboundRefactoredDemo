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
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }
}

[RequireComponent(typeof(ShieldSystem))]
public class StaminaWithShield : BaseStamina
{
   [Inject] private ShieldSystem _shieldSystem;
   [SerializeField] private float shieldedRegenerationSpeed;
   private bool _isShielded;

   private readonly CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
       if (_shieldSystem == null)
        {
            Debug.LogError($"The ShieldSystem component on {gameObject.name} wasn.t injected , the StaminaSystem is disabled ");
            enabled = false;
            return;
        }

        _shieldSystem.IsShielded.Subscribe(SetIsShielded).AddTo(_disposables);
    }
    
    private void SetIsShielded(bool value) => _isShielded = value;

    protected override IEnumerator RestoreRoutine()
    {
       while (_currentStamina.Value < 100f)
       {
          yield return new WaitForSeconds(0.05f);
          _currentStamina.Value += _isShielded ? shieldedRegenerationSpeed : regenerationSpeed;
       }
    }

    protected override void OnDestroy()
    {
        _disposables.Dispose();
    }
}
