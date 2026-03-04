using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectData", menuName = "Combat/Effect Data")]
public class EffectData : ScriptableObject
{
    [SerializeField] private List<GameObject> effectPrefabs = new();

    [Header("공통 타격 이펙트")]
    [SerializeField] private GameObject defaultHitEffectPrefab;
    [SerializeField] private GameObject defaultBonusHitEffectPrefab;

    public IReadOnlyList<GameObject> EffectPrefabs => effectPrefabs;
    public GameObject DefaultHitEffectPrefab => defaultHitEffectPrefab;
    public GameObject DefaultBonusHitEffectPrefab => defaultBonusHitEffectPrefab;

#if UNITY_EDITOR
    public void SetPrefabs(List<GameObject> prefabs)
    {
        effectPrefabs = prefabs;
    }
#endif
}
