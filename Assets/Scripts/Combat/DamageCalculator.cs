using System.Collections.Generic;

public class DamageCalculator
{
    private readonly List<IDamageModifier> _modifiers = new List<IDamageModifier>();
    private bool _sorted = false;

    public void RegisterModifier(IDamageModifier modifier)
    {
        _modifiers.Add(modifier);
        _sorted = false;
    }

    public void UnregisterModifier(IDamageModifier modifier)
    {
        _modifiers.Remove(modifier);
    }

    public void ClearModifiers()
    {
        _modifiers.Clear();
    }

    public float Calculate(DamageContext context)
    {
        EnsureSorted();

        float damage = context.BaseDamage;

        foreach (var modifier in _modifiers)
        {
            if (modifier.IsApplicable(context))
            {
                damage = modifier.ModifyDamage(damage, context);
            }
        }

        return damage;
    }

    private void EnsureSorted()
    {
        if (_sorted) return;

        _modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _sorted = true;
    }
}
