namespace StatSystem
{
    public class StatModBasePercent : StatModifier
    {
        public override int Order
        {
            get { return 1; }
        }
        public override ModifierType Type
        {
            get { return ModifierType.BasePercent; }
        }

        public override int ApplyModifier(int statValue, float modValue)
        {
            return (int)(statValue * modValue);
        }

        public StatModBasePercent(float value, bool stacks) : base(value, stacks) { }

        public StatModBasePercent(float value) : base(value) { }
    }
}