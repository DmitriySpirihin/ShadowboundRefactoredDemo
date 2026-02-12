using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
/*
public class AnimationMapperTest
{
    private Animator _testAnimator;
    private GameObject _testObject;
    IAnimationService animationMapper = new AnimationMapper();

    [SetUp]
    public void Setup()
    {
        _testObject = new GameObject();
        _testAnimator = _testObject.AddComponent<Animator>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(_testObject);
    }

    [Test]
    public void SetState_SetsBoolParameter()
    {
        Assert.DoesNotThrow(() => {
        animationMapper.SetState(_testAnimator, States.Idle, true);
     });
   }
   [UnityTest]
    public IEnumerator SetTrigger_ClearsAfterDelay() => UniTask.ToCoroutine(async () =>
   {
    animationMapper.SetTrigger(_testAnimator, Triggers.Attack);
    await UniTask.Delay(150);
    Assert.DoesNotThrow(() => {
        animationMapper.SetTrigger(_testAnimator, Triggers.Dash);
    });
    await UniTask.Yield(); 
});
}
*/