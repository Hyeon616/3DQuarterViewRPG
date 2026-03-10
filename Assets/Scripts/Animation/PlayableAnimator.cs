using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


public class PlayableAnimator : MonoBehaviour, IAnimatable, ICharacterData
{
    [SerializeField] private CharacterData _animationData;

    public CharacterData CharacterData => _animationData;
    [SerializeField] private Animator _animator;

    private PlayableGraph _playableGraph;
    private AnimationMixerPlayable _mixerPlayable;

    private readonly Dictionary<string, AnimationClipPlayable> _clipPlayables = new();
    private readonly Dictionary<string, int> _clipIndices = new();

    private string _currentAnimation;
    private string _targetAnimation;

    private float _transitionDuration;
    private float _transitionTime;

    private bool _isTransitioning;

    public string CurrentAnimation => _currentAnimation;
    public int AttackCount => _animationData != null ? _animationData.AttackCount : 0;
    public float RunThreshold => _animationData != null ? _animationData.RunThreshold : 0.3f;

    private void Awake()
    {
        InitializePlayableGraph();
    }


    private void InitializePlayableGraph()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_animator == null || _animationData == null)
        {
            return;
        }

        _animator.applyRootMotion = false;

        _playableGraph = PlayableGraph.Create($"{gameObject.name}_AnimationGraph");
        _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        int validCount = 0;
        foreach (var anim in _animationData.GetAllAnimations())
        {
            if (anim.Clip != null)
                validCount++;
        }

        _mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, validCount);

        int index = 0;
        foreach (var anim in _animationData.GetAllAnimations())
        {
            if (anim.Clip == null)
                continue;

            var clipPlayable = AnimationClipPlayable.Create(_playableGraph, anim.Clip);
            _playableGraph.Connect(clipPlayable, 0, _mixerPlayable, index);

            _clipPlayables[anim.Name] = clipPlayable;
            _clipIndices[anim.Name] = index;
            index++;
        }

        var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
        output.SetSourcePlayable(_mixerPlayable);

        if (_clipIndices.ContainsKey(BaseAnimationData.Idle))
        {
            _currentAnimation = BaseAnimationData.Idle;
            _mixerPlayable.SetInputWeight(_clipIndices[BaseAnimationData.Idle], 1f);
        }

        _playableGraph.Play();
    }

    private void Update()
    {
        if (!_isTransitioning)
            return;

        _transitionTime += Time.deltaTime;
        float t = Mathf.Clamp01(_transitionTime / _transitionDuration);

        UpdateMixerWeights(t);

        if (t >= 1f)
        {
            _isTransitioning = false;
            _currentAnimation = _targetAnimation;
        }
    }

    private void UpdateMixerWeights(float t)
    {
        if (!_clipIndices.ContainsKey(_currentAnimation) || !_clipIndices.ContainsKey(_targetAnimation))
            return;

        int currentIndex = _clipIndices[_currentAnimation];
        int targetIndex = _clipIndices[_targetAnimation];

        for (int i = 0; i < _mixerPlayable.GetInputCount(); i++)
        {
            _mixerPlayable.SetInputWeight(i, 0f);
        }

        _mixerPlayable.SetInputWeight(currentIndex, 1f - t);
        _mixerPlayable.SetInputWeight(targetIndex, t);
    }

    public void PlayAnimation(string animationName, float? duration = null)
    {
        if (!_clipIndices.ContainsKey(animationName))
            return;

        if (_currentAnimation == animationName && !_isTransitioning)
            return;

        if (_targetAnimation == animationName && _isTransitioning)
            return;

        var anim = _animationData.GetAnimation(animationName);

        if (anim == null)
            return;

        if (anim.IsOneShot && _clipPlayables.ContainsKey(animationName))
            _clipPlayables[animationName].SetTime(0);

        if (_isTransitioning)
        {
            _currentAnimation = _targetAnimation;
        }

        _targetAnimation = animationName;
        _transitionDuration = duration ?? anim.BlendDuration;
        _transitionTime = 0f;
        _isTransitioning = true;
    }

    public void SetAnimationSpeed(float speed)
    {
        if (_playableGraph.IsValid())
        {
            _mixerPlayable.SetSpeed(speed);
        }
    }

    public float GetAnimationDuration(string animationName)
    {
        var anim = _animationData.GetAnimation(animationName);
        return anim?.Duration ?? 0f;
    }

    public (float distance, float duration) GetAttackMoveData(string animationName)
    {
        var anim = _animationData.GetAnimation(animationName);
        if (anim == null)
            return (0f, 0f);
        return (anim.MoveDistance, anim.MoveDuration);
    }

    public SkillData GetBasicAttack(int comboIndex)
    {
        return _animationData?.GetBasicAttack(comboIndex);
    }

    private void OnDestroy()
    {
        if (_playableGraph.IsValid())
        {
            _playableGraph.Destroy();
        }
    }
}
