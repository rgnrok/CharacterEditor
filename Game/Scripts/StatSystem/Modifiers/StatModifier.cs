using System;

namespace StatSystem
{
    public abstract class StatModifier
    {
        private float _value = 0f;
        private bool _stacks = true;

        public event Action OnValueChange;

        public abstract int Order { get; }
        public abstract ModifierType Type { get; }
        public abstract int ApplyModifier(int statValue, float modValue);

        public float Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    if (OnValueChange != null)
                    {
                        OnValueChange();
                    }
                }
            }
        }

        public bool Stacks
        {
            get { return _stacks; }
            set { _stacks = value; }
        }

        public StatModifier(float value) : this(value, true) { }

        public StatModifier(float value, bool stacks)
        {
            Value = value;
            Stacks = stacks;
        }
    }
}