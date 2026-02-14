using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

public class HeroBarsPresenter : MonoBehaviour
{
    [Inject] private BaseHealth<HealthConfigSO> _health;
    [Inject] private IStamina _stamina;
    
    [Header("Ui health components")]
    [SerializeField] private Image[] heartsFillTemp = new Image[7];
    [SerializeField] private Image[] heartsFill = new Image[7];
    [SerializeField] private Image[] heartFrames = new Image[7];
    
    [Header("Ui stamina components")]
    [SerializeField] private Image[] staminaFill = new Image[7];
    [SerializeField] private Image[] staminaFrames = new Image[7];
    
    [Header("Configuration")]
    [SerializeField] private float perSegmentAmount = 25f;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        // validation
        if (_health == null)
        {
            Debug.LogError("BaseHealth not injected — HeroBarsPresenter disabled");
            enabled = false;
            return;
        }
        if (_stamina == null)
        {
            Debug.LogError("IStamina not injected — HeroBarsPresenter disabled");
            enabled = false;
            return;
        }
        if (!ValidateUiArrays()) return;
        
        InitializeHealthSegments();
        InitializeStaminaSegments();
        
        // subscription
        _health.CurrentTempHealth.Subscribe(FillTempHealthSegments).AddTo(_disposables);
        _health.CurrentHealth.Subscribe(FillHealthSegments).AddTo(_disposables);
        _stamina.CurrentStamina.Subscribe(FillStaminaSegments).AddTo(_disposables);
        
        // init fill
        FillHealthSegments(_health.CurrentHealth.Value);
        FillTempHealthSegments(_health.CurrentTempHealth.Value);
        FillStaminaSegments(_stamina.CurrentStamina.Value);
    }

    private bool ValidateUiArrays()
    {
        // addit validations
        for (int i = 0; i < heartsFill.Length; i++)
        {
            if (heartsFill[i] == null || heartsFillTemp[i] == null || heartFrames[i] == null)
            {
                Debug.LogError($"Health UI array incomplete at index {i} — HeroBarsPresenter disabled");
                enabled = false;
                return false;
            }
        }
        for (int i = 0; i < staminaFill.Length; i++)
        {
            if (staminaFill[i] == null || staminaFrames[i] == null)
            {
                Debug.LogError($"Stamina UI array incomplete at index {i} — HeroBarsPresenter disabled");
                enabled = false;
                return false;
            }
        }
        return true;
    }

    private void InitializeHealthSegments()
    {
        int maxSegments = Mathf.CeilToInt(_health.MaxHealth.Value / perSegmentAmount);
        
        for (int i = 0; i < heartFrames.Length; i++)
        {
            bool isActive = i < maxSegments;
            heartFrames[i].gameObject.SetActive(isActive);
            heartsFillTemp[i].gameObject.SetActive(isActive);
            heartsFill[i].gameObject.SetActive(isActive);
            
            if (isActive && i == maxSegments - 1)
            {
                float remainder = _health.MaxHealth.Value % perSegmentAmount;
                if (remainder > 0)
                    heartsFill[i].fillAmount = remainder / perSegmentAmount;
                else
                    heartsFill[i].fillAmount = 1f;
            }
            else if (isActive)
            {
                heartsFill[i].fillAmount = 1f;
            }
        }
    }

    private void InitializeStaminaSegments()
    {
        const float maxStamina = 100f;
        int maxSegments = Mathf.CeilToInt(maxStamina / perSegmentAmount);
        
        for (int i = 0; i < staminaFrames.Length; i++)
        {
            bool isActive = i < maxSegments;
            staminaFrames[i].gameObject.SetActive(isActive);
            staminaFill[i].gameObject.SetActive(isActive);
            
            if (isActive && i == maxSegments - 1)
            {
                float remainder = maxStamina % perSegmentAmount;
                staminaFill[i].fillAmount = remainder > 0 ? remainder / perSegmentAmount : 1f;
            }
            else if (isActive)
            {
                staminaFill[i].fillAmount = 1f;
            }
        }
    }

    private void FillHealthSegments(float health)
    {
        int fullSegments = Mathf.FloorToInt(health / perSegmentAmount);
        float remainder = health % perSegmentAmount;
        
        for (int i = 0; i < heartsFill.Length; i++)
        {
            if (i < fullSegments)
                heartsFill[i].fillAmount = 1f;
            else if (i == fullSegments)
                heartsFill[i].fillAmount = remainder / perSegmentAmount;
            else
                heartsFill[i].fillAmount = 0f;
        }
    }

    private void FillTempHealthSegments(float tempHealth)
    {
        int fullSegments = Mathf.FloorToInt(tempHealth / perSegmentAmount);
        float remainder = tempHealth % perSegmentAmount;
        
        for (int i = 0; i < heartsFillTemp.Length; i++)
        {
            if (i < fullSegments)
                heartsFillTemp[i].fillAmount = 1f;
            else if (i == fullSegments)
                heartsFillTemp[i].fillAmount = remainder / perSegmentAmount;
            else
                heartsFillTemp[i].fillAmount = 0f;
        }
    }

    private void FillStaminaSegments(float stamina)
    {
        int fullSegments = Mathf.FloorToInt(stamina / perSegmentAmount);
        float remainder = stamina % perSegmentAmount;
        
        for (int i = 0; i < staminaFill.Length; i++)
        {
            if (i < fullSegments)
                staminaFill[i].fillAmount = 1f;
            else if (i == fullSegments)
                staminaFill[i].fillAmount = remainder / perSegmentAmount;
            else
                staminaFill[i].fillAmount = 0f;
        }
    }

    void OnDestroy()
    {
        _disposables.Dispose();
    }
}
