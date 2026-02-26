using System;
using UnityEngine;

/// <summary>
/// 플레이어 컴포넌트 간 이벤트 기반 통신을 위한 이벤트 정의
/// </summary>
public class PlayerEvents
{
    // 공격 관련 이벤트
    public event Action<Vector3> OnAttackRequested;
    public event Action<int> OnAttackStarted;
    public event Action OnAttackEnded;

    // 이동 관련 이벤트
    public event Action<Vector3> OnMoveRequested;
    public event Action OnMoveStopped;

    public void RequestAttack(Vector3 direction) => OnAttackRequested?.Invoke(direction);
    public void StartAttack(int comboIndex) => OnAttackStarted?.Invoke(comboIndex);
    public void EndAttack() => OnAttackEnded?.Invoke();

    public void RequestMove(Vector3 destination) => OnMoveRequested?.Invoke(destination);
    public void StopMove() => OnMoveStopped?.Invoke();
}