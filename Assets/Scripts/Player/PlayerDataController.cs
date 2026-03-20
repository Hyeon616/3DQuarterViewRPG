using Mirror;
using UnityEngine;
using System;

/// <summary>
/// 플레이어 데이터 중앙 관리
/// 저장/로드 및 각 컴포넌트에 데이터 배포
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerDataController : NetworkBehaviour
{
    [Header("자동 저장")]
    [SerializeField] private float autoSaveInterval = 60f; // 초
    [SerializeField] private bool autoSaveEnabled = true;

    private PlayerSaveData _saveData;
    private float _lastSaveTime;
    private bool _isDirty;

    // 하위 컴포넌트 참조
    private PlayerEquipment _equipment;
    private PlayerStatAllocation _statAllocation;
    private PlayerStatController _statController;

    public PlayerSaveData SaveData => _saveData;
    public bool IsDirty => _isDirty;

    public event Action<PlayerSaveData> OnDataLoaded;
    public event Action OnDataSaved;

    public override void OnStartServer()
    {
        base.OnStartServer();
        CacheComponents();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        CacheComponents();

        // 변경 이벤트 구독
        if (_equipment != null)
            _equipment.OnEquipmentChanged += OnEquipmentChanged;
        if (_statAllocation != null)
            _statAllocation.OnAllocationChanged += OnAllocationChanged;

        // 로컬 플레이어만 저장 데이터 로드
        Load();
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();

        // 이벤트 구독 해제
        if (_equipment != null)
            _equipment.OnEquipmentChanged -= OnEquipmentChanged;
        if (_statAllocation != null)
            _statAllocation.OnAllocationChanged -= OnAllocationChanged;

        // 종료 전 저장
        if (_isDirty)
            Save();
    }

    private void OnEquipmentChanged()
    {
        MarkDirty();
    }

    private void OnAllocationChanged()
    {
        MarkDirty();
    }

    private void CacheComponents()
    {
        _equipment = GetComponent<PlayerEquipment>();
        _statAllocation = GetComponent<PlayerStatAllocation>();
        _statController = GetComponent<PlayerStatController>();
    }

    private void Update()
    {
        if (netIdentity == null || !isOwned) return;

        // 자동 저장
        if (autoSaveEnabled && _isDirty && Time.time - _lastSaveTime > autoSaveInterval)
        {
            Save();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (netIdentity == null || !isOwned) return;

        // 앱 일시정지 시 저장
        if (pause && _isDirty)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        if (netIdentity == null || !isOwned) return;

        // 종료 시 저장
        if (_isDirty)
        {
            Save();
        }
    }

    /// <summary>
    /// 데이터 로드 및 각 컴포넌트에 적용
    /// </summary>
    public void Load()
    {
        _saveData = SaveDataManager.Load();

        if (_saveData == null)
        {
            _saveData = new PlayerSaveData();
        }

        // 서버에 로드된 데이터 적용 요청
        CmdApplyLoadedData(
            _saveData.level,
            _saveData.equippedWeaponId,
            _saveData.equippedArmorId,
            _saveData.availableStatPoints,
            _saveData.totalStatPoints,
            SerializeStatAllocations()
        );

        OnDataLoaded?.Invoke(_saveData);
        _isDirty = false;
        _lastSaveTime = Time.time;

        Debug.Log($"[PlayerData] Loaded - Level: {_saveData.level}, Weapon: {_saveData.equippedWeaponId}, Armor: {_saveData.equippedArmorId}");
    }

    /// <summary>
    /// 데이터 저장
    /// </summary>
    public void Save()
    {
        if (_saveData == null) return;

        // 현재 상태 수집
        CollectCurrentState();

        // 파일로 저장
        if (SaveDataManager.Save(_saveData))
        {
            _isDirty = false;
            _lastSaveTime = Time.time;
            OnDataSaved?.Invoke();
            Debug.Log("[PlayerData] Saved");
        }
    }

    /// <summary>
    /// 현재 게임 상태를 SaveData에 수집
    /// </summary>
    private void CollectCurrentState()
    {
        if (_saveData == null) return;

        // 레벨
        if (_statController != null)
        {
            _saveData.level = _statController.Level;
        }

        // 장비
        if (_equipment != null)
        {
            _saveData.equippedWeaponId = _equipment.CurrentWeapon != null
                ? EquipmentDatabase.Instance?.GetWeaponId(_equipment.CurrentWeapon) ?? -1
                : -1;
            _saveData.equippedArmorId = _equipment.CurrentArmor != null
                ? EquipmentDatabase.Instance?.GetArmorId(_equipment.CurrentArmor) ?? -1
                : -1;
        }

        // StatTree 할당
        CollectStatAllocations();
    }

    private void CollectStatAllocations()
    {
        if (_statAllocation == null || _saveData == null) return;

        _saveData.statAllocations.Clear();
        _saveData.availableStatPoints = _statAllocation.AvailablePoints;
        _saveData.totalStatPoints = _statAllocation.TotalPoints;

        var statTree = _statAllocation.StatTree;
        if (statTree == null) return;

        for (int tierIdx = 0; tierIdx < statTree.TierCount; tierIdx++)
        {
            var tier = statTree.GetTier(tierIdx);
            if (tier?.Nodes == null) continue;

            for (int nodeIdx = 0; nodeIdx < tier.Nodes.Length; nodeIdx++)
            {
                int points = _statAllocation.GetAllocatedPoints(tierIdx, nodeIdx);
                if (points > 0)
                {
                    _saveData.statAllocations.Add(new StatAllocationEntry(tierIdx, nodeIdx, points));
                }
            }
        }
    }

    /// <summary>
    /// 데이터 변경 표시 (다음 자동저장에 포함)
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    #region Network Commands

    [Command]
    private void CmdApplyLoadedData(int level, int weaponId, int armorId, int availablePoints, int totalPoints, byte[] statAllocations)
    {
        // 레벨 적용
        if (_statController != null)
        {
            _statController.SetLevel(level);
        }

        // 장비 적용
        if (_equipment != null)
        {
            if (weaponId >= 0)
                _equipment.EquipWeaponById(weaponId);
            if (armorId >= 0)
                _equipment.EquipArmorById(armorId);
        }

        // StatTree 적용
        ApplyStatAllocations(availablePoints, totalPoints, statAllocations);
    }

    [Server]
    private void ApplyStatAllocations(int availablePoints, int totalPoints, byte[] data)
    {
        if (_statAllocation == null) return;

        // 포인트 설정은 Allocation 내부에서 처리하도록 수정 필요
        // 여기서는 할당 데이터만 적용

        var allocations = DeserializeStatAllocations(data);
        foreach (var entry in allocations)
        {
            // 직접 할당 (검증 없이) - 저장된 데이터이므로
            _statAllocation.ServerSetAllocation(entry.tierIndex, entry.nodeIndex, entry.points);
        }

        _statAllocation.ServerSetPoints(availablePoints, totalPoints);
    }

    #endregion

    #region Serialization Helpers

    private byte[] SerializeStatAllocations()
    {
        if (_saveData == null || _saveData.statAllocations == null)
            return new byte[0];

        // 간단한 직렬화: tierIndex, nodeIndex, points를 int로 직렬화
        var list = _saveData.statAllocations;
        byte[] data = new byte[list.Count * 12]; // 3 ints * 4 bytes

        for (int i = 0; i < list.Count; i++)
        {
            int offset = i * 12;
            Buffer.BlockCopy(BitConverter.GetBytes(list[i].tierIndex), 0, data, offset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(list[i].nodeIndex), 0, data, offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(list[i].points), 0, data, offset + 8, 4);
        }

        return data;
    }

    private StatAllocationEntry[] DeserializeStatAllocations(byte[] data)
    {
        if (data == null || data.Length == 0)
            return new StatAllocationEntry[0];

        int count = data.Length / 12;
        var result = new StatAllocationEntry[count];

        for (int i = 0; i < count; i++)
        {
            int offset = i * 12;
            int tierIndex = BitConverter.ToInt32(data, offset);
            int nodeIndex = BitConverter.ToInt32(data, offset + 4);
            int points = BitConverter.ToInt32(data, offset + 8);
            result[i] = new StatAllocationEntry(tierIndex, nodeIndex, points);
        }

        return result;
    }

    #endregion
}
