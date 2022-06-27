using System;

public class RangeAttackComponent : AttackComponent
{
    public override float AttackDistance { get { return 10f; }}

    public override void Attack(IAttacked entity, Action completeHandler)
    {
        throw new System.NotImplementedException();
    }
}
