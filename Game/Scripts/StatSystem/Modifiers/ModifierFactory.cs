namespace StatSystem
{
    public static class ModifierFactory
    {
        public static StatModifier Create(ModifierType type, float value)
        {
            switch (type)
            {
                case ModifierType.BaseAdd:
                    return new StatModBaseAdd(value);
                case ModifierType.BasePercent:
                    return new StatModBasePercent(value);
                case ModifierType.TotalAdd:
                    return new StatModTotalAdd(value);
                case ModifierType.TotalPercent:
                    return new StatModTotalPercent(value);
                default:
                    return null;
            }
        }
    }
}