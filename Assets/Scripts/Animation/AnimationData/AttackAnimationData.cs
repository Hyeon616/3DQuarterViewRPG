using UnityEngine;

[System.Serializable]
public class AttackAnimationData
{
    [SerializeField] private AnimationClip clip;
    [SerializeField] private float moveDistance = 0.2f;
    [SerializeField] private float moveDuration = 0.2f;

    public AnimationClip Clip => clip;
    public float Duration => clip != null ? clip.length : 0f;
    public float MoveDistance => moveDistance;
    public float MoveDuration => moveDuration;
}
