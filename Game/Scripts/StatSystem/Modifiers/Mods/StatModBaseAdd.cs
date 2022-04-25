namespace StatSystem
{
    public class StatModBaseAdd : StatModifier
    {
        public override int Order
        {
            get { return 2; }
        }

        public override ModifierType Type
        {
            get { return ModifierType.BaseAdd; }
        }

        public override int ApplyModifier(int statValue, float modValue)
        {
            return (int)modValue;
        }

        public StatModBaseAdd(float value, bool stacks) : base(value, stacks) { }

        public StatModBaseAdd(float value) : base(value) { }
    }
}