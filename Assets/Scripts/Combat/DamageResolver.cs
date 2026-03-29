/// <summary>
/// 데미지 계산 처리
/// DamageCalculator를 통해 모디파이어 체인으로 최종 데미지 계산
/// </summary>
public class DamageResolver
{
    private readonly DamageCalculator _calculator;

    public DamageResolver()
    {
        _calculator = new DamageCalculator();
        RegisterDefaultModifiers();
    }

    private void RegisterDefaultModifiers()
    {
        _calculator.RegisterModifier(new BaseStatModifier());
        _calculator.RegisterModifier(new DamageIncreaseModifier());
        _calculator.RegisterModifier(new CriticalDamageModifier());
        _calculator.RegisterModifier(new HitBonusModifier());
        _calculator.RegisterModifier(new RandomVarianceModifier());
    }

    public float CalculateDamage(DamageContext context)
    {
        return _calculator.Calculate(context);
    }
}
