namespace GameEnums
{
    public enum HealthState { Alive, Dead, Invincible }
    public enum WeaponType { Sword, Glave, Dagger,Arrow, Projectile, Magic, Unarmed, Spear, Heavy, Parry, Miss, Death }
    
    public enum Triggers { AirAttack, WallJump, Climb, DoubleJump, Dash, Attack, MagicAttack, Hit, Execute, Throw, Parry }

    public enum GameState {Running, Paused}

    public enum ButtonType {DoJump, DoShield, DoParry, DoAttack, DoRoll, DoMagic}

    public enum AnimStates { Idle,isDead,CanSuperAttack,MoovingBack,Cling,Slide,Grounded,Shielded,Charging,InWeb,IsBackStrike,Destruct,Staggered,CanMove, HasConcentration}
    public enum AnimTriggers { Jump,AirAttack,WallJump,Climb,DoubleJump,Dash,Attack,MagicAttack,Hit,Execute,Throw,Parry}
    public enum AnimValues  { VelocityX, VelocityY, AttackNum}

    public enum VfxName { DamageSlash, ParrySparks, ShieldSparks, RunTrail, RollTrail, DashVfx, DoubleJumpVfx,LandVfx,WallSlideTrail,
                         AttackSlash_1, AttackSlash_2, AttackSlash_3, AttackSlash_4, AttackSlash_5, SuperAttackSlash, airAttackSlash}

    public enum SlashType{ Pierce, Slash, WideSlash, SemiCircleSlash, Circle}

}



 