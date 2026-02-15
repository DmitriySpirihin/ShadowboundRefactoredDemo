using GameEnums;
using UnityEngine;
using Zenject;

public class AnimatorEventsRouter : MonoBehaviour
{
    [SerializeField] private GameObject _objectToDisable;
    [Inject] private IAnimationService _animationService;
    [SerializeField] private CharacterVFXManager _vfxService;
    [SerializeField] private Animator _animator;
    [SerializeField] private HeroHealth _health;
    [SerializeField] private Rigidbody2D _rigidbody;

    public void PlayVfx(VfxName vfxName) => _vfxService.ActivateVfx(vfxName);
    public void PlayVfxFromPool(VfxName vfxName) => _vfxService.ActivateVfxFromPool(vfxName);

    public void SetInvincibility(int value) => _health.ChangeInvincibility(value > 0);
    public void DisableOrEnableObject(int needToEnable) => _objectToDisable.SetActive(needToEnable > 0);

    public void PropelForward(float amount) => _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocityX * amount , _rigidbody.linearVelocityY);
}
