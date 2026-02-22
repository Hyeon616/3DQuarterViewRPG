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

    [Header("전투")]
    [SerializeField] private AttackAnimationData[] attacks;
    
    [SerializeField] private AnimationClip hit;
    [SerializeField] private AnimationClip die;

    public int AttackCount => attacks != null ? attacks.Length : 0;

    public static string GetAttackName(int index) => $"{Attack}{index + 1}";

    public AttackAnimationData GetAttackData(int index)
    {
        if (attacks == null || index < 0 || index >= attacks.Length)
            return null;
        return attacks[index];
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
        if (attacks != null)
        {
            for (int i = 0; i < attacks.Length; i++)
            {
                if (attacks[i]?.Clip != null)
                {
                    animations.Add(new CharacterAnimation(
                        GetAttackName(i),
                        attacks[i].Clip,
                        true,
                        0.1f,
                        attacks[i].MoveDistance,
                        attacks[i].MoveDuration
                    ));
                }
            }
        }

        return animations;
    }
}
