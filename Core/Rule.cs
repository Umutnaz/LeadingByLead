namespace Core;

public class Rule
{
    public StatGroup TargetGroup { get; set; }

    public string ConditionStat { get; set; } = "";

    public RuleOperator Operator { get; set; }

    public int CompareValue { get; set; }

    public string EffectStat { get; set; } = "";

    public int ChangeValue { get; set; }
}

public enum StatGroup
{
    CurrentStats,
    Traits
}

public enum RuleOperator
{
    LessThan,
    GreaterThan,
    Equal
}