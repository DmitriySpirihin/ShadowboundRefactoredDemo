using GameEnums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

public class OnScreenButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    [Inject] private HeroController _heroController;
    [SerializeField] private ButtonType type;
    [SerializeField] private Vector3 startSize = new Vector3(2f,2f,2f);
    [SerializeField] private Vector3 pressedSize = new Vector3(2.5f,2.5f,2.5f);

    Color startColor = new Color(0.5f,0.5f,0.5f,0.5f);
    RectTransform _rectTransform;
    Image _image;
    
    void Start()
    {
        _image = transform.parent.GetComponent<Image>();
        _rectTransform = transform.parent.GetComponent<RectTransform>();
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
      _image.color = new Color(1,1,1,1);
      _rectTransform.localScale = pressedSize;
      switch (type)
      {
         case ButtonType.DoJump:
           
         break;
      }
      
    }
    public virtual void OnPointerUp(PointerEventData ped)
    {
      _image.color = startColor;
      _rectTransform.localScale = startSize;
      switch (type)
      {
        case ButtonType.DoParry:

         break;
      }
    }
    
}
