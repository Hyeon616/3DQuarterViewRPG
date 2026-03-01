using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectDatabase", menuName = "Combat/Effect Database")]
public class EffectDatabase : ScriptableObject
{
    [SerializeField] private List<GameObject> effectPrefabs = new();

    public IReadOnlyList<GameObject> EffectPrefabs => effectPrefabs;

#if UNITY_EDITOR
    public void SetPrefabs(List<GameObject> prefabs)
    {
        effectPrefabs = prefabs;
    }
#endif
}