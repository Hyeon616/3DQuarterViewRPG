using UnityEngine;

/// <summary>
/// 퀘스트 아이템
/// </summary>
[CreateAssetMenu(fileName = "Quest_", menuName = "Items/Quest Item")]
public class QuestItemData : ItemData
{
    [Header("퀘스트 정보")]
    [SerializeField] private int questId = -1;

    public int QuestId => questId;

    public QuestItemData()
    {
        isStackable = true;
        maxStackSize = 99;
        isTradeable = false;
        isDroppable = false;
    }
}
