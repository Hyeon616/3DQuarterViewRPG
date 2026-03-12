using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class StatTreeUIConnector : NetworkBehaviour
{
    private PlayerController _player;
    private StatTreeUI _treeUI;
    private PlayerStatAllocation _allocation;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        _allocation = GetComponent<PlayerStatAllocation>();
        _treeUI = FindAnyObjectByType<StatTreeUI>();

        if (_allocation != null && _treeUI != null)
        {
            _treeUI.Initialize(_allocation);
            _treeUI.OnUIToggled += OnStatTreeToggled;
        }
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();

        if (_treeUI != null)
        {
            _treeUI.OnUIToggled -= OnStatTreeToggled;
        }
    }

    public void OnStatTree(InputValue value)
    {
        if (!isOwned) return;

        if (value.isPressed && _treeUI != null)
        {
            _treeUI.Toggle();
        }
    }

    private void OnStatTreeToggled(bool isOpen)
    {
        // UI 열리면 Player 입력 비활성화
        _player?.SetPlayerInputEnabled(!isOpen);
    }
}
