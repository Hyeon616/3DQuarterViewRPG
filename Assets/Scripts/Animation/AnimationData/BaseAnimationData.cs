using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "BaseAnimationData", menuName = "Character/Base Animation Data")]
public class BaseAnimationData : ScriptableObject
{
    public const string Idle   = "Idle";
    public const string Walk   = "Walk";
    public const string Run    = "Run";
    public const string Attack = "Attack";
    public const string Hit    = "Hit";
    public const string Die    = "Die";

    [Header("이동")] 
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip walk;
    [SerializeField] private AnimationClip run;
    
    [Header("전투")]
    [SerializeField] private AnimationClip attack;
    [SerializeField] private AnimationClip hit;
    [SerializeField] private AnimationClip die;

    public IEnumerable<CharacterAnimation> GetAnimations()
    {
        return new List<CharacterAnimation>
        {
            new CharacterAnimation(Idle, idle, false, 0.2f),
            new CharacterAnimation(Walk, walk, false, 0.2f),
            new CharacterAnimation(Run, run, false, 0.2f),
            new CharacterAnimation(Attack, attack, true, 0.1f),
            new CharacterAnimation(Hit, hit, true, 0.05f),
            new CharacterAnimation(Die, die, true, 0.2f),
        };
        
    }

}
