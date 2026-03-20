using UnityEngine;

/// <summary>
/// 플레이어 기본 설정
/// Resources/PlayerDefaultSettings에 배치
/// </summary>
[CreateAssetMenu(fileName = "PlayerDefaultSettings", menuName = "Player/Default Settings")]
public class PlayerDefaultSettings : ScriptableObject
{
    private static PlayerDefaultSettings _instance;
    public static PlayerDefaultSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<PlayerDefaultSettings>("PlayerDefaultSettings");
            }
            return _instance;
        }
    }

    [Header("Equipment")]
    [SerializeField] private WeaponData defaultWeapon;
    [SerializeField] private ArmorData defaultArmor;

    [Header("Stat Tree")]
    [SerializeField] private StatTreeData defaultStatTree;

    public WeaponData DefaultWeapon => defaultWeapon;
    public ArmorData DefaultArmor => defaultArmor;
    public StatTreeData DefaultStatTree => defaultStatTree;
}
