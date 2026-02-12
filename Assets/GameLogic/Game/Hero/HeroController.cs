using GameEnums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;


public class HeroController : MonoBehaviour
{
    [Inject] private AnimationMapper _animationMapper;
}
