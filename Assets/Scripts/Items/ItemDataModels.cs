using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 파싱용 데이터 모델들
/// </summary>
namespace Items
{
    #region Enums

    public enum ItemType
    {
        Equipment,
        Consumable,
        Material,
        Quest
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum EquipmentType
    {
        Weapon,
        Armor
    }

    public enum WeaponType
    {
        None,
        TwoHandedSword,
        SwordAndShield,
        Spear,
        Hammer,
        Gauntlet,
        Bow
    }

    public enum ArmorType
    {
        None,
        Cloth,
        Leather,
        Light,
        Heavy,
        Plate
    }

    public enum ConsumableEffectType
    {
        RestoreHP,
        RestoreHPPercent,
        RestoreMana,
        Buff,
        Debuff
    }

    public enum MaterialType
    {
        Ore,
        Herb,
        Leather,
        Wood,
        Crystal,
        Other
    }

    #endregion

    #region JSON Root

    [Serializable]
    public class ItemsJsonData
    {
        public List<EquipmentJsonData> equipment;
        public List<ConsumableJsonData> consumables;
        public List<MaterialJsonData> materials;
        public List<QuestJsonData> quests;
    }

    [Serializable]
    public class EquipmentTypesJsonData
    {
        public List<WeaponTypeJsonData> weaponTypes;
        public List<ArmorTypeJsonData> armorTypes;
    }

    #endregion

    #region Item JSON Data

    [Serializable]
    public class BaseItemJsonData
    {
        public int id;
        public string name;
        public string description;
        public string itemType;
        public string rarity;
        public int maxStackSize = 1;
        public bool isTradeable = true;
        public bool isDroppable = true;
        public int sellPrice;
    }

    [Serializable]
    public class EquipmentJsonData : BaseItemJsonData
    {
        public string equipmentType;
        public string weaponType;
        public string armorType;

        // 무기 스탯
        public float baseAttack;
        public float baseAttackSpeed = 1f;
        public float baseCriticalChance;
        public float baseCriticalDamage = 2f;
        public float attackPerLevel = 2f;

        // 방어구 스탯
        public float baseHp;
        public float baseDefense;
        public float hpPerLevel = 10f;
        public float defensePerLevel = 1f;
    }

    [Serializable]
    public class ConsumableJsonData : BaseItemJsonData
    {
        public float cooldown;
        public List<ConsumableEffectJsonData> effects;
    }

    [Serializable]
    public class ConsumableEffectJsonData
    {
        public string type;
        public float value;
        public float duration;
    }

    [Serializable]
    public class MaterialJsonData : BaseItemJsonData
    {
        public string materialType;
    }

    [Serializable]
    public class QuestJsonData : BaseItemJsonData
    {
        public int questId = -1;
    }

    #endregion

    #region Equipment Type JSON Data

    [Serializable]
    public class WeaponTypeJsonData
    {
        public string type;
        public string displayName;
        public float attackMultiplier = 1f;
        public float attackSpeedMultiplier = 1f;
        public float criticalChanceMultiplier = 1f;
        public float criticalDamageMultiplier = 1f;
    }

    [Serializable]
    public class ArmorTypeJsonData
    {
        public string type;
        public string displayName;
        public float hpMultiplier = 1f;
        public float defenseMultiplier = 1f;
    }

    #endregion

    #region Runtime Item Data

    /// <summary>
    /// 런타임에서 사용되는 아이템 데이터 (JSON에서 파싱 후 변환)
    /// </summary>
    public class ItemData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType ItemType { get; set; }
        public ItemRarity Rarity { get; set; }
        public int MaxStackSize { get; set; } = 1;
        public bool IsTradeable { get; set; } = true;
        public bool IsDroppable { get; set; } = true;
        public int SellPrice { get; set; }

        // 아이콘 (ItemIconDatabase에서 연결)
        public Sprite Icon { get; set; }

        public virtual bool IsUsable => false;
        public virtual bool IsEquippable => false;
        public bool IsStackable => MaxStackSize > 1;

        public virtual string GetTooltipInfo() => Description;
    }

    public class EquipmentData : ItemData
    {
        public EquipmentType EquipmentType { get; set; }
        public WeaponType WeaponType { get; set; }
        public ArmorType ArmorType { get; set; }

        // 무기 스탯
        public float BaseAttack { get; set; }
        public float BaseAttackSpeed { get; set; } = 1f;
        public float BaseCriticalChance { get; set; }
        public float BaseCriticalDamage { get; set; } = 2f;
        public float AttackPerLevel { get; set; } = 2f;

        // 방어구 스탯
        public float BaseHp { get; set; }
        public float BaseDefense { get; set; }
        public float HpPerLevel { get; set; } = 10f;
        public float DefensePerLevel { get; set; } = 1f;

        public override bool IsEquippable => true;

