using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


public class PlayableAnimator : MonoBehaviour, IAnimatable
{
    [SerializeField] private CharacterAnimationData animationData;
    [SerializeField] private Animator animator;

    private PlayableGraph playableGraph;
    private AnimationMixerPlayable mixerPlayable;

    private Dictionary<string, AnimationClipPlayable> clipPlayables = new();
    private Dictionary<string, int> clipIndices = new();

    private string currentAnimation;
    private string targetAnimation;

    private float transitionDuration;
    private float transitionTime;

    private bool isTransitioning;

    public string CurrentAnimation => currentAnimation;
    public int AttackCount => animationData != null ? animationData.AttackCount : 0;
    public float RunThreshold => animationData != null ? animationData.RunThreshold : 0.3f;

    private void Awake()
    {
        InitializePlayableGraph();
    }


    private void InitializePlayableGraph()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null || animationData == null)
        {
            
            return;
        }

        animator.applyRootMotion = false;

        playableGraph = PlayableGraph.Create($"{gameObject.name}_AnimationGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        int validCount = 0;
        foreach (var anim in animationData.GetAllAnimations())
        {
            if (anim.Clip != null)
                validCount++;
        }

        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, validCount);

        int index = 0;
        foreach (var anim in animationData.GetAllAnimations())
        {
            if (anim.Clip == null)
                continue;

            var clipPlayable = AnimationClipPlayable.Create(playableGraph, anim.Clip);
            playableGraph.Connect(clipPlayable, 0, mixerPlayable, index);

            clipPlayables[anim.Name] = clipPlayable;
            clipIndices[anim.Name] = index;
            index++;
        }

        var output = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        output.SetSourcePlayable((mixerPlayable));

        if (clipIndices.ContainsKey(BaseAnimationData.Idle))
        {
            currentAnimation = BaseAnimationData.Idle;
            mixerPlayable.SetInputWeight(clipIndices[BaseAnimationData.Idle],1f);
        }

        playableGraph.Play();
    }

    private void Update()
    {
        if (!isTransitioning)
            return;

        transitionTime += Time.deltaTime;
        float t = Mathf.Clamp01(transitionTime / transitionDuration);

        UpdateMixerWeights(t);

        if (t >= 1f)
        {
            isTransitioning = false;
            currentAnimation = targetAnimation;
        }
    }

    private void UpdateMixerWeights(float t)
    {
        if (!clipIndices.ContainsKey(currentAnimation) || !clipIndices.ContainsKey(targetAnimation))
            return;

        int currentIndex = clipIndices[currentAnimation];
        int targetIndex = clipIndices[targetAnimation];


        for (int i = 0; i < mixerPlayable.GetInputCount(); i++)
        {
            mixerPlayable.SetInputWeight(i, 0f);
        }

        mixerPlayable.SetInputWeight(currentIndex, 1f - t);
        mixerPlayable.SetInputWeight(targetIndex, t);
    }

    public void PlayAnimation(string animationName, float? duration = null)
    {
        if (!clipIndices.ContainsKey(animationName))
            return;

        // 이미 해당 애니메이션이 재생 중이거나 전환 중이면 무시
        if (currentAnimation == animationName && !isTransitioning)
            return;

        if (targetAnimation == animationName && isTransitioning)
            return;

        var anim = animationData.GetAnimation(animationName);

        if(anim == null)
            return;

        if (anim.IsOneShot && clipPlayables.ContainsKey(animationName))
            clipPlayables[animationName].SetTime(0);

        // 블렌딩 중에 새로운 전환이 시작되면 현재 블렌딩 상태를 기준으로 시작
        if (isTransitioning)
        {
            currentAnimation = targetAnimation;
        }

        targetAnimation = animationName;
        transitionDuration = duration ?? anim.BlendDuration;
        transitionTime = 0f;
        isTransitioning = true;
    }

    public void SetAnimationSpeed(float speed)
    {
        if (playableGraph.IsValid())
        {
            mixerPlayable.SetSpeed(speed);
        }
    }

    public float GetAnimationDuration(string animationName)
    {
        var anim = animationData.GetAnimation(animationName);
        return anim?.Duration ?? 0f;
    }

    public (float distance, float duration) GetAttackMoveData(string animationName)
    {
        var anim = animationData.GetAnimation(animationName);
        if (anim == null)
            return (0f, 0f);
        return (anim.MoveDistance, anim.MoveDuration);
    }

    public SkillData GetBasicAttack(int comboIndex)
    {
        return animationData?.GetBasicAttack(comboIndex);
    }

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }
}
