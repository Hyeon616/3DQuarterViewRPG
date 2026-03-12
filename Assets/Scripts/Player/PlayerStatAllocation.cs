using Mirror;
using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
public class PlayerStatAllocation : NetworkBehaviour
{
    [Header("스탯 트리")]
    [SerializeField] private StatTreeData statTree;
    [SerializeField] private int initialPoints = 100; // 테스트용 초기 포인트

    [SyncVar(hook = nameof(OnAvailablePointsChanged))]
    private int _availablePoints;

    [SyncVar]
    private int _totalPoints; // 초기 총 포인트 (변하지 않음)

    // 티어별, 노드별 투자 포인트 (2D 배열을 1D로 직렬화)
    private readonly SyncList<int> _allocatedPoints = new SyncList<int>();

    private int[] _tierPointsSpent;
    private int _totalNodes;

    public StatTreeData StatTree => statTree;
    public int AvailablePoints => _availablePoints;
    public int SpentPoints
    {
        get
        {
            if (statTree == null) return 0;

            int total = 0;
            int nodeIndex = 0;
            for (int tierIdx = 0; tierIdx < statTree.TierCount; tierIdx++)
            {
                var tier = statTree.GetTier(tierIdx);
                if (tier?.Nodes == null) continue;

                foreach (var node in tier.Nodes)
                {
                    if (nodeIndex < _allocatedPoints.Count)
                    {
                        int investments = _allocatedPoints[nodeIndex];
                        int cost = node?.CostPerPoint ?? 1;
                        total += investments * cost;
                    }
                    nodeIndex++;
                }
            }
            return total;
        }
    }
    public int TotalPoints => _totalPoints;

