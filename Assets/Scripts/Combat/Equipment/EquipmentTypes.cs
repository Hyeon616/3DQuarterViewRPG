/// <summary>
/// 무기 타입 - 타입별로 고유한 배율 적용
/// </summary>
public enum WeaponType
{
    None = 0,
    TwoHandedSword,     // 양손검 - 높은 공격력, 느린 속도
    SwordAndShield,     // 검+방패 - 균형잡힌 공격과 방어
    Spear,              // 창 - 긴 사거리, 빠른 속도
    Hammer,             // 해머 - 매우 높은 공격력, 매우 느린 속도
    Gauntlet,           // 건틀릿 - 매우 빠른 속도, 낮은 공격력
    Bow                 // 활 - 원거리, 중간 공격력
}

/// <summary>
/// 방어구 타입 - 타입별로 고유한 배율 적용
/// </summary>
public enum ArmorType
{
    None = 0,
    Cloth,      // 천 - 마법사용, 최소 방어
    Leather,    // 가죽 - 경량, 낮은 방어
    Light,      // 경갑 - 중간 방어
    Heavy,      // 중갑 - 높은 방어
    Plate       // 판금 - 최고 방어
}
