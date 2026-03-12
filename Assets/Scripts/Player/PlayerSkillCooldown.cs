using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerSkillCooldown : NetworkBehaviour
{
    private PlayerController _player;
    private Dictionary<string, float> _cooldownEndTimes = new Dictionary<string, float>();

    public event Action<string, float> OnCooldownStarted;
    public event Action<string> OnCooldownEnded;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    [Server]
    public bool IsOnCooldown(SkillData skill)
    {
        if (skill == null || skill.Cooldown <= 0f) return false;

        if (_cooldownEndTimes.TryGetValue(skill.SkillName, out float endTime))
        {
            return Time.time < endTime;
        }
        return false;
    }

    [Server]
    public float GetRemainingCooldown(SkillData skill)
    {
        if (skill == null) return 0f;

        if (_cooldownEndTimes.TryGetValue(skill.SkillName, out float endTime))
        {
            return Mathf.Max(0f, endTime - Time.time);
        }
        return 0f;
    }

    [Server]
    public void StartCooldown(SkillData skill)
    {
        if (skill == null || skill.Cooldown <= 0f) return;

        // 쿨타임 감소 적용 (CooldownReduction 0.1 = 10% 감소)
        float cooldownReduction = _player.PlayerStat?.CooldownReduction ?? 0f;
        float reducedCooldown = skill.Cooldown * (1f - cooldownReduction);
        reducedCooldown = Mathf.Max(0.1f, reducedCooldown); // 최소 0.1초

        _cooldownEndTimes[skill.SkillName] = Time.time + reducedCooldown;

        RpcCooldownStarted(skill.SkillName, reducedCooldown);
    }

    [ClientRpc]
    private void RpcCooldownStarted(string skillName, float duration)
    {
        OnCooldownStarted?.Invoke(skillName, duration);
    }

    [Server]
    public bool TryUseSkill(SkillData skill)
    {
        if (IsOnCooldown(skill)) return false;

        StartCooldown(skill);
        return true;
    }

    private void Update()
    {
        if (!isServer) return;

        // 쿨다운 종료 체크
        var expiredSkills = new List<string>();
        foreach (var kvp in _cooldownEndTimes)
        {
            if (Time.time >= kvp.Value)
            {
                expiredSkills.Add(kvp.Key);
            }
        }

        foreach (var skillName in expiredSkills)
        {
            _cooldownEndTimes.Remove(skillName);
            RpcCooldownEnded(skillName);
        }
    }

    [ClientRpc]
    private void RpcCooldownEnded(string skillName)
    {
        OnCooldownEnded?.Invoke(skillName);
    }
}