    public event Action OnAllocationChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializeAllocation();
        _availablePoints = initialPoints;
        _totalPoints = initialPoints; // 총 포인트 고정
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        InitializeLocalCache();
        _allocatedPoints.Callback += OnAllocatedPointsChanged;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _allocatedPoints.Callback -= OnAllocatedPointsChanged;
    }

    private void InitializeAllocation()
    {
        if (statTree == null) return;

        _totalNodes = 0;
        foreach (var tier in statTree.Tiers)
        {
            _totalNodes += tier.Nodes?.Length ?? 0;
        }

        _allocatedPoints.Clear();
        for (int i = 0; i < _totalNodes; i++)
        {
            _allocatedPoints.Add(0);
        }

        _tierPointsSpent = new int[statTree.TierCount];
    }

    private void InitializeLocalCache()
    {
        if (statTree == null) return;
        _tierPointsSpent = new int[statTree.TierCount];
        RecalculateTierPoints();
    }

    private void OnAllocatedPointsChanged(SyncList<int>.Operation op, int index, int oldValue, int newValue)
    {
        RecalculateTierPoints();
        // UI 업데이트는 OnAvailablePointsChanged에서 처리 (동기화 타이밍 보장)
    }

    private void OnAvailablePointsChanged(int oldValue, int newValue)
    {
        // SyncVar hook - _availablePoints가 동기화되면 UI 업데이트
        OnAllocationChanged?.Invoke();
    }

    private void RecalculateTierPoints()
    {
        if (statTree == null || _tierPointsSpent == null) return;

        Array.Clear(_tierPointsSpent, 0, _tierPointsSpent.Length);

        int nodeIndex = 0;
        for (int tierIdx = 0; tierIdx < statTree.TierCount; tierIdx++)
        {
            var tier = statTree.GetTier(tierIdx);
            if (tier?.Nodes == null) continue;

            foreach (var node in tier.Nodes)
            {
                if (nodeIndex < _allocatedPoints.Count)
                {
                    // 투자 횟수 * 노드당 비용 = 실제 소모 포인트
                    int investments = _allocatedPoints[nodeIndex];
                    int cost = node?.CostPerPoint ?? 1;
                    _tierPointsSpent[tierIdx] += investments * cost;
                }
                nodeIndex++;
            }
        }
    }

    public int GetAllocatedPoints(int tierIndex, int nodeIndex)
    {
        int flatIndex = GetFlatIndex(tierIndex, nodeIndex);
        if (flatIndex < 0 || flatIndex >= _allocatedPoints.Count) return 0;
        return _allocatedPoints[flatIndex];
    }

    public int GetTierPointsSpent(int tierIndex)
    {
        if (_tierPointsSpent == null || tierIndex < 0 || tierIndex >= _tierPointsSpent.Length)
            return 0;
        return _tierPointsSpent[tierIndex];
    }

    /// <summary>
    /// 티어가 해금되었는지 (한 번이라도 조건을 충족했는지)
    /// 투자 취소로 조건이 안 맞아도 해금 상태는 유지됨
    /// </summary>
    public bool IsTierUnlocked(int tierIndex)
    {
        if (tierIndex <= 0) return true;

        // 이 티어에 투자가 있으면 이미 해금된 것
        if (GetTierPointsSpent(tierIndex) > 0) return true;

        var prevTier = statTree?.GetTier(tierIndex - 1);
        if (prevTier == null) return false;

        return GetTierPointsSpent(tierIndex - 1) >= prevTier.RequiredPointsToUnlockNext;
    }

    /// <summary>
    /// 티어가 활성 상태인지 (현재 조건을 충족하고 있는지)
    /// 하위 티어 투자 취소 시 비활성화될 수 있음 (재귀적으로 확인)
    /// </summary>
    public bool IsTierActive(int tierIndex)
    {
        if (tierIndex <= 0) return true;

        // 이전 티어가 활성 상태인지 먼저 확인 (재귀)
        if (!IsTierActive(tierIndex - 1)) return false;

        var prevTier = statTree?.GetTier(tierIndex - 1);
        if (prevTier == null) return false;

        return GetTierPointsSpent(tierIndex - 1) >= prevTier.RequiredPointsToUnlockNext;
    }

    public bool CanAllocate(int tierIndex, int nodeIndex)
    {
        // 비활성 티어에는 투자 불가 (IsTierActive 사용)
        if (!IsTierActive(tierIndex)) return false;

        var tier = statTree?.GetTier(tierIndex);
        var node = tier?.Nodes?[nodeIndex];
        if (node == null) return false;

        // 투자 비용 확인
        int cost = node.CostPerPoint;
        if (_availablePoints < cost) return false;

        // 노드 개별 최대 투자 확인
        int current = GetAllocatedPoints(tierIndex, nodeIndex);
        if (current >= node.MaxPoints) return false;

        // 티어 전체 최대 투자 확인
        int tierSpent = GetTierPointsSpent(tierIndex);
        if (tierSpent + cost > tier.MaxTierPoints) return false;

        return true;
    }

    public int GetNodeCost(int tierIndex, int nodeIndex)
    {
        var tier = statTree?.GetTier(tierIndex);
        var node = tier?.Nodes?[nodeIndex];
        return node?.CostPerPoint ?? 1;
    }

    public bool CanDeallocate(int tierIndex, int nodeIndex)
    {
        int current = GetAllocatedPoints(tierIndex, nodeIndex);
        if (current <= 0) return false;

        // 하위 티어 투자 취소 시 상위 티어가 비활성화될 수 있지만,
        // 취소 자체는 허용 (UX 개선)
        return true;
    }

    [Command]
    public void CmdAllocatePoint(int tierIndex, int nodeIndex)
    {
        if (!CanAllocate(tierIndex, nodeIndex)) return;

        int flatIndex = GetFlatIndex(tierIndex, nodeIndex);
        if (flatIndex < 0 || flatIndex >= _allocatedPoints.Count) return;

        int cost = GetNodeCost(tierIndex, nodeIndex);
        _allocatedPoints[flatIndex]++;
        _availablePoints -= cost;
        RecalculateTierPoints();
    }

    [Command]
    public void CmdDeallocatePoint(int tierIndex, int nodeIndex)
    {
        if (!CanDeallocate(tierIndex, nodeIndex)) return;

        int flatIndex = GetFlatIndex(tierIndex, nodeIndex);
        if (flatIndex < 0 || flatIndex >= _allocatedPoints.Count) return;

        int cost = GetNodeCost(tierIndex, nodeIndex);
        _allocatedPoints[flatIndex]--;
        _availablePoints += cost;
        RecalculateTierPoints();
    }

    [Server]
    public void AddPoints(int points)
    {
        _availablePoints += points;
        _totalPoints += points;
    }

    private int GetFlatIndex(int tierIndex, int nodeIndex)
    {
        if (statTree == null) return -1;

        int flatIndex = 0;
        for (int t = 0; t < tierIndex; t++)
        {
            var tier = statTree.GetTier(t);
            flatIndex += tier?.Nodes?.Length ?? 0;
        }
        flatIndex += nodeIndex;

        return flatIndex;
    }

    public Dictionary<StatType, float> GetTotalModifiers()
    {
        var totals = new Dictionary<StatType, float>();

        if (statTree == null) return totals;

        int nodeIndex = 0;
        for (int tierIdx = 0; tierIdx < statTree.TierCount; tierIdx++)
        {
            var tier = statTree.GetTier(tierIdx);
            if (tier?.Nodes == null) continue;

            // 비활성 티어는 효과 적용 안 함
            bool isActive = IsTierActive(tierIdx);

            foreach (var node in tier.Nodes)
            {
                if (nodeIndex < _allocatedPoints.Count)
                {
                    int points = _allocatedPoints[nodeIndex];
                    if (points > 0 && isActive)
                    {
                        var modifiers = node.GetTotalModifiers(points);
                        foreach (var mod in modifiers)
                        {
                            if (!totals.ContainsKey(mod.StatType))
                                totals[mod.StatType] = 0f;
                            totals[mod.StatType] += mod.Value;
                        }
                    }
                }
                nodeIndex++;
            }
        }

        return totals;
    }
}
