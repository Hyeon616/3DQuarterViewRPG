public interface IAnimatable
{
    void PlayAnimation(string animationName, float? transitionDuration = null);
    void SetAnimationSpeed(float speed);
    string CurrentAnimation { get; }
    int AttackCount { get; }
    float RunThreshold { get; }
    float GetAnimationDuration(string animationName);
    (float distance, float duration) GetAttackMoveData(string animationName);
    SkillData GetBasicAttack(int comboIndex);
}
