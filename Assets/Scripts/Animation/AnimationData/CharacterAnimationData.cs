using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterAnimationData", menuName = "Character/Character Animation Data")]

public class CharacterAnimationData : ScriptableObject
{
    [Header("공통 애니메이션")]
    [SerializeField] private BaseAnimationData baseAnimations;

    [Header("스킬 애니메이션")]
    [SerializeField] private List<SkillAnimationData> skills;

    public int AttackCount => baseAnimations != null ? baseAnimations.AttackCount : 0;

    public IEnumerable<CharacterAnimation> GetAllAnimations()
    {
        var result = new List<CharacterAnimation>();
        
        if(baseAnimations != null)
            result.AddRange(baseAnimations.GetAnimations());

        foreach (var skill in skills)
        {
            if (skill != null)
                result.Add(skill.GetAnimation());
        }
        
        return result;
    }

    public CharacterAnimation GetAnimation(string name)
    {
        foreach (var anim in GetAllAnimations())
        {
            if (anim.Name == name)
                return anim;
            
        }

        return null;
    }
    
}
