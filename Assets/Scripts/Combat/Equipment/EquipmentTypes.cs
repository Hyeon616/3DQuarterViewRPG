/// <summary>
/// 무기 타입 - 타입별로 고유한 배율 적용
/// </summary>
public enum WeaponType
{
    None = 0,
    TwoHandedSword,     // 양손검
    SwordAndShield,     // 검+방패
    Spear,              // 창
    Axe,                // 도끼
    Hammer              // 해머
}

/// <summary>
/// 방어구 타입 - 타입별로 고유한 배율 적용
/// </summary>
public enum ArmorType
{
    None = 0,
    Leather,    // 가죽
    Plate,      // 철갑
    Light,      // 경장
    Heavy       // 중장
}
