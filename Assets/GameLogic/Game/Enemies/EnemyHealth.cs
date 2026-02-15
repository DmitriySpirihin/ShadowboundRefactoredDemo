using UnityEngine;
using Zenject;
using GameEnums;

public class EnemyHealth : BaseHealth
{
    protected override void OnDeath(DamageData damData)
    {
        throw new System.NotImplementedException();
    }
}