using UniRx;
using System.Collections;
using UnityEngine;
using Zenject;
using GameEnums;

public class ConcentrationSystem : MonoBehaviour
{
   [Inject] private AnimationMapper _animationService;
   [Inject] private CameraMoovement _cameraMoovement;
   [Inject] private Animator _animator;
   [Inject] protected PlayerHealthConfigSO _config;

   [SerializeField] private float maxCooldown;
   [SerializeField] private float regenerationSpeed;
   private bool isCooldown;
   private Coroutine _restoreRoutine;
   private Coroutine _cooldownRoutine;

   private readonly ReactiveProperty<float> _currentConcentration = new ReactiveProperty<float>(); 
   public IReadOnlyReactiveProperty<float> CurrentConcentration => _currentConcentration;
 
   public void ReduceConcentration(float amount)
   {
      float nextValue = Mathf.Max(0, _currentConcentration.Value - amount);
      _currentConcentration.Value = nextValue;
      if (nextValue == 0)
      {
        isCooldown = true;
        TriggerLostConcentration();
        if (_cooldownRoutine != null) StopCoroutine(_cooldownRoutine);
        _cooldownRoutine = StartCoroutine(CooldownRoutine());
      }
      if (_restoreRoutine != null) StopCoroutine(_restoreRoutine);
        _restoreRoutine = StartCoroutine(RestoreRoutine());
   }

   public void RestoreConcentration(float amount)
   {
      float nextValue = Mathf.Min(100f, _currentConcentration.Value + amount);
      _currentConcentration.Value = nextValue;
      if (nextValue == 100f)
      {
         if (_restoreRoutine != null) StopCoroutine(_restoreRoutine);
      }
      
   }
   
    private IEnumerator CooldownRoutine()
    {
       yield return new WaitForSeconds(maxCooldown);
       isCooldown = false;
    }

    private IEnumerator RestoreRoutine()
    {
       while (_currentConcentration.Value < 100f)
       {
          yield return new WaitForSeconds(0.05f);
          if(!isCooldown) _currentConcentration.Value += regenerationSpeed;
       }
    }

    private void TriggerLostConcentration()
    {
        _animationService.SetState(_animator, AnimStates.HasConcentration, true);
        _cameraMoovement.SlowMotionEffect(false,transform , _config.slowMoPower , _config.cameraSize);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}