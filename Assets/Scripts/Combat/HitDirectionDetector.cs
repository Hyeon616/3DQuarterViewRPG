using UnityEngine;

public class HitDirectionDetector
{
    private const float DefaultAngle = 45f;

    public HitDirection Detect(Vector3 attackerPosition, Transform target)
    {
        float headAngle = DefaultAngle;
        float backAngle = DefaultAngle;

        var indicator = target.GetComponentInChildren<HitZoneIndicator>();
        if (indicator != null)
        {
            headAngle = indicator.HeadAngle;
            backAngle = indicator.BackAngle;
        }

        return Detect(attackerPosition, target, headAngle, backAngle);
    }

    public HitDirection Detect(Transform attacker, Transform target)
    {
        return Detect(attacker.position, target);
    }

    private HitDirection Detect(Vector3 attackerPosition, Transform target, float headAngle, float backAngle)
    {
        Vector3 toAttacker = attackerPosition - target.position;
        toAttacker.y = 0f;
        toAttacker.Normalize();

        Vector3 targetForward = target.forward;
        targetForward.y = 0f;
        targetForward.Normalize();

        float angle = Vector3.Angle(targetForward, toAttacker);

        if (angle <= headAngle)
            return HitDirection.Head;

        if (angle >= 180f - backAngle)
            return HitDirection.Back;

        return HitDirection.Normal;
    }
}