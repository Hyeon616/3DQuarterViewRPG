using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 인벤토리/장비 UI와 플레이어 입력 연결 (NetworkBehaviour)
/// StatTreeUI 패턴 - OnStartAuthority에서 로컬 플레이어만 초기화
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerInventory))]
public class InventoryUIConnector : NetworkBehaviour
{
    private PlayerController _player;
    private PlayerInventory _inventory;
    private InventoryUI _inventoryUI;
    private EquipmentUI _equipmentUI;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        _inventory = GetComponent<PlayerInventory>();
        var equipment = GetComponent<PlayerEquipment>();
        var statController = GetComponent<PlayerStatController>();

        // 비활성화된 오브젝트도 찾기 위해 FindObjectsByType 사용
        var inventoryUIs = FindObjectsByType<InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _inventoryUI = inventoryUIs.Length > 0 ? inventoryUIs[0] : null;

        var equipmentUIs = FindObjectsByType<EquipmentUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _equipmentUI = equipmentUIs.Length > 0 ? equipmentUIs[0] : null;

        // InventoryUI 초기화
        if (_inventoryUI != null && _inventory != null)
        {
            _inventoryUI.Initialize(_inventory);
            _inventoryUI.OnUIToggled += OnInventoryUIToggled;
            Debug.Log("[InventoryUIConnector] InventoryUI Initialized");
        }

        // EquipmentUI 초기화
        if (_equipmentUI != null && equipment != null && statController != null)
        {
            _equipmentUI.Initialize(equipment, statController);
            _equipmentUI.OnUIToggled += OnEquipmentUIToggled;
            Debug.Log("[InventoryUIConnector] EquipmentUI Initialized");
        }
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();

        if (_inventoryUI != null)
        {
            _inventoryUI.OnUIToggled -= OnInventoryUIToggled;
        }

        if (_equipmentUI != null)
        {
            _equipmentUI.OnUIToggled -= OnEquipmentUIToggled;
        }
    }

    /// <summary>
    /// Input System - Inventory 액션 (I 키)
    /// </summary>
    public void OnInventory(InputValue value)
    {
        if (!isOwned) return;

        if (value.isPressed && _inventoryUI != null)
        {
            _inventoryUI.Toggle();
        }
    }

    /// <summary>
    /// Input System - Equipment 액션 (P 키)
    /// </summary>
    public void OnEquipment(InputValue value)
    {
        if (!isOwned) return;

        if (value.isPressed && _equipmentUI != null)
        {
            _equipmentUI.Toggle();
        }
    }

    private void OnInventoryUIToggled(bool isOpen)
    {
        UpdatePlayerInput();
    }

    private void OnEquipmentUIToggled(bool isOpen)
    {
        UpdatePlayerInput();
    }

    /// <summary>
    /// 어떤 UI라도 열려있으면 플레이어 입력 비활성화
    /// </summary>
    private void UpdatePlayerInput()
    {
        bool anyUIOpen = (_inventoryUI != null && _inventoryUI.IsOpen) ||
                         (_equipmentUI != null && _equipmentUI.IsOpen);

        _player?.SetPlayerInputEnabled(!anyUIOpen);
    }
}
