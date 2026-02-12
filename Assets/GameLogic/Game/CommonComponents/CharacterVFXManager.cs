using System.Collections.Generic;
using System.Collections;
using GameEnums;
using UnityEngine;

public class CharacterVFXManager : MonoBehaviour , IVfxService
{
    [SerializeField] private Transform tempVfxHolder;
    [SerializeField] private VfxName[] vfxKeys;
    [SerializeField] private GameObject[] vfxPrefabs;
    private Dictionary<VfxName, GameObject> vfxBank = new Dictionary<VfxName, GameObject>();
    private Dictionary<VfxName, Vector2> localPosBank = new Dictionary<VfxName, Vector2>();
    [SerializeField] private PoolableVfx[] pool;

    void Awake()
    {
        // fill vfx dictionary from keys array
        if (vfxKeys.Length > 0 && vfxPrefabs.Length > 0)
        {
            for (int i = 0; i < vfxKeys.Length; i++)
            {
               if(vfxPrefabs[i] == null) return;
               if(vfxBank.ContainsKey(vfxKeys[i])) return;
               vfxBank.Add(vfxKeys[i], vfxPrefabs[i]);
            }
        }
        // fill positions and isEnabled flags for pool
        if(pool != null && pool.Length > 0)
        {
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i].objects == null || pool[i].objects.Length == 0)
                {
                    Debug.LogWarning($"Pool '{pool[i].key}' has no objects!");
                    continue;
                }
                pool[i].enabledVfxes = new List<bool>();
                pool[i].localPositions = new List<Vector2>();
                for (int j = 0; j < pool[i].objects.Length; j++)
                {
                    if (pool[i].objects[j] == null)
                    {
                      Debug.LogError($"Pool '{pool[i].key}' has null object at index {j}");
                      continue;
                    }
                  pool[i].enabledVfxes.Add(true);
                  pool[i].localPositions.Add(pool[i].objects[j].transform.localPosition);
                  pool[i].objects[j].SetActive(false);
                }
            }
        }
    }

    public void ActivateVfx(VfxName vfxName)
    {
        if (!vfxBank.TryGetValue(vfxName, out GameObject vfx) || vfx == null) return;
        localPosBank[vfxName] = vfx.transform.localPosition;
        vfx.SetActive(true);
        // disable parent binding
        if(tempVfxHolder != null) vfx.transform.SetParent(tempVfxHolder,false);
        // back on start position
        StartCoroutine(SetObjectBackRoutine(vfx, localPosBank[vfxName], 1f));
    }

    public void ActivateVfxFromPool(VfxName vfxName)
    {
        if(pool.Length == 0)return;
        PoolableVfx poolableVfx = null;
        for (int i = 0; i < pool.Length; i++)
        {
            if(pool[i].key == vfxName)
            {
                poolableVfx = pool[i];
            }
        }
        if(poolableVfx == null)return;
     
        for (int i = 0; i < poolableVfx.enabledVfxes.Count; i++)
        {
            if (poolableVfx.enabledVfxes[i])
            {
                poolableVfx.objects[i].SetActive(true);
                if(tempVfxHolder != null) poolableVfx.objects[i].transform.SetParent(tempVfxHolder,false);
                poolableVfx.enabledVfxes[i] = false;
                StartCoroutine(CheckParticlesLifeTimeRoutine(poolableVfx, poolableVfx.objects[i], i));
                break;
            }   
        }
    }

    private IEnumerator CheckParticlesLifeTimeRoutine(PoolableVfx poolableVfx,GameObject vfx, int index)
    {
        yield return new WaitForSeconds(poolableVfx.duration);
        if (this == null || vfx == null || poolableVfx == null) yield break;
        vfx.SetActive(false);
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = poolableVfx.localPositions[index];
        poolableVfx.enabledVfxes[index] = true;
    }
    private IEnumerator SetObjectBackRoutine(GameObject vfx,Vector2 localPos, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (this == null) yield break;
        vfx.SetActive(false);
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = localPos;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}

[System.Serializable]
public class PoolableVfx
{
    [Tooltip("Vfx name")]
    public VfxName key;  
    public float duration; 
    [Tooltip("Effects array")]
    public GameObject[] objects;
    [System.NonSerialized] public List<Vector2> localPositions;
    [System.NonSerialized] public List<bool> enabledVfxes;
}

public interface IVfxService
{
   public void ActivateVfxFromPool(VfxName vfxName);
   public void ActivateVfx(VfxName vfxName);
}