using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputOnScreenStick : MonoBehaviour,IPointerUpHandler,IPointerDownHandler,IDragHandler,IInputMove
{
   
   [SerializeField] private Color[] colors = new Color[2];
   [SerializeField] private Vector3[] scale = new Vector3[2];
   [SerializeField] private Camera cam;
   [SerializeField] private RectTransform lowerStick, upperStick,mainRectTransform;
   private Vector3 stickPosition;
   [SerializeField] private float deadZone = 35f;
   [SerializeField] private float maxDistance = 150f;
   [SerializeField] private float direction = 0;
   [SerializeField] private float attitude = 0;


    void Start()
    {
        lowerStick = transform.GetChild(0).GetComponent<RectTransform>();
        upperStick = transform.GetChild(1).GetComponent<RectTransform>();
        stickPosition = upperStick.anchoredPosition;
        
        lowerStick.localScale = scale[0];
        upperStick.localScale = scale[0];
        lowerStick.gameObject.GetComponent<Image>().color = colors[0];
        upperStick.gameObject.GetComponent<Image>().color = colors[0];
    }
    
    public Vector2 GetDirection()
    {
        return new Vector2(direction,attitude);
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainRectTransform,ped.position,cam,out Vector2 startPos);
        upperStick.anchoredPosition = startPos;
        lowerStick.anchoredPosition = startPos;

        lowerStick.localScale = scale[1];
        upperStick.localScale = scale[1];
        lowerStick.gameObject.GetComponent<Image>().color = colors[1];
        upperStick.gameObject.GetComponent<Image>().color = colors[1];
    }
     public virtual void OnPointerUp(PointerEventData ped)
    {
       upperStick.anchoredPosition = stickPosition;
       lowerStick.anchoredPosition = stickPosition;
       direction = 0;
       attitude = 0;
       lowerStick.localScale = scale[0];
        upperStick.localScale = scale[0];
        lowerStick.gameObject.GetComponent<Image>().color = colors[0];
        upperStick.gameObject.GetComponent<Image>().color = colors[0];
    }
     public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 startPos = lowerStick.anchoredPosition;
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainRectTransform,ped.position,cam,out localPos);
        Vector2 current = localPos - startPos;
        float distance = current.magnitude;

        Vector2 clampedPos = distance > maxDistance ? current.normalized * maxDistance : current;
        upperStick.anchoredPosition = startPos + clampedPos;
        direction = Mathf.Abs(clampedPos.x) < deadZone ? 0 : Mathf.Sign(clampedPos.x);
        attitude  = Mathf.Abs(clampedPos.y) < deadZone ? 0 : Mathf.Sign(clampedPos.y);
    }

    
}
