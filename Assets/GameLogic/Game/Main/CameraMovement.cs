
using UnityEngine;
using Zenject;
using System.Collections;


public class CameraMoovement : MonoBehaviour
{
    [Inject(Id = "HeroTr")]
    private readonly Transform _aim;
    [Inject]
    private readonly GameData _gameData;

    [SerializeField]private Transform targetTransform;
    
    [Range(0.01f,5f)]public float lerpTime,size,baseSize,shakeIntencity,shakeDelay;
    [SerializeField]private Vector2 offset;
    private Camera cam;
    private Vector3 startPos;
    private Coroutine shakeRoutine;

    void Start()
    {
        cam = GetComponent<Camera>();
        startPos = transform.position;
        targetTransform = _aim;
    }
    
    private void FixedUpdate()
    {
       transform.position = Vector3.Lerp(transform.position, targetTransform.position - new Vector3(offset.x,offset.y,10) , lerpTime * Time.fixedDeltaTime);
    }
    
    void Update()
    {
       // transform.position = targetTransform.position - new Vector3(0,offsetY,10);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize,size,20f * Time.deltaTime);
    }

    public void SlowMotionEffect(bool needToChangeTransform,Transform tr , float slowmoPower,float cameraSize)
    {
      if(!_gameData.NeedSlowMo.Value)return;
      if(needToChangeTransform) targetTransform = tr;
      size = cameraSize;
      Time.timeScale = slowmoPower;
      Invoke("StopEffect",1f);
    }

     public void ShakeEffect(int shakeDuration)
    {
      if(_gameData.NeedCameraShake.Value)
      {
         if(shakeRoutine != null)
         {
             StopCoroutine(shakeRoutine);
             shakeRoutine = null;
         }
         shakeRoutine = StartCoroutine(ShakeRoutine(shakeDuration));
      }
    }
    
    void StopEffect()
    {
      size = baseSize;
      Time.timeScale = 1f;
      targetTransform = _aim;
    }

    private IEnumerator ShakeRoutine(int switchCount)
    {
        for (int i = 0; i < switchCount; i++)
        {
            float direction = (i % 2 == 0) ? 1f : -1f;
            transform.position = startPos + new Vector3(direction * shakeIntencity, 0, 0);
            yield return new WaitForSeconds(shakeDelay);
        }
        
        transform.position = startPos;
        shakeRoutine = null;
    }

    private void OnDestroy()
    {
        if (shakeRoutine != null)StopCoroutine(shakeRoutine);
    }
    

    public void SetCameraByDefault()
    {
      size = 1.7f;
      targetTransform = _aim;
    }
}