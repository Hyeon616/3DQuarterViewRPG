/// <summary>
/// 공격 상태를 제공하는 인터페이스
/// </summary>
public interface IAttackState
{
    bool IsAttacking { get; }
    int CurrentComboIndex { get; }
}