using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public class BloodEffectsPoolManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject bloodParticlesPref;
    [SerializeField] private GameObject bloodStainPref;
    [SerializeField] private TMP_Text[] damageTextPool;
    
    [Header("Settings")]
    [SerializeField] private float bloodLifeTime = 0.6f;
    [SerializeField] private float stainLifeTime = 5.6f;
    [SerializeField] private Color[] colors;
    [SerializeField] private Sprite[] stainsSprites;

    private Stack<GameObject> bloodParticlesPool = new Stack<GameObject>();
    private Stack<GameObject> bloodStainsPool = new Stack<GameObject>();  
    
    [Inject] private DiContainer _container;

    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject bloodParticles = Instantiate(bloodParticlesPref,Vector2.zero,Quaternion.identity,transform);
            _container.InjectGameObject(bloodParticles);
            bloodParticlesPool.Push(bloodParticles);
            bloodParticles.SetActive(false);

            GameObject bloodStain = Instantiate(bloodStainPref,Vector2.zero,Quaternion.identity,transform);
            bloodStainsPool.Push(bloodStain);
            bloodStain.SetActive(false);

        }
    }

    public void SpawnDamageText(Vector2 point, DamageData damData)
    {
       
    }


    public void SpawnBlood(Vector2 point, DamageData damData)
    {
        GameObject bloodParticles;
        if(bloodParticlesPool.Count == 0)
        {
            bloodParticles = Instantiate(bloodParticlesPref,point,Quaternion.identity,transform);
            _container.InjectGameObject(bloodParticles);
        }
        else 
        {
            bloodParticles = bloodParticlesPool.Pop();
            bloodParticles.transform.position = point;
            bloodParticles.SetActive(true);
        }
       
        bloodParticles.transform.localScale = new Vector2(Mathf.Sign(damData.Direction) * 1, 1);
        bloodParticles.GetComponent<ParticleSystem>().Emit(Mathf.CeilToInt(damData.BaseDamage / 2f));

        StartCoroutine(CheckParticlesLifeTimeRoutine(bloodParticles, bloodParticlesPool, bloodLifeTime));
    }

    public void SpawnBloodStain(Vector2 point)
    {
        GameObject bloodStain;
        if(bloodStainsPool.Count == 0) bloodStain = Instantiate(bloodStainPref,point,Quaternion.identity,transform);
        else 
        {
            bloodStain = bloodStainsPool.Pop();
            bloodStain.transform.position = point;
            bloodStain.SetActive(true);
        }
       
        bloodStain.transform.localScale = new Vector2(Random.Range(0.5f,1.5f), Random.Range(0.3f,1f));
         bloodStain.GetComponent<SpriteRenderer>().sprite = stainsSprites[Random.Range(0, stainsSprites.Length)];
        bloodStain.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

        StartCoroutine(CheckParticlesLifeTimeRoutine(bloodStain, bloodStainsPool, stainLifeTime * Random.Range(1f,2f)));
    }


    private IEnumerator CheckParticlesLifeTimeRoutine(GameObject particles,Stack<GameObject> stack, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (this == null || particles == null || stack == null) yield break;
        stack.Push(particles);
        particles.SetActive(false);
    }

    private void OnDestroy()
    {
       StopAllCoroutines();
       ClearPool(bloodParticlesPool);
       ClearPool(bloodStainsPool);
    }

    private void ClearPool(Stack<GameObject> pool)
   {
      while (pool.Count > 0) Destroy(pool.Pop());
   }

}


