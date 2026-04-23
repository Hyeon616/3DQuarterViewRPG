using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    /// <summary>
    /// 아이템 ID와 아이콘 스프라이트를 매핑하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ItemIconDatabase", menuName = "Items/Item Icon Database")]
    public class ItemIconDatabase : ScriptableObject
    {
        [Serializable]
        public class ItemIconEntry
        {
            public int itemId = -1;
            public Sprite icon;
        }

        [Header("아이템 아이콘 매핑")]
        [SerializeField] private List<ItemIconEntry> itemIcons = new List<ItemIconEntry>();

        [Header("기본 아이콘")]
        [SerializeField] private Sprite defaultIcon;

        private Dictionary<int, Sprite> _lookup;

        private void OnEnable()
        {
            BuildLookup();
        }


        private void BuildLookup()
        {
            _lookup = new Dictionary<int, Sprite>();
            foreach (var entry in itemIcons)
            {
                if (entry.itemId >= 0 && entry.icon != null)
                {
                    _lookup[entry.itemId] = entry.icon;
                }
            }
        }

        /// <summary>
        /// 아이템 ID로 아이콘 조회
        /// </summary>
        public Sprite GetIcon(int itemId)
        {
            if (_lookup == null) BuildLookup();
            if (_lookup.TryGetValue(itemId, out var icon))
            {
                return icon;
            }
            return defaultIcon;
        }

        /// <summary>
        /// 에디터에서 새 아이콘 추가/업데이트
        /// </summary>
        public void SetIcon(int itemId, Sprite icon)
        {
            var existing = itemIcons.Find(e => e.itemId == itemId);
            if (existing != null)
            {
                existing.icon = icon;
            }
            else
            {
                itemIcons.Add(new ItemIconEntry
                {
                    itemId = itemId,
                    icon = icon
                });
            }
            BuildLookup();
        }

        /// <summary>
        /// 모든 아이콘 엔트리 반환
        /// </summary>
        public List<ItemIconEntry> GetAllEntries() => itemIcons;

        /// <summary>
        /// 기본 아이콘
        /// </summary>
        public Sprite DefaultIcon => defaultIcon;
    }
}
