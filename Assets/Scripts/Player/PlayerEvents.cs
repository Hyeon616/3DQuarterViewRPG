using System;
using UnityEngine;

/// <summary>
/// 플레이어 컴포넌트 간 이벤트 기반 통신을 위한 이벤트 정의
/// </summary>
public class PlayerEvents
{
    // 스킬 관련 이벤트
    public event Action<Vector3> OnSkillRequested;
    public event Action<SkillData> OnSkillStarted;
    public event Action OnSkillEnded;

    // 이동 관련 이벤트
    public event Action<Vector3> OnMoveRequested;
    public event Action OnMoveStopped;

    public void RequestSkill(Vector3 direction) => OnSkillRequested?.Invoke(direction);
    public void StartSkill(SkillData skill) => OnSkillStarted?.Invoke(skill);
    public void EndSkill() => OnSkillEnded?.Invoke();

    public void RequestMove(Vector3 destination) => OnMoveRequested?.Invoke(destination);
    public void StopMove() => OnMoveStopped?.Invoke();
}
