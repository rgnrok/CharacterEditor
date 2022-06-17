using System;

public class RangeAttackComponent : AttackComponent
{
    protected override float AttackDistance { get { return 10f; }}

    public override void Attack(IAttacked entity, Action completeHandler)
    {
        throw new System.NotImplementedException();
    }
}
