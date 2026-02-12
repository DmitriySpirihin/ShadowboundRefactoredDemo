using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBar : MonoBehaviour
{
    private Transform parrentTransform;
    private EnemyHealth health;
    public Sprite enhencedSprite;
    [SerializeField][Range(0f,.1f)] private float vanishingSpeed;
    [SerializeField][Range(0f,1f)] private float delay;
    private float hp,st,conc;
    private float offset;
    private float tempHp = 1;
    private float timer;

    Transform bar,tempbar,staminaBar;
    [SerializeField]private Transform concentrationBar,concentrationFill;
    public bool needToDestroy;
    public GameObject barAcid,barBleeding,barWeakness;

    

    void Start()
    {
        staminaBar = transform.GetChild(2);
        bar = transform.GetChild(1);
        tempbar = transform.GetChild(0);
        //transform.localScale = health.maxHealth / 100f * 0.2f > 0.35f ? new Vector3(health.maxHealth / 100f * 0.2f,0.4f,0.4f) : new Vector3(0.35f,0.4f,0.4f);
        if(transform.localScale.x > 0.9f)transform.localScale = new Vector3(0.9f,0.4f,0.4f);
    }

    
    void Update()
    {
        //conc = health.concentration / 100f ;
        //hp = health.health / health.maxHealth;
        //st = enemyStamina.stamina > 0 ? enemyStamina.stamina / 100f : 0;
        if(hp < tempHp)
        {
           timer -= Time.deltaTime;
           if(timer < 0)tempHp -= vanishingSpeed;
        }
        else timer = delay;
        
        concentrationBar.gameObject.SetActive(conc > 0.1f);
        concentrationFill.localScale = new Vector3(conc,1f,1f);

        transform.position = new Vector3(parrentTransform.position.x , parrentTransform.position.y + offset,0f) ;
        tempbar.localScale = new Vector3(tempHp,1f,1f);
        tempbar.localPosition = new Vector3((1f - tempHp) * -0.231f,0,0);
        bar.localScale = new Vector3(hp,1f,1f);
        bar.localPosition = new Vector3((1f - hp) * -0.231f,0,0);
        staminaBar.localScale = new Vector3(st,1,1);
        
        if(hp <= 0f)Destroy(gameObject,0f);

    }

    public void SetParentTransform(Transform parT,EnemyHealth parHealth,float posY)
    {
        offset = posY;
        parrentTransform = parT;
        health = parHealth;
        //enemyStamina = parrentTransform.GetComponent<EnemyStamina>();
    }
    public void SetEnhenced()
    {
       GetComponent<SpriteRenderer>().sprite = enhencedSprite;
    }
}