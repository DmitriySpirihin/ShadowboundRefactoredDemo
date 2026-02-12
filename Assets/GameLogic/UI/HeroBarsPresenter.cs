using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;

public class HeroBarsPresenter : MonoBehaviour
{
    [Inject] private BaseHealth<HealthConfigSO> _health;
    [Header("Ui health components")]
    [SerializeField]private Image[] heartsFillTemp = new Image[7];
    [SerializeField]private Image[] heartsFill = new Image[7];
    [SerializeField]private Image[] heartFrames = new Image[7];
    [SerializeField]float perHeartAmount = 25f;
    [Header("Ui stamina components")]
    [SerializeField]private Image[] staminaFill = new Image[7];
    [SerializeField]private Image[] staminaFrames = new Image[7];
    
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        if (_health == null)
        {
            Debug.LogError("The BaseHealth component wasn.t injected , the HeroBarsPresenter is disabled ");
            enabled = false;
            return;
        }
        for (int i = 0; i < 7; i++)
        {
            if (heartsFill[i] == null || heartsFillTemp[i] == null || heartFrames[i] == null)
            {
                Debug.LogError("Sprites arrays not completely filled , the HeroBarsPresenter is disabled ");
                enabled = false;
                break;
            }
        }
        InitializeHealth();
        _health.CurrentTempHealth.Subscribe(FillTempHearts).AddTo(_disposables);
        _health.CurrentHealth.Subscribe(FillHearts).AddTo(_disposables);
    }

    void FillHearts(float health)
    {
        int fullHearts = Mathf.FloorToInt(health / perHeartAmount);
        float remainder = health % perHeartAmount;

        for (int i = 0; i < heartsFill.Length; i++)
        {
           if (i < fullHearts) heartsFill[i].fillAmount = 1f;
           else if (i == fullHearts) heartsFill[i].fillAmount = remainder / perHeartAmount;
           else heartsFill[i].fillAmount = 0f;
        }
    }
    public void FillTempHearts(float temphealth)
    {
        int fullHearts = Mathf.FloorToInt(temphealth / perHeartAmount);
        float remainder = temphealth % perHeartAmount;

        for (int i = 0; i < heartsFill.Length; i++)
        {
           if (i < fullHearts) heartsFillTemp[i].fillAmount = 1f;
           else if (i == fullHearts) heartsFillTemp[i].fillAmount = remainder / perHeartAmount;
           else heartsFillTemp[i].fillAmount = 0f;
        }
    }
    public void InitializeHealth()
    {
        int fullHearts = Mathf.FloorToInt(_health.MaxHealth.Value / perHeartAmount);
        float remainder = _health.MaxHealth.Value % perHeartAmount;
        for(int i = 1;i < heartsFill.Length; i++)
        {
            if (i < fullHearts)
            {
                heartFrames[i].gameObject.SetActive(true);
                heartsFillTemp[i].gameObject.SetActive(true);
                heartFrames[i-1].fillAmount = 1;
                heartsFillTemp[i-1].fillAmount = 1;
            }
            else if (i == fullHearts)
            {
                heartFrames[i].gameObject.SetActive(true);
                heartsFillTemp[i].gameObject.SetActive(true);
                heartsFill[i].fillAmount = remainder / perHeartAmount;
            }
            else
            {
                heartFrames[i].gameObject.SetActive(false);
                heartsFillTemp[i].gameObject.SetActive(false);
                heartFrames[i-1].fillAmount = 0;
                heartsFillTemp[i-1].fillAmount = 0;
            }
        }
        
    }

    void OnDestroy()
    {
        _disposables.Dispose();
    }
}
