namespace EnemySystem
{
    public class EnemyAttackComponent: EntityAttackComponent
    {

        protected Enemy _enemy;

        public EnemyAttackComponent(Enemy enemy): base(enemy.GameObjectData.Entity)
        {
            _enemy = enemy;
          
        }

        public override AttackComponent GetCurrentAttackComponent()
        {
            return _meleAttackComponent;
        }
    }
}
