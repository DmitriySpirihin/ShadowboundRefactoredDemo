using UnityEngine;
using Zenject;
using UniRx;
using System.Collections;

[RequireComponent (typeof(ShieldSystem))]
public class StaminaWithShield : BaseStamina
{
   [Inject] GameData _gameData;
   private ShieldSystem _shieldSystem;
   [SerializeField] private float shieldedRegenerationSpeed;
   private bool _isShielded;

   private readonly CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        _maxStamina.Value = _gameData.StaminaLevel.Value * 25f;
        _currentStamina.Value = _maxStamina.Value;
        _shieldSystem = GetComponent<ShieldSystem>();
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
