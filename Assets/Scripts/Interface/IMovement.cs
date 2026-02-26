using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 이동 관련 데이터를 제공하는 인터페이스
/// </summary>
public interface IMovement
{
    NavMeshAgent Agent { get; }
    Vector3 Velocity { get; }
    void Move(Vector3 delta);
    void ResetPath();
}
