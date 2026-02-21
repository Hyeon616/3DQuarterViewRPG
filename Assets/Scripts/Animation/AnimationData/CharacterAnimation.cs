using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterAnimation
{
    [SerializeField] private string name;
    [SerializeField] private AnimationClip clip;
    [SerializeField] private bool isOneShot;
    [SerializeField] private float blendDuration = 0.2f;

    public string Name => name;
    public AnimationClip Clip => clip;
    public bool IsOneShot => isOneShot;
    public float BlendDuration => blendDuration;

    public CharacterAnimation(string name, AnimationClip clip, bool isOneShot, float blendDuration)
    {
        this.name = name;
        this.clip = clip;
        this.isOneShot = isOneShot;
        this.blendDuration = blendDuration;
    }

}
