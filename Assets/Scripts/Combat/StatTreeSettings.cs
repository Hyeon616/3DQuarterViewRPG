using UnityEngine;

[CreateAssetMenu(fileName = "StatTreeSettings", menuName = "Combat/Stat Tree Settings")]
public class StatTreeSettings : ScriptableObject
{
    [Header("Tier UI")]
    [SerializeField] private Sprite leftInfoBackgroundSprite;
    [SerializeField] private Sprite lockIconSprite;

    [Header("Glow Effect")]
    [SerializeField] private Sprite glowSprite;

    public Sprite LeftInfoBackgroundSprite => leftInfoBackgroundSprite;
    public Sprite LockIconSprite => lockIconSprite;
    public Sprite GlowSprite => glowSprite;
}
