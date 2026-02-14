using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using Zenject;
using GameEnums;


public class AnimationMapper : IAnimationService
{
    private readonly Dictionary<Animator, CancellationTokenSource> animatorResetTokens = new();
    // Hashes for Animation Parameters
    public readonly int IdleHash = Animator.StringToHash("Idle");
    public readonly int DeadHash = Animator.StringToHash("Dead");
    public readonly int SuperAttackHash = Animator.StringToHash("SuperAttack");
    public readonly int MoovingBackHash = Animator.StringToHash("MoovingBack");
    public readonly int ClingHash = Animator.StringToHash("Cling");
    public readonly int SlideHash = Animator.StringToHash("Slide");
    public readonly int GroundedHash = Animator.StringToHash("Grounded");
    public readonly int ShieldedHash = Animator.StringToHash("Shielded");
    public readonly int ChargingHash = Animator.StringToHash("Charging");
    public readonly int InWebHash = Animator.StringToHash("InWeb");
    public readonly int IsBackStrikeHash = Animator.StringToHash("IsBackStrike");
    public readonly int DestructHash = Animator.StringToHash("Destruct");
    public readonly int StaggeredHash = Animator.StringToHash("Staggered");
    public readonly int AirAttackHash = Animator.StringToHash("AirAttack");
    public readonly int WallJumpHash = Animator.StringToHash("WallJump");
    public readonly int ClimbHash = Animator.StringToHash("Climb");
    public readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");    
    public readonly int DashHash = Animator.StringToHash("Dash");
    public readonly int AttackHash = Animator.StringToHash("Attack");
    public readonly int MagicAttackHash = Animator.StringToHash("MagicAttack");
    public readonly int HitHash = Animator.StringToHash("Hit");
    public readonly int ExecuteHash = Animator.StringToHash("Execute");
    public readonly int ThrowHash = Animator.StringToHash("Throw");
    public readonly int ParryHash = Animator.StringToHash("Parry");
    public readonly int JumpHash = Animator.StringToHash("Jump");
    public readonly int HasConcentrationHash = Animator.StringToHash("HasConcentration");

    public readonly int CanMoveHash = Animator.StringToHash("CanMove");
    public readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    public readonly int VelocityYHash = Animator.StringToHash("VelocityY");

    public void SetState(Animator animator, AnimStates state, bool value)
    {
        switch (state)
        {
            case AnimStates.Idle:  animator.SetBool(IdleHash, value);  break;
            case AnimStates.isDead:  animator.SetBool(DeadHash, value);  break;
            case AnimStates.CanSuperAttack:  animator.SetBool(SuperAttackHash, value);  break;
            case AnimStates.MoovingBack:  animator.SetBool(MoovingBackHash, value);  break;
            case AnimStates.Cling:  animator.SetBool(ClingHash, value);  break;
            case AnimStates.Slide:  animator.SetBool(SlideHash, value);  break;
            case AnimStates.Grounded:  animator.SetBool(GroundedHash, value);  break;
            case AnimStates.Shielded:  animator.SetBool(ShieldedHash, value);  break;
            case AnimStates.Charging:  animator.SetBool(ChargingHash, value);  break;
            case AnimStates.InWeb:  animator.SetBool(InWebHash, value);  break;
            case AnimStates.IsBackStrike:  animator.SetBool(IsBackStrikeHash, value);  break;
            case AnimStates.Destruct:  animator.SetBool(DestructHash, value);  break;
            case AnimStates.Staggered:  animator.SetBool(StaggeredHash, value);  break;  
            case AnimStates.CanMove:  animator.SetBool(CanMoveHash, value);  break; 
            case AnimStates.HasConcentration:  animator.SetBool(HasConcentrationHash, value);  break;
        }
    }

    public void SetValue(Animator animator, AnimValues state, float value, int num)
    {
        switch (state)
        {
            case AnimValues.VelocityX:  animator.SetFloat(VelocityXHash, value);  break;  
            case AnimValues.VelocityY:  animator.SetFloat(VelocityYHash, value);  break;
            case AnimValues.AttackNum:  animator.SetFloat(VelocityYHash, num);  break;   
        }
    }

    public async void SetTrigger(Animator animator, AnimTriggers trigger)
    {
        if (animatorResetTokens.TryGetValue(animator, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        
        switch (trigger)
        {
             case AnimTriggers.Jump: animator.SetTrigger(JumpHash); break;
            case AnimTriggers.AirAttack: animator.SetTrigger(AirAttackHash); break;
            case AnimTriggers.WallJump: animator.SetTrigger(WallJumpHash); break;
            case AnimTriggers.Climb: animator.SetTrigger(ClimbHash); break;
            case AnimTriggers.DoubleJump: animator.SetTrigger(DoubleJumpHash); break;
            case AnimTriggers.Dash: animator.SetTrigger(DashHash); break;
            case AnimTriggers.Attack: animator.SetTrigger(AttackHash); break;
            case AnimTriggers.MagicAttack: animator.SetTrigger(MagicAttackHash); break;
            case AnimTriggers.Hit: animator.SetTrigger(HitHash); break;
            case AnimTriggers.Execute: animator.SetTrigger(ExecuteHash); break;
            case AnimTriggers.Throw: animator.SetTrigger(ThrowHash); break;
            case AnimTriggers.Parry: animator.SetTrigger(ParryHash); break;
        }

        animatorResetTokens[animator] = new CancellationTokenSource();
        await ResetAnimTriggersAfterDelay(animator, animatorResetTokens[animator].Token);
        
    }

    private async UniTask ResetAnimTriggersAfterDelay(Animator animator, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(100, cancellationToken: token);
             animator.ResetTrigger(JumpHash);
            animator.ResetTrigger(AirAttackHash);
            animator.ResetTrigger(WallJumpHash);
            animator.ResetTrigger(ClimbHash);
            animator.ResetTrigger(DoubleJumpHash);
            animator.ResetTrigger(DashHash);
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(MagicAttackHash);
            animator.ResetTrigger(HitHash);
            animator.ResetTrigger(ExecuteHash);
            animator.ResetTrigger(ThrowHash);
            animator.ResetTrigger(ParryHash);
        }
        catch (OperationCanceledException){}
    }
}
