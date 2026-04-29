using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 모든 UI 입력을 통합 관리하는 Connector
/// PlayerController에 붙어서 PlayerInput 메시지를 수신
/// PlayerInput은 로컬 플레이어에게만 활성화되므로 별도 권한 체크 불필요
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class UIInputConnector : MonoBehaviour
{
    private PlayerController _player;

    // UI References (lazy initialization)
    private StatTreeUI _statTreeUI;
    private InventoryUI _inventoryUI;
    private EquipmentUI _equipmentUI;
    private ClientNetworkUI _networkUI;
    private bool _isInitialized;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // StatTreeUI
        _statTreeUI = FindAnyObjectByType<StatTreeUI>();
        if (_statTreeUI != null)
        {
            var allocation = GetComponent<PlayerStatAllocation>();
            if (allocation != null)
            {
                _statTreeUI.Initialize(allocation);
            }
            _statTreeUI.OnUIToggled += OnAnyUIToggled;
        }

        // InventoryUI
        var inventoryUIs = FindObjectsByType<InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _inventoryUI = inventoryUIs.Length > 0 ? inventoryUIs[0] : null;
        if (_inventoryUI != null)
        {
            var inventory = GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                _inventoryUI.Initialize(inventory);
            }
            _inventoryUI.OnUIToggled += OnAnyUIToggled;
        }

        // EquipmentUI
        var equipmentUIs = FindObjectsByType<EquipmentUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _equipmentUI = equipmentUIs.Length > 0 ? equipmentUIs[0] : null;
        if (_equipmentUI != null)
        {
            var equipment = GetComponent<PlayerEquipment>();
            var statController = GetComponent<PlayerStatController>();
            if (equipment != null && statController != null)
            {
                _equipmentUI.Initialize(equipment, statController);
            }
            _equipmentUI.OnUIToggled += OnAnyUIToggled;
        }

        // ClientNetworkUI
        var networkUIs = FindObjectsByType<ClientNetworkUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _networkUI = networkUIs.Length > 0 ? networkUIs[0] : null;
        if (_networkUI != null)
        {
            _networkUI.OnUIToggled += OnAnyUIToggled;
        }
    }

    private void OnDestroy()
    {
        if (_statTreeUI != null)
            _statTreeUI.OnUIToggled -= OnAnyUIToggled;

        if (_inventoryUI != null)
            _inventoryUI.OnUIToggled -= OnAnyUIToggled;

        if (_equipmentUI != null)
            _equipmentUI.OnUIToggled -= OnAnyUIToggled;

        if (_networkUI != null)
            _networkUI.OnUIToggled -= OnAnyUIToggled;
    }

    #region Input Handlers

    public void OnStatTree(InputValue value)
    {
        if (!value.isPressed) return;
        Initialize();

        if (_statTreeUI != null)
        {
            _statTreeUI.Toggle();
        }
    }

    public void OnInventory(InputValue value)
    {
        if (!value.isPressed) return;
        Initialize();

        if (_inventoryUI != null)
        {
            _inventoryUI.Toggle();
        }
    }

    public void OnEquipment(InputValue value)
    {
        if (!value.isPressed) return;
        Initialize();

        if (_equipmentUI != null)
        {
            _equipmentUI.Toggle();
        }
    }

    public void OnExit(InputValue value)
    {
        if (!value.isPressed) return;
        Initialize();

        if (_networkUI != null)
        {
            _networkUI.Toggle();
        }
    }

    #endregion

    private void OnAnyUIToggled(bool isOpen)
    {
        UpdatePlayerInput();
    }

    private void UpdatePlayerInput()
    {
        bool anyUIOpen = (_statTreeUI != null && _statTreeUI.IsOpen) ||
                         (_inventoryUI != null && _inventoryUI.IsOpen) ||
                         (_equipmentUI != null && _equipmentUI.IsOpen) ||
                         (_networkUI != null && _networkUI.IsOpen);

        _player?.SetPlayerInputEnabled(!anyUIOpen);
    }
}
