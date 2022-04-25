namespace StatSystem
{
    public class StatModTotalPercent : StatModifier
    {
        public override int Order
        {
            get { return 3; }
        }
        public override ModifierType Type
        {
            get { return ModifierType.TotalPercent; }
        }

        public override int ApplyModifier(int statValue, float modValue)
        {
            return (int)(statValue * modValue);
        }

        public StatModTotalPercent(float value, bool stacks) : base(value, stacks) { }

        public StatModTotalPercent(float value) : base(value) { }
    }
}