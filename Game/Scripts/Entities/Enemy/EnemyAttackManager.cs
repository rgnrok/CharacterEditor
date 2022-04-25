namespace EnemySystem
{
    public class EnemyAttackManager: EntityAttackManager
    {

        protected Enemy _enemy;

        public EnemyAttackManager(Enemy enemy): base(enemy.GameObjectData.Entity)
        {
            _enemy = enemy;
          
        }

        protected override AttackComponent GetCurrentAttackComponent()
        {
            return _meleAttackComponent;
        }
    }
}
