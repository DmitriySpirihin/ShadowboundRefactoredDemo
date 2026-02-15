using UnityEngine;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;

public class HeroConcentrationPresenter : MonoBehaviour
{
    private ConcentrationSystem _concentrationSystem;
    [SerializeField]private Transform concentrationBar, concentrationFill;

    private CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        _concentrationSystem = transform.parent.GetComponent<ConcentrationSystem>();
        _concentrationSystem.CurrentConcentration.Subscribe(v => ShowConcentration(v / 100f)).AddTo(_disposables);
    }

    private void ShowConcentration(float concentration)
    {
        concentrationBar.gameObject.SetActive(concentration > 0.1f);
        concentrationFill.localScale = new Vector3(concentration,1f,1f);
    }

    void OnDestroy()
    {
        _disposables.Dispose();
    }
}
