using Zenject;
using System.Collections.Generic;
using UnityEngine;

public class BloodCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    [Inject]private BloodEffectsPoolManager _poolManager;

    private void Awake()
    {
       _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(_particleSystem,other,collisionEvents);
        int count = collisionEvents.Count;

        for (int i = 0; i < count; i++)
        {
           if(Random.Range(0,10) > 8)
           {
              _poolManager.SpawnBloodStain(collisionEvents[i].intersection);
           }
        }
    }
    
}
