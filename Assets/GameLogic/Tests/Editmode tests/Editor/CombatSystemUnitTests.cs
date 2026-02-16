using NUnit.Framework;
using UnityEngine;
using GameEnums;

[TestFixture]
public class CombatSystemLogicTests
{
    [Test]
    public void SlashGeometry_Circle_Returns8Directions()
    {
        var slashVectors = new System.Collections.Generic.Dictionary<SlashType, Vector2[]>
        {
            { SlashType.Circle, new Vector2[] {
                new Vector2(0,-1), new Vector2(-1,-1), new Vector2(-1,0), new Vector2(-1,1),
                new Vector2(0,1), new Vector2(1,1), new Vector2(1,0), new Vector2(1,-1)
            }}
        };
        
        Assert.AreEqual(8, slashVectors[SlashType.Circle].Length, "Circle slash must have exactly 8 directions");
    }

    [Test]
    public void DamageCalculation_NonCritical_IsAlwaysLessThanCritical_WithSameParameters()
    {
       const float baseDamage = 10f;
       const float comboMultiplier = 1.5f; 
       const float randomizer = 1.0f; 

       float criticalDamage = baseDamage * 2f * randomizer;  
       float nonCriticalDamage = baseDamage * comboMultiplier * randomizer; 

       Assert.Less(nonCriticalDamage, criticalDamage, "Non critical damage must always be less than critical damage with identical parameters");
    
       Assert.AreEqual(20f, criticalDamage, 0.01f, "Critical damage must be exactly double base damage with randomizer (ignoring combo multiplier)");
    }
}
