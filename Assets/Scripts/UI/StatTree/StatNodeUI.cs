using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StatNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image raycastTarget;    // 전체 영역 레이캐스트용 (투명)
    [SerializeField] private Image backgroundFrame;  // 원형 배경 프레임
    [SerializeField] private Image iconMask;         // 원형 마스크 (Mask 컴포넌트 필요)
    [SerializeField] private Image iconImage;        // 실제 아이콘 (iconMask의 자식)
    [SerializeField] private RectTransform iconContainer; // 아이콘 컨테이너 (툴팁 호버 감지용)
    [SerializeField] private TextMeshProUGUI costText;    // 필요 포인트 (IconContainer 하단에 겹쳐서 표시)
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Image pointsBackground; // PointsText 배경
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;

    [Header("Tooltip")]
    [SerializeField] private StatNodeTooltip tooltip;

    [Header("Brightness")]
    [SerializeField] private Color dimmedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.6f, 0.4f, 0.4f, 1f); // 비활성 상태 (붉은 톤)

    private PlayerStatAllocation _allocation;
    private StatNodeData _nodeData;
    private int _tierIndex;
    private int _nodeIndex;
    private bool _isHovered;
    private bool _isIconHovered;
    private Camera _uiCamera;

    public void Initialize(PlayerStatAllocation allocation, StatNodeData nodeData, int tierIndex, int nodeIndex, StatNodeTooltip tooltipOverride = null)
    {
        // 기존 리스너 제거 (재초기화 시 중복 방지)
        addButton?.onClick.RemoveListener(OnAddClicked);
        removeButton?.onClick.RemoveListener(OnRemoveClicked);

        _allocation = allocation;
        _nodeData = nodeData;
        _tierIndex = tierIndex;
        _nodeIndex = nodeIndex;

        // 런타임에 전달된 tooltip이 있으면 사용
        if (tooltipOverride != null)
            tooltip = tooltipOverride;

        if (iconImage != null && nodeData.Icon != null)
            iconImage.sprite = nodeData.Icon;

        // 필요 포인트 표시
        if (costText != null)
            costText.text = $"{nodeData.CostPerPoint}p";

        addButton?.onClick.AddListener(OnAddClicked);
        removeButton?.onClick.AddListener(OnRemoveClicked);

        // 초기에는 버튼 숨김
        SetButtonsVisible(false);
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        addButton?.onClick.RemoveListener(OnAddClicked);
        removeButton?.onClick.RemoveListener(OnRemoveClicked);
    }

    private void Update()
    {
        if (!_isHovered) return;

        // 아이콘 영역 위에 있는지 체크하여 툴팁 표시/숨김
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
        // 툴팁은 Update에서 아이콘 호버 체크로 처리
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

        // 노드의 오른쪽 끝 위치 계산 (RectTransform 기준)
        RectTransform nodeRect = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        nodeRect.GetWorldCorners(corners);
        // corners[2]는 오른쪽 상단, corners[3]는 오른쪽 하단
        // 오른쪽 중앙 위치 사용
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
        UpdateDisplay(true); // 기본값: 활성 상태
    }

    public void UpdateDisplay(bool isTierActive)
    {
        if (_allocation == null || _nodeData == null) return;

        int current = _allocation.GetAllocatedPoints(_tierIndex, _nodeIndex);
        int max = _nodeData.MaxPoints;

        if (pointsText != null)
            pointsText.text = $"{current}/{max}";

        // 버튼 interactable 상태
        if (addButton != null)
            addButton.interactable = _allocation.CanAllocate(_tierIndex, _nodeIndex);

        if (removeButton != null)
            removeButton.interactable = _allocation.CanDeallocate(_tierIndex, _nodeIndex);

        // 버튼 visibility 업데이트
        UpdateButtonVisibility();

        // 투자 여부 및 티어 활성 상태에 따른 밝기 조절
        UpdateBrightness(current > 0, isTierActive);
    }

    private void UpdateBrightness(bool isInvested, bool isTierActive = true)
    {
        Color targetColor;

        if (!isTierActive && isInvested)
        {
            // 비활성 티어에 투자된 상태 (붉은 톤으로 비활성 표시)
            targetColor = inactiveColor;
        }
        else if (isInvested)
        {
            // 활성 티어에 투자된 상태
            targetColor = normalColor;
        }
        else
        {
            // 투자되지 않은 상태
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
}
