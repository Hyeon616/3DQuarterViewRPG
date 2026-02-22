using UnityEngine;

[System.Serializable]
public class CharacterAnimation
{
    [SerializeField] private string name;
    [SerializeField] private AnimationClip clip;
    [SerializeField] private bool isOneShot;
    [SerializeField] private float blendDuration = 0.2f;
    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;

    public string Name => name;
    public AnimationClip Clip => clip;
    public bool IsOneShot => isOneShot;
    public float BlendDuration => blendDuration;
    public float Duration => clip != null ? clip.length : 0f;
    public float MoveDistance => moveDistance;
    public float MoveDuration => moveDuration;

    public CharacterAnimation(string name, AnimationClip clip, bool isOneShot, float blendDuration)
    {
        this.name = name;
        this.clip = clip;
        this.isOneShot = isOneShot;
        this.blendDuration = blendDuration;
        this.moveDistance = 0f;
        this.moveDuration = 0f;
    }

    public CharacterAnimation(string name, AnimationClip clip, bool isOneShot, float blendDuration, float moveDistance, float moveDuration)
    {
        this.name = name;
        this.clip = clip;
        this.isOneShot = isOneShot;
        this.blendDuration = blendDuration;
        this.moveDistance = moveDistance;
        this.moveDuration = moveDuration;
    }
}
