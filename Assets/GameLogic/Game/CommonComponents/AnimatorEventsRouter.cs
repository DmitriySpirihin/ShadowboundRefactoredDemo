using GameEnums;
using UnityEngine;
using Zenject;

public class AnimatorEventsRouter : MonoBehaviour
{
    [Inject] private IVfxService _vfxService;

    public void PlayVfx(VfxName vfxName) => _vfxService.ActivateVfx(vfxName);
    public void PlayVfxFromPool(VfxName vfxName) => _vfxService.ActivateVfxFromPool(vfxName);
}
