using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private DamageText prefab;

    [Header("Settings")]
    [SerializeField] private Canvas canvas;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f);           // FFFFFF
    [SerializeField] private Color criticalColor = new Color(1f, 0.93f, 0f);      // FFED00
    [SerializeField] private Color shieldColor = new Color(0.46f, 0.9f, 1f);      // 75E5FF
    [SerializeField] private Color staggerColor = new Color(1f, 0.2f, 0f);        // FF3300
    [SerializeField] private Color bonusColor = new Color(1f, 0.6f, 0f);          // FF9900

    [Header("Bonus Text")]
    [SerializeField] private string backAttackText = "백어택";
    [SerializeField] private string headAttackText = "헤드어택";

    private DamageResolver _resolver;

    private void Awake()
    {
        _resolver = new DamageResolver(backAttackText, headAttackText);
    }

    public void Spawn(DamageEventData data)
    {
        if (canvas == null || prefab == null) return;

        var color = GetColor(data.DamageType);
        var text = Instantiate(prefab, canvas.transform);
        text.Initialize(data.Damage, data.Position, color);

        if (_resolver.IsBonusHit(data.AttackType, data.HitDirection))
        {
            string bonus = _resolver.GetBonusText(data.HitDirection);
            text.SetBonus(bonus, bonusColor);
        }
    }

    private Color GetColor(DamageType type)
    {
        return type switch
        {
            DamageType.Critical => criticalColor,
            DamageType.Shield => shieldColor,
            DamageType.Stagger => staggerColor,
            _ => normalColor
        };
    }
}