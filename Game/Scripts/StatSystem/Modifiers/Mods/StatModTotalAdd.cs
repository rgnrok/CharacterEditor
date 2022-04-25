namespace StatSystem
{
    public class StatModTotalAdd : StatModifier
    {
        public override int Order
        {
            get { return 4; }
        }
        public override ModifierType Type
        {
            get { return ModifierType.TotalAdd; }
        }

        public override int ApplyModifier(int statValue, float modValue)
        {
            return (int)(modValue);
        }

        public StatModTotalAdd(float value, bool stacks) : base(value, stacks) { }

        public StatModTotalAdd(float value) : base(value) { }
    }
}