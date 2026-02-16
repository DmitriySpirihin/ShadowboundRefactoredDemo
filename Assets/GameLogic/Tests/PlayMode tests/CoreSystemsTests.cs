using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;

public class CoreSystemsTests
{
    [UnityTest]
    public IEnumerator ReactiveProperty_UpdatesValue_Correctly()
    {
        var health = new ReactiveProperty<float>(100f);
        
        Assert.AreEqual(100f, health.Value);
        
        health.Value = 80f;
        yield return null;
        
        Assert.AreEqual(80f, health.Value, "ReactiveProperty must update value immediately");
    }
}
