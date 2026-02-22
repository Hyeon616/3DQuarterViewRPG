using UnityEngine;

[CreateAssetMenu(fileName = "SkillAnimationData", menuName = "Character/Skill Animation Data")]
public class SkillAnimationData : ScriptableObject
{
    [Header("스킬 이름")] 
    [SerializeField] private string skillName;
    
    [Header("애니메이션")] 
    [SerializeField] private AnimationClip clip;
    [SerializeField] private bool isOneShot = true;
    [SerializeField] private float blendDuration = 0.1f;

    public string SkillName => skillName;

    public CharacterAnimation GetAnimation()
    {
        return new CharacterAnimation(skillName, clip, isOneShot, blendDuration);
    }
}
