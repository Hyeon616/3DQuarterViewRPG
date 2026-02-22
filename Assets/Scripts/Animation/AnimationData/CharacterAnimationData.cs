using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAnimationData", menuName = "Character/Character Animation Data")]
public class CharacterAnimationData : ScriptableObject
{
    [Header("공통 애니메이션")]
    [SerializeField] private BaseAnimationData baseAnimations;

    [Header("스킬 애니메이션")]
    [SerializeField] private List<SkillAnimationData> skills;

    private List<CharacterAnimation> cachedAnimations;
    private Dictionary<string, CharacterAnimation> animationLookup;

    public int AttackCount => baseAnimations != null ? baseAnimations.AttackCount : 0;
    public float RunThreshold => baseAnimations != null ? baseAnimations.RunThreshold : 0.3f;

    private void OnEnable()
    {
        BuildCache();
    }

    private void OnValidate()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        cachedAnimations = new List<CharacterAnimation>();
        animationLookup = new Dictionary<string, CharacterAnimation>();

        if (baseAnimations != null)
            cachedAnimations.AddRange(baseAnimations.GetAnimations());

        if (skills != null)
        {
            foreach (var skill in skills)
            {
                if (skill != null)
                    cachedAnimations.Add(skill.GetAnimation());
            }
        }

        foreach (var anim in cachedAnimations)
        {
            if (anim != null && !string.IsNullOrEmpty(anim.Name))
                animationLookup[anim.Name] = anim;
        }
    }

    public IEnumerable<CharacterAnimation> GetAllAnimations()
    {
        if (cachedAnimations == null)
            BuildCache();

        return cachedAnimations;
    }

    public CharacterAnimation GetAnimation(string name)
    {
        if (animationLookup == null)
            BuildCache();

        return animationLookup.TryGetValue(name, out var anim) ? anim : null;
    }
}
