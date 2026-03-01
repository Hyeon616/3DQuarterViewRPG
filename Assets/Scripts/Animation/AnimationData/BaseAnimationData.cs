using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseAnimationData", menuName = "Character/Base Animation Data")]
public class BaseAnimationData : ScriptableObject
{
    public const string Idle = "Idle";
    public const string Walk = "Walk";
    public const string Run = "Run";
    public const string Attack = "Attack";
    public const string Hit = "Hit";
    public const string Die = "Die";

    [Header("이동")]
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip walk;
    [SerializeField] private AnimationClip run;
    [SerializeField] private float runThreshold = 0.3f;

    public float RunThreshold => runThreshold;

    [Header("전투")]
    [SerializeField] private SkillData[] basicAttacks;

    [SerializeField] private AnimationClip hit;
    [SerializeField] private AnimationClip die;

    public int AttackCount => basicAttacks != null ? basicAttacks.Length : 0;

    public static string GetAttackName(int index) => $"{Attack}{index + 1}";

    public SkillData GetBasicAttack(int index)
    {
        if (basicAttacks == null || index < 0 || index >= basicAttacks.Length)
            return null;
        return basicAttacks[index];
    }

    public IEnumerable<CharacterAnimation> GetAnimations()
    {
        var animations = new List<CharacterAnimation>
        {
            new CharacterAnimation(Idle, idle, false, 0.2f),
            new CharacterAnimation(Walk, walk, false, 0.2f),
            new CharacterAnimation(Run, run, false, 0.2f),
            new CharacterAnimation(Hit, hit, true, 0.05f),
            new CharacterAnimation(Die, die, true, 0.2f),
        };

        // 콤보 공격 애니메이션 추가
        if (basicAttacks != null)
        {
            for (int i = 0; i < basicAttacks.Length; i++)
            {
                if (basicAttacks[i] != null)
                {
                    var skillAnim = basicAttacks[i].ToCharacterAnimation();
                    // 스킬 이름 대신 Attack1, Attack2 등으로 이름 변경
                    animations.Add(new CharacterAnimation(
                        GetAttackName(i),
                        basicAttacks[i].Clip,
                        true,
                        basicAttacks[i].BlendDuration,
                        basicAttacks[i].MoveDistance,
                        basicAttacks[i].MoveDuration
                    ));
                }
            }
        }

        return animations;
    }
}
