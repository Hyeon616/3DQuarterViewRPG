using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    /// <summary>
    /// 아이템 데이터 관리자
    /// JSON에서 아이템 데이터를 로드하고 에셋과 연결
    /// </summary>
    public class ItemManager
    {
        private static ItemManager _instance;
        public static ItemManager Instance => _instance ??= new ItemManager();

        private Dictionary<int, ItemData> _items = new Dictionary<int, ItemData>();

        private Dictionary<WeaponType, WeaponTypeMultiplier> _weaponMultipliers = new Dictionary<WeaponType, WeaponTypeMultiplier>();
        private Dictionary<ArmorType, ArmorTypeMultiplier> _armorMultipliers = new Dictionary<ArmorType, ArmorTypeMultiplier>();

        private ItemIconDatabase _iconDatabase;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 초기화 (Resources에서 JSON과 에셋 DB 로드)
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            LoadIconDatabase();
            LoadEquipmentTypes();
            LoadItems();

            _isInitialized = true;
            Debug.Log($"[ItemManager] Initialized with {_items.Count} items");
        }

        /// <summary>
        /// 강제 재초기화
        /// </summary>
        public void Reload()
        {
            _isInitialized = false;
            _items.Clear();
            _weaponMultipliers.Clear();
            _armorMultipliers.Clear();
            Initialize();
        }

        #region Load Methods

        private void LoadIconDatabase()
        {
            _iconDatabase = Resources.Load<ItemIconDatabase>("ItemIconDatabase");
            if (_iconDatabase == null)
            {
                Debug.LogWarning("[ItemManager] ItemIconDatabase not found in Resources folder");
            }
        }

        private void LoadEquipmentTypes()
        {
            var jsonAsset = Resources.Load<TextAsset>("Data/equipment_types");
            if (jsonAsset == null)
            {
                Debug.LogWarning("[ItemManager] equipment_types.json not found, using defaults");
                SetDefaultEquipmentTypes();
                return;
            }

            try
            {
                var data = JsonUtility.FromJson<EquipmentTypesJsonData>(jsonAsset.text);

                // 무기 타입 배율
                if (data.weaponTypes != null)
                {
                    foreach (var wt in data.weaponTypes)
                    {
                        if (Enum.TryParse<WeaponType>(wt.type, out var type))
                        {
                            _weaponMultipliers[type] = new WeaponTypeMultiplier
                            {
                                Type = type,
                                DisplayName = wt.displayName,
                                AttackMultiplier = wt.attackMultiplier,
                                AttackSpeedMultiplier = wt.attackSpeedMultiplier,
                                CriticalChanceMultiplier = wt.criticalChanceMultiplier,
                                CriticalDamageMultiplier = wt.criticalDamageMultiplier
                            };
                        }
                    }
                }

                // 방어구 타입 배율
                if (data.armorTypes != null)
                {
                    foreach (var at in data.armorTypes)
                    {
                        if (Enum.TryParse<ArmorType>(at.type, out var type))
                        {
                            _armorMultipliers[type] = new ArmorTypeMultiplier
                            {
                                Type = type,
                                DisplayName = at.displayName,
                                HpMultiplier = at.hpMultiplier,
                                DefenseMultiplier = at.defenseMultiplier
                            };
                        }
                    }
                }

                Debug.Log($"[ItemManager] Loaded {_weaponMultipliers.Count} weapon types, {_armorMultipliers.Count} armor types");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ItemManager] Failed to parse equipment_types.json: {e.Message}");
                SetDefaultEquipmentTypes();
            }
        }

        private void SetDefaultEquipmentTypes()
        {
            // 기본 무기 타입 배율
            _weaponMultipliers[WeaponType.TwoHandedSword] = new WeaponTypeMultiplier { Type = WeaponType.TwoHandedSword, AttackMultiplier = 1.2f, AttackSpeedMultiplier = 0.8f };
            _weaponMultipliers[WeaponType.SwordAndShield] = new WeaponTypeMultiplier { Type = WeaponType.SwordAndShield, AttackMultiplier = 1.0f, AttackSpeedMultiplier = 1.0f };
            _weaponMultipliers[WeaponType.Spear] = new WeaponTypeMultiplier { Type = WeaponType.Spear, AttackMultiplier = 0.9f, AttackSpeedMultiplier = 1.2f };
            _weaponMultipliers[WeaponType.Hammer] = new WeaponTypeMultiplier { Type = WeaponType.Hammer, AttackMultiplier = 1.5f, AttackSpeedMultiplier = 0.6f };
            _weaponMultipliers[WeaponType.Gauntlet] = new WeaponTypeMultiplier { Type = WeaponType.Gauntlet, AttackMultiplier = 0.7f, AttackSpeedMultiplier = 1.5f };
            _weaponMultipliers[WeaponType.Bow] = new WeaponTypeMultiplier { Type = WeaponType.Bow, AttackMultiplier = 1.0f, AttackSpeedMultiplier = 1.0f };

            // 기본 방어구 타입 배율
            _armorMultipliers[ArmorType.Cloth] = new ArmorTypeMultiplier { Type = ArmorType.Cloth, HpMultiplier = 0.8f, DefenseMultiplier = 0.5f };
            _armorMultipliers[ArmorType.Leather] = new ArmorTypeMultiplier { Type = ArmorType.Leather, HpMultiplier = 0.9f, DefenseMultiplier = 0.7f };
            _armorMultipliers[ArmorType.Light] = new ArmorTypeMultiplier { Type = ArmorType.Light, HpMultiplier = 1.0f, DefenseMultiplier = 1.0f };
            _armorMultipliers[ArmorType.Heavy] = new ArmorTypeMultiplier { Type = ArmorType.Heavy, HpMultiplier = 1.1f, DefenseMultiplier = 1.3f };
            _armorMultipliers[ArmorType.Plate] = new ArmorTypeMultiplier { Type = ArmorType.Plate, HpMultiplier = 1.2f, DefenseMultiplier = 1.5f };
        }

        private void LoadItems()
        {
            var jsonAsset = Resources.Load<TextAsset>("Data/items");
            if (jsonAsset == null)
            {
                Debug.LogWarning("[ItemManager] items.json not found in Resources/Data folder");
                return;
            }

            try
            {
                var data = JsonUtility.FromJson<ItemsJsonData>(jsonAsset.text);

                // 장비 로드
                if (data.equipment != null)
                {
                    foreach (var eq in data.equipment)
                    {
                        var item = CreateEquipmentData(eq);
                        RegisterItem(item);
                    }
                }

                // 소비 아이템 로드
                if (data.consumables != null)
                {
                    foreach (var con in data.consumables)
                    {
                        var item = CreateConsumableData(con);
                        RegisterItem(item);
                    }
                }

                // 재료 로드
                if (data.materials != null)
                {
                    foreach (var mat in data.materials)
                    {
                        var item = CreateMaterialData(mat);
                        RegisterItem(item);
                    }
                }

                // 퀘스트 아이템 로드
                if (data.quests != null)
                {
                    foreach (var quest in data.quests)
                    {
                        var item = CreateQuestData(quest);
                        RegisterItem(item);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ItemManager] Failed to parse items.json: {e.Message}");
            }
        }

        private void RegisterItem(ItemData item)
        {
            _items[item.Id] = item;

            // 아이콘 연결
            if (_iconDatabase != null)
            {
                item.Icon = _iconDatabase.GetIcon(item.Id);
            }
        }

        #endregion

        #region Item Creation

        private EquipmentData CreateEquipmentData(EquipmentJsonData json)
        {
            var item = new EquipmentData
            {
                Id = json.id,
                Name = json.name,
                Description = json.description,
                ItemType = ItemType.Equipment,
                Rarity = ParseEnum<ItemRarity>(json.rarity),
                MaxStackSize = 1,
                IsTradeable = json.isTradeable,
                IsDroppable = json.isDroppable,
                SellPrice = json.sellPrice,
                EquipmentType = ParseEnum<EquipmentType>(json.equipmentType),
                WeaponType = ParseEnum<WeaponType>(json.weaponType),
                ArmorType = ParseEnum<ArmorType>(json.armorType),
                BaseAttack = json.baseAttack,
                BaseAttackSpeed = json.baseAttackSpeed,
                BaseCriticalChance = json.baseCriticalChance,
                BaseCriticalDamage = json.baseCriticalDamage,
                AttackPerLevel = json.attackPerLevel,
                BaseHp = json.baseHp,
                BaseDefense = json.baseDefense,
                HpPerLevel = json.hpPerLevel,
                DefensePerLevel = json.defensePerLevel
            };
            return item;
        }

        private ConsumableData CreateConsumableData(ConsumableJsonData json)
        {
            var item = new ConsumableData
            {
                Id = json.id,
                Name = json.name,
                Description = json.description,
                ItemType = ItemType.Consumable,
                Rarity = ParseEnum<ItemRarity>(json.rarity),
                MaxStackSize = json.maxStackSize > 0 ? json.maxStackSize : 99,
                IsTradeable = json.isTradeable,
                IsDroppable = json.isDroppable,
                SellPrice = json.sellPrice,
                Cooldown = json.cooldown
            };

            if (json.effects != null)
            {
                foreach (var eff in json.effects)
                {
                    item.Effects.Add(new ConsumableEffect
                    {
                        EffectType = ParseEnum<ConsumableEffectType>(eff.type),
                        Value = eff.value,
                        Duration = eff.duration
                    });
                }
            }

            return item;
        }

        private MaterialData CreateMaterialData(MaterialJsonData json)
        {
            return new MaterialData
            {
                Id = json.id,
                Name = json.name,
                Description = json.description,
                ItemType = ItemType.Material,
                Rarity = ParseEnum<ItemRarity>(json.rarity),
                MaxStackSize = json.maxStackSize > 0 ? json.maxStackSize : 99,
                IsTradeable = json.isTradeable,
                IsDroppable = json.isDroppable,
                SellPrice = json.sellPrice,
                MaterialType = ParseEnum<MaterialType>(json.materialType)
            };
        }

        private QuestData CreateQuestData(QuestJsonData json)
        {
            return new QuestData
            {
                Id = json.id,
                Name = json.name,
                Description = json.description,
                ItemType = ItemType.Quest,
                Rarity = ParseEnum<ItemRarity>(json.rarity),
                MaxStackSize = json.maxStackSize > 0 ? json.maxStackSize : 99,
                IsTradeable = false,
                IsDroppable = false,
                SellPrice = 0,
                QuestId = json.questId
            };
        }

        private T ParseEnum<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return default;
            return Enum.TryParse<T>(value, true, out var result) ? result : default;
        }

        #endregion

        #region Public API

        /// <summary>
        /// 아이템 ID로 조회
        /// </summary>
        public ItemData GetItem(int itemId)
        {
            if (!_isInitialized) Initialize();
            return _items.TryGetValue(itemId, out var item) ? item : null;
        }

        /// <summary>
        /// 장비 아이템 조회 (캐스팅 헬퍼)
        /// </summary>
        public EquipmentData GetEquipment(int itemId)
        {
            return GetItem(itemId) as EquipmentData;
        }

        /// <summary>
        /// 무기 타입 배율 조회
        /// </summary>
        public WeaponTypeMultiplier GetWeaponTypeMultiplier(WeaponType type)
        {
            if (!_isInitialized) Initialize();
            return _weaponMultipliers.TryGetValue(type, out var mult) ? mult : null;
        }

        /// <summary>
        /// 방어구 타입 배율 조회
        /// </summary>
        public ArmorTypeMultiplier GetArmorTypeMultiplier(ArmorType type)
        {
            if (!_isInitialized) Initialize();
            return _armorMultipliers.TryGetValue(type, out var mult) ? mult : null;
        }

        /// <summary>
        /// 모든 아이템 반환
        /// </summary>
        public IEnumerable<ItemData> GetAllItems()
        {
            if (!_isInitialized) Initialize();
            return _items.Values;
        }

        /// <summary>
        /// 타입별 아이템 필터
        /// </summary>
        public List<ItemData> GetItemsByType(ItemType type)
        {
            if (!_isInitialized) Initialize();
            var result = new List<ItemData>();
            foreach (var item in _items.Values)
            {
                if (item.ItemType == type)
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 총 아이템 수
        /// </summary>
        public int ItemCount => _items.Count;

        #endregion
    }
}
