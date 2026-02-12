using GameEnums;
// all damage data
public struct DamageData
{
    public float BaseDamage;
    public int Direction;
    public bool IsCritical;
    public WeaponType WeaponType;
    public float ImpactSpeed;
    public float AirImpactSpeed;
    public bool IsFromBehind;
    
    public DamageData(float baseDamage, int direction, bool isCritical, WeaponType weaponType, float impactSpeed = 0f, float airImpactSpeed = 0f, bool isFromBehind = false)
    {
        BaseDamage = baseDamage;
        Direction = direction;
        IsCritical = isCritical;
        WeaponType = weaponType;
        ImpactSpeed = impactSpeed;
        AirImpactSpeed = airImpactSpeed;
        IsFromBehind = isFromBehind;
    }
}

