using GameEnums;
using UnityEngine;
using Zenject;

public class AnimatorEventsRouter : MonoBehaviour
{
    [SerializeField] private GameObject _objectToDisable;
    [Inject] private AudioManager _audioManager;
    [Inject] private IAnimationService _animationService;
    [Inject] private IVfxService _vfxService;
    [Inject] private Animator _animator;

    public void PlayVfx(VfxName vfxName) => _vfxService.ActivateVfx(vfxName);
    public void PlayVfxFromPool(VfxName vfxName) => _vfxService.ActivateVfxFromPool(vfxName);

    public void SetState(AnimStates state, bool value) => _animationService.SetState(_animator, state, value);

    public void DisableOrEnableObject(bool needToEnable) => _objectToDisable.SetActive(needToEnable);
}
