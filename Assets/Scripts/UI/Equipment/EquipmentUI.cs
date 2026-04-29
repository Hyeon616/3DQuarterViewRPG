using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

/// <summary>
/// 장비 UI (현재 장착된 무기/방어구 표시)
/// </summary>
public class EquipmentUI : MonoBehaviour, IToggleableUI
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Weapon Slot")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private Button unequipWeaponButton;

    [Header("Armor Slot")]
    [SerializeField] private Image armorIcon;
    [SerializeField] private TMP_Text armorNameText;
    [SerializeField] private Button unequipArmorButton;

    [Header("Stats Display")]
    [SerializeField] private TMP_Text statsText;

    [Header("Visual")]
    [SerializeField] private Sprite emptySlotSprite;

    private PlayerEquipment _equipment;
    private PlayerStatController _statController;
    private bool _isInitialized = false;

    public bool IsOpen => panel != null && panel.activeSelf;

    /// <summary>
    /// UI 열림/닫힘 이벤트 (플레이어 입력 제어용)
    /// </summary>
    public event System.Action<bool> OnUIToggled;

    private void Awake()
    {
        if (unequipWeaponButton != null)
            unequipWeaponButton.onClick.AddListener(OnUnequipWeaponClicked);

        if (unequipArmorButton != null)
            unequipArmorButton.onClick.AddListener(OnUnequipArmorClicked);

    }

    private void OnDestroy()
    {
        if (_equipment != null)
        {
            _equipment.OnEquipmentChanged -= UpdateEquipmentDisplay;
        }

        if (_statController != null)
        {
            _statController.OnStatsChanged -= UpdateStats;
        }

        if (unequipWeaponButton != null)
            unequipWeaponButton.onClick.RemoveListener(OnUnequipWeaponClicked);

        if (unequipArmorButton != null)
            unequipArmorButton.onClick.RemoveListener(OnUnequipArmorClicked);
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(PlayerEquipment equipment, PlayerStatController statController)
    {
        if (_isInitialized) return;

        _equipment = equipment;
        _statController = statController;

        if (_equipment != null)
        {
            _equipment.OnEquipmentChanged += UpdateEquipmentDisplay;
        }

        if (_statController != null)
        {
            _statController.OnStatsChanged += UpdateStats;
        }

        _isInitialized = true;

        UpdateEquipmentDisplay();
    }

    /// <summary>
    /// 장비 표시 업데이트
    /// </summary>
    private void UpdateEquipmentDisplay()
    {
        UpdateWeaponSlot();
        UpdateArmorSlot();
        UpdateStats();
    }

    private void UpdateWeaponSlot()
    {
        if (_equipment == null) return;

        var weapon = _equipment.CurrentWeapon;

        if (weapon != null)
        {
            // 무기 있음
            if (weaponIcon != null)
            {
                weaponIcon.sprite = weapon.Icon != null ? weapon.Icon : emptySlotSprite;
                weaponIcon.enabled = true;
            }

            if (weaponNameText != null)
            {
                weaponNameText.text = weapon.Name;
            }

            if (unequipWeaponButton != null)
            {
                unequipWeaponButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // 무기 없음
            if (weaponIcon != null)
            {
                weaponIcon.sprite = emptySlotSprite;
                weaponIcon.enabled = emptySlotSprite != null;
            }

            if (weaponNameText != null)
            {
                weaponNameText.text = "무기 없음";
            }

            if (unequipWeaponButton != null)
            {
                unequipWeaponButton.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateArmorSlot()
    {
        if (_equipment == null) return;

        var armor = _equipment.CurrentArmor;

        if (armor != null)
        {
            // 방어구 있음
            if (armorIcon != null)
            {
                armorIcon.sprite = armor.Icon != null ? armor.Icon : emptySlotSprite;
                armorIcon.enabled = true;
            }

            if (armorNameText != null)
            {
                armorNameText.text = armor.Name;
            }

            if (unequipArmorButton != null)
            {
                unequipArmorButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // 방어구 없음
            if (armorIcon != null)
            {
                armorIcon.sprite = emptySlotSprite;
                armorIcon.enabled = emptySlotSprite != null;
            }

            if (armorNameText != null)
            {
                armorNameText.text = "방어구 없음";
            }

            if (unequipArmorButton != null)
            {
                unequipArmorButton.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateStats()
    {
        if (statsText == null || _statController == null) return;

        string stats = "스탯\n\n";
        stats += $"HP: {_statController.MaxHp:F0}\n";
        stats += $"공격력: {_statController.Attack:F0}\n";
        stats += $"방어력: {_statController.Defense:F0}\n";
        stats += $"치명타 확률: {_statController.CriticalChance:F1}%\n";
        stats += $"치명타 데미지: {(_statController.CriticalDamage - 1f) * 100f:F0}%\n";
        stats += $"공격 속도: {_statController.AttackSpeed:F2}\n";
        stats += $"데미지 증가: {_statController.DamageIncrease:F1}%\n";

        statsText.text = stats;
    }

    private void OnUnequipWeaponClicked()
    {
        if (_equipment == null) return;

        // TODO: 인벤토리에 공간 체크
        // 현재는 단순히 해제만

        _equipment.CmdUnequipWeapon();
    }

    private void OnUnequipArmorClicked()
    {
        if (_equipment == null) return;

        // TODO: 인벤토리에 공간 체크
        // 현재는 단순히 해제만

        _equipment.CmdUnequipArmor();
    }

    #region Panel Control

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            UpdateEquipmentDisplay();
            OnUIToggled?.Invoke(true);
        }
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        OnUIToggled?.Invoke(false);
    }

    #endregion
}
