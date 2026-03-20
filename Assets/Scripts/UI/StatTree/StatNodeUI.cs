using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StatNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image raycastTarget;
    [SerializeField] private Image backgroundFrame;
    [SerializeField] private Image iconMask;
    [SerializeField] private Image iconImage;
    [SerializeField] private RectTransform iconContainer;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Image pointsBackground;
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;

    [Header("Tooltip")]
    [SerializeField] private StatNodeTooltip tooltip;

    [Header("Brightness")]
    [SerializeField] private Color dimmedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.6f, 0.4f, 0.4f, 1f);

    [Header("Max Points Glow Effect")]
    [SerializeField] private Image glowImage;
    [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0.3f, 1f);
    [SerializeField, Range(1f, 5f)] private float glowIntensity = 2.5f;

    private PlayerStatAllocation _allocation;
    private StatNodeData _nodeData;
    private int _tierIndex;
    private int _nodeIndex;
    private bool _isHovered;
    private bool _isIconHovered;
    private Camera _uiCamera;
    private bool _isMaxed;
    private Material _glowMaterial;

    public void Initialize(PlayerStatAllocation allocation, StatNodeData nodeData, int tierIndex, int nodeIndex, StatNodeTooltip tooltipOverride = null)
    {
        addButton?.onClick.RemoveListener(OnAddClicked);
        removeButton?.onClick.RemoveListener(OnRemoveClicked);

        _allocation = allocation;
        _nodeData = nodeData;
        _tierIndex = tierIndex;
        _nodeIndex = nodeIndex;

        if (tooltipOverride != null)
            tooltip = tooltipOverride;

        if (iconImage != null && nodeData.Icon != null)
            iconImage.sprite = nodeData.Icon;

        if (costText != null)
            costText.text = $"{nodeData.CostPerPoint}p";

        addButton?.onClick.AddListener(OnAddClicked);
        removeButton?.onClick.AddListener(OnRemoveClicked);

        SetButtonsVisible(false);
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        addButton?.onClick.RemoveListener(OnAddClicked);
        removeButton?.onClick.RemoveListener(OnRemoveClicked);
        if (_glowMaterial != null)
            Destroy(_glowMaterial);
    }

    private void Update()
    {
        if (!_isHovered) return;

        bool wasIconHovered = _isIconHovered;
        _isIconHovered = IsPointerOverIcon();

        if (_isIconHovered && !wasIconHovered)
            ShowTooltip();
        else if (!_isIconHovered && wasIconHovered)
            HideTooltip();
    }

    private bool IsPointerOverIcon()
    {
        if (iconContainer == null) return false;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            iconContainer,
            Input.mousePosition,
            _uiCamera,
            out localPoint
        );

        return iconContainer.rect.Contains(localPoint);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _uiCamera = eventData.enterEventCamera;
        UpdateButtonVisibility();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _isIconHovered = false;
        UpdateButtonVisibility();
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltip == null || _nodeData == null || _allocation == null) return;

        int currentPoints = _allocation.GetAllocatedPoints(_tierIndex, _nodeIndex);

        RectTransform nodeRect = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        nodeRect.GetWorldCorners(corners);
        Vector3 rightCenter = (corners[2] + corners[3]) / 2f;

        tooltip.Show(_nodeData, currentPoints, rightCenter);
    }

    private void HideTooltip()
    {
        if (tooltip != null)
            tooltip.Hide();
    }

    private void UpdateButtonVisibility()
    {
        if (_allocation == null) return;

        bool isUnlocked = _allocation.IsTierUnlocked(_tierIndex);
        bool showButtons = _isHovered && isUnlocked;

        SetButtonsVisible(showButtons);
    }

    private void SetButtonsVisible(bool visible)
    {
        if (addButton != null)
            addButton.gameObject.SetActive(visible);

        if (removeButton != null)
            removeButton.gameObject.SetActive(visible);
    }

    public void UpdateDisplay()
    {
        UpdateDisplay(true);
    }

    public void UpdateDisplay(bool isTierActive)
    {
        if (_allocation == null || _nodeData == null) return;

        int current = _allocation.GetAllocatedPoints(_tierIndex, _nodeIndex);
        int max = _nodeData.MaxPoints;

        if (pointsText != null)
            pointsText.text = $"{current}/{max}";

        if (addButton != null)
            addButton.interactable = _allocation.CanAllocate(_tierIndex, _nodeIndex);

        if (removeButton != null)
            removeButton.interactable = _allocation.CanDeallocate(_tierIndex, _nodeIndex);

        UpdateButtonVisibility();
        UpdateBrightness(current > 0, isTierActive);

        bool isNowMaxed = current >= max && isTierActive;
        if (isNowMaxed != _isMaxed)
        {
            _isMaxed = isNowMaxed;
            if (_isMaxed)
                StartGlowEffect();
            else
                StopGlowEffect();
        }
    }

    private void UpdateBrightness(bool isInvested, bool isTierActive = true)
    {
        Color targetColor;

        if (!isTierActive && isInvested)
        {
            targetColor = inactiveColor;
        }
        else if (isInvested)
        {
            targetColor = normalColor;
        }
        else
        {
            targetColor = dimmedColor;
        }

        if (iconImage != null)
            iconImage.color = targetColor;

        if (pointsText != null)
            pointsText.color = targetColor;
    }

    private void OnAddClicked()
    {
        if (_allocation == null) return;
        _allocation.CmdAllocatePoint(_tierIndex, _nodeIndex);
    }

    private void OnRemoveClicked()
    {
        if (_allocation == null) return;
        _allocation.CmdDeallocatePoint(_tierIndex, _nodeIndex);
    }

    private void SetupGlowMaterial()
    {
        if (glowImage == null || _glowMaterial != null) return;

        _glowMaterial = new Material(Shader.Find("UI/Default"));
        _glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        glowImage.material = _glowMaterial;
    }

    private void StartGlowEffect()
    {
        if (glowImage == null) return;

        SetupGlowMaterial();
        glowImage.gameObject.SetActive(true);
        glowImage.color = new Color(
            glowColor.r * glowIntensity,
            glowColor.g * glowIntensity,
            glowColor.b * glowIntensity,
            glowColor.a
        );
    }

    private void StopGlowEffect()
    {
        if (glowImage != null)
            glowImage.gameObject.SetActive(false);
    }
}
