using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Combat/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string skillName;

    [Header("애니메이션")]
    [SerializeField] private AnimationClip clip;
    [SerializeField] private float blendDuration = 0.1f;

    [Header("전투")]
    [SerializeField] private AttackType attackType;
    [SerializeField] private float baseDamage;

    [Header("이펙트")]
    [SerializeField] private GameObject effectPrefab;

    [Header("자원")]
    [SerializeField] private float resourceCost;
    [SerializeField] private float identityCharge;

    public string SkillName => skillName;
    public AnimationClip Clip => clip;
    public float BlendDuration => blendDuration;
    public AttackType AttackType => attackType;
    public float BaseDamage => baseDamage;
    public GameObject EffectPrefab => effectPrefab;
    public float ResourceCost => resourceCost;
    public float IdentityCharge => identityCharge;

    public CharacterAnimation ToCharacterAnimation()
    {
        return new CharacterAnimation(skillName, clip, true, blendDuration);
    }
}