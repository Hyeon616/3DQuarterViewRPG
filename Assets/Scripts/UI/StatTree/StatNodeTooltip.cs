using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class StatNodeTooltip : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI currentEffectText;
    [SerializeField] private GameObject nextLevelSection;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private TextMeshProUGUI requiredPointsText;
    [SerializeField] private TextMeshProUGUI nextEffectText;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10f, 10f);

    private RectTransform _rectTransform;
    private Canvas _canvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    public void Show(StatNodeData nodeData, int currentPoints, Vector3 position)
    {
        if (nodeData == null) return;

        gameObject.SetActive(true);

        if (nameText != null)
            nameText.text = nodeData.NodeName;

        if (iconImage != null && nodeData.Icon != null)
            iconImage.sprite = nodeData.Icon;

        if (currentLevelText != null)
            currentLevelText.text = $"레벨: {currentPoints}/{nodeData.MaxPoints}";

        if (currentEffectText != null)
        {
            if (currentPoints > 0)
            {
                currentEffectText.text = FormatModifiers(nodeData.GetTotalModifiers(currentPoints));
                currentEffectText.gameObject.SetActive(true);
            }
            else
            {
                currentEffectText.gameObject.SetActive(false);
            }
        }

        bool canUpgrade = currentPoints < nodeData.MaxPoints;
        if (nextLevelSection != null)
            nextLevelSection.SetActive(canUpgrade);

        if (canUpgrade)
        {
            int nextLevel = currentPoints + 1;

            if (nextLevelText != null)
                nextLevelText.text = $"다음 레벨: {nextLevel}";

            if (requiredPointsText != null)
                requiredPointsText.text = $"필요 포인트: {nodeData.CostPerPoint}";

            if (nextEffectText != null)
                nextEffectText.text = FormatModifiers(nodeData.GetTotalModifiers(nextLevel));
        }

        UpdatePosition(position);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdatePosition(Vector3 anchorWorldPosition)
    {
        if (_rectTransform == null || _canvas == null) return;

        RectTransform canvasRect = _canvas.transform as RectTransform;
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, anchorWorldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            cam,
            out Vector2 localPoint
        );

        Canvas.ForceUpdateCanvases();
        Vector2 tooltipSize = _rectTransform.sizeDelta;
        Vector2 canvasSize = canvasRect.sizeDelta;

        float x = localPoint.x + offset.x;
        float y = localPoint.y;

        if (x + tooltipSize.x > canvasSize.x / 2)
            x = localPoint.x - tooltipSize.x - offset.x;

        float halfHeight = tooltipSize.y / 2;
        if (y + halfHeight > canvasSize.y / 2)
            y = canvasSize.y / 2 - halfHeight;

        if (y - halfHeight < -canvasSize.y / 2)
            y = -canvasSize.y / 2 + halfHeight;

        _rectTransform.anchoredPosition = new Vector2(x, y);
    }

    private string FormatModifiers(StatModifier[] modifiers)
    {
        if (modifiers == null || modifiers.Length == 0)
            return "효과 없음";

        var sb = new StringBuilder();
        for (int i = 0; i < modifiers.Length; i++)
        {
            if (i > 0) sb.Append("\n");
            sb.Append(FormatStatType(modifiers[i].StatType));
            sb.Append(": +");
            sb.Append(FormatValue(modifiers[i].StatType, modifiers[i].Value));
        }
        return sb.ToString();
    }

    private string FormatStatType(StatType type)
    {
        return type switch
        {
            StatType.CriticalChance => "치명타 확률",
            StatType.CriticalDamage => "치명타 피해",
            StatType.DamageIncrease => "피해 증가",
            StatType.AttackSpeed => "공격 속도",
            StatType.CooldownReduction => "쿨타임 감소",
            StatType.ManaReduction => "마나 소모 감소",
            _ => type.ToString()
        };
    }

    private string FormatValue(StatType type, float value)
    {
        if (type == StatType.CriticalChance || type == StatType.CriticalDamage ||
            type == StatType.DamageIncrease || type == StatType.AttackSpeed ||
            type == StatType.CooldownReduction || type == StatType.ManaReduction)
        {
            return $"{value:F1}%";
        }
        return value.ToString("F1");
    }
}
