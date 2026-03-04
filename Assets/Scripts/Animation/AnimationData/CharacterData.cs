using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Character/Character Data")]
public class CharacterData : ScriptableObject
{
    #region Layer Masks

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask hitLayerMask;

    public LayerMask GroundLayerMask => groundLayerMask;
    public LayerMask HitLayerMask => hitLayerMask;

    #endregion

    #region Animation

    [Header("Animation")]
    [SerializeField] private BaseAnimationData baseAnimations;

    [Header("Skills")]
    [SerializeField] private List<SkillData> skills;

    private List<CharacterAnimation> _cachedAnimations;
    private Dictionary<string, CharacterAnimation> _animationLookup;

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
        _cachedAnimations = new List<CharacterAnimation>();
        _animationLookup = new Dictionary<string, CharacterAnimation>();

        if (skills != null)
        {
            foreach (var skill in skills)
            {
                if (skill != null)
                    _cachedAnimations.Add(skill.ToCharacterAnimation());
            }
        }

        if (baseAnimations != null)
            _cachedAnimations.AddRange(baseAnimations.GetAnimations());

        foreach (var anim in _cachedAnimations)
        {
            if (anim != null && !string.IsNullOrEmpty(anim.Name))
                _animationLookup[anim.Name] = anim;
        }
    }

    public IEnumerable<CharacterAnimation> GetAllAnimations()
    {
        if (_cachedAnimations == null)
            BuildCache();

        return _cachedAnimations;
    }

    public CharacterAnimation GetAnimation(string animName)
    {
        if (_animationLookup == null)
            BuildCache();

        return _animationLookup.TryGetValue(animName, out var anim) ? anim : null;
    }

    public SkillData GetBasicAttack(int index)
    {
        return baseAnimations?.GetBasicAttack(index);
    }

    #endregion
}
