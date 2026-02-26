using UnityEngine;

public enum IdentityChargeType
{
    None,       // 아이덴티티 없음
    OnHit,      // 스킬 적중 시 충전
    OverTime    // 시간에 따라 충전
}

[System.Serializable]
public class IdentityData
{
    [SerializeField] private string identityName;
    [SerializeField] private float maxValue;
    [SerializeField] private IdentityChargeType chargeType;
    [SerializeField] private float chargePerSecond; // OverTime일 때 사용

    public string IdentityName => identityName;
    public float MaxValue => maxValue;
    public IdentityChargeType ChargeType => chargeType;
    public float ChargePerSecond => chargePerSecond;
    public bool HasIdentity => chargeType != IdentityChargeType.None;
}