        public float GetAttack(int level)
        {
            if (EquipmentType != EquipmentType.Weapon) return 0f;
            float attack = BaseAttack + AttackPerLevel * (level - 1);
            var multiplier = ItemManager.Instance?.GetWeaponTypeMultiplier(WeaponType);
            if (multiplier != null) attack *= multiplier.AttackMultiplier;
            return attack;
        }

        public float GetAttackSpeed()
        {
            if (EquipmentType != EquipmentType.Weapon) return 1f;
            float speed = BaseAttackSpeed;
            var multiplier = ItemManager.Instance?.GetWeaponTypeMultiplier(WeaponType);
            if (multiplier != null) speed *= multiplier.AttackSpeedMultiplier;
            return speed;
        }

        public float GetCriticalChance()
        {
            if (EquipmentType != EquipmentType.Weapon) return 0f;
            return BaseCriticalChance;
        }

        public float GetCriticalDamage()
        {
            if (EquipmentType != EquipmentType.Weapon) return 2f;
            return BaseCriticalDamage;
        }

        public float GetHp(int level)
        {
            if (EquipmentType != EquipmentType.Armor) return 0f;
            float hp = BaseHp + HpPerLevel * (level - 1);
            var multiplier = ItemManager.Instance?.GetArmorTypeMultiplier(ArmorType);
            if (multiplier != null) hp *= multiplier.HpMultiplier;
            return hp;
        }

        public float GetDefense(int level)
        {
            if (EquipmentType != EquipmentType.Armor) return 0f;
            float defense = BaseDefense + DefensePerLevel * (level - 1);
            var multiplier = ItemManager.Instance?.GetArmorTypeMultiplier(ArmorType);
            if (multiplier != null) defense *= multiplier.DefenseMultiplier;
            return defense;
        }

        public override string GetTooltipInfo()
        {
            string info = base.GetTooltipInfo();

            if (EquipmentType == EquipmentType.Weapon)
            {
                info += $"\n\n<color=orange>무기 타입:</color> {WeaponType}";
                info += $"\n<color=red>공격력:</color> {BaseAttack} (+{AttackPerLevel}/레벨)";
                info += $"\n<color=yellow>공격 속도:</color> {BaseAttackSpeed}";
                info += $"\n<color=cyan>치명타 확률:</color> {BaseCriticalChance}%";
                info += $"\n<color=magenta>치명타 데미지:</color> {(BaseCriticalDamage - 1f) * 100f}%";
            }
            else if (EquipmentType == EquipmentType.Armor)
            {
                info += $"\n\n<color=orange>방어구 타입:</color> {ArmorType}";
                info += $"\n<color=green>HP:</color> {BaseHp} (+{HpPerLevel}/레벨)";
                info += $"\n<color=blue>방어력:</color> {BaseDefense} (+{DefensePerLevel}/레벨)";
            }

            return info;
        }
    }

    public class ConsumableData : ItemData
    {
        public float Cooldown { get; set; }
        public List<ConsumableEffect> Effects { get; set; } = new List<ConsumableEffect>();

        public override bool IsUsable => true;

        public override string GetTooltipInfo()
        {
            string info = base.GetTooltipInfo();
            if (Effects != null && Effects.Count > 0)
            {
                info += "\n\n<color=green>효과:</color>";
                foreach (var effect in Effects)
                {
                    info += $"\n- {effect.GetDescription()}";
                }
            }
            if (Cooldown > 0)
            {
                info += $"\n\n<color=yellow>쿨다운: {Cooldown}초</color>";
            }
            return info;
        }
    }

    public class ConsumableEffect
    {
        public ConsumableEffectType EffectType { get; set; }
        public float Value { get; set; }
        public float Duration { get; set; }

        public string GetDescription()
        {
            return EffectType switch
            {
                ConsumableEffectType.RestoreHP => $"HP {Value} 회복",
                ConsumableEffectType.RestoreHPPercent => $"최대 HP의 {Value}% 회복",
                ConsumableEffectType.RestoreMana => $"마나 {Value} 회복",
                ConsumableEffectType.Buff => $"버프 효과 ({Duration}초)",
                ConsumableEffectType.Debuff => $"디버프 효과 ({Duration}초)",
                _ => "알 수 없는 효과"
            };
        }
    }

    public class MaterialData : ItemData
    {
        public MaterialType MaterialType { get; set; }
    }

    public class QuestData : ItemData
    {
        public int QuestId { get; set; } = -1;
    }

    #endregion

    #region Equipment Type Multipliers

    public class WeaponTypeMultiplier
    {
        public WeaponType Type { get; set; }
        public string DisplayName { get; set; }
        public float AttackMultiplier { get; set; } = 1f;
        public float AttackSpeedMultiplier { get; set; } = 1f;
        public float CriticalChanceMultiplier { get; set; } = 1f;
        public float CriticalDamageMultiplier { get; set; } = 1f;
    }

    public class ArmorTypeMultiplier
    {
        public ArmorType Type { get; set; }
        public string DisplayName { get; set; }
        public float HpMultiplier { get; set; } = 1f;
        public float DefenseMultiplier { get; set; } = 1f;
    }

    #endregion
}
