using System;

namespace StatSystem
{
    public class Vital : Attribute, IStatCurrentValueChange
    {
        public event Action OnCurrentValueChange;

        private int _statCurrentValue;
        public int StatCurrentValue
        {
            get
            {
                if (_statCurrentValue > StatValue)
                {
                    _statCurrentValue = StatValue;
                }
                else if (_statCurrentValue < 0)
                {
                    _statCurrentValue = 0;
                }
                return _statCurrentValue;
            }
            set
            {
                if (_statCurrentValue != value)
                {
                    _statCurrentValue = value;
                    TriggerCurrentValueChange();
                }
            }
        }

        public Vital(int val): base(val)
        {
            _statCurrentValue = 0;
        }

        public void SetValueCurrentToMax()
        {
            StatCurrentValue = StatValue;
        }

        private void TriggerCurrentValueChange()
        {
            OnCurrentValueChange?.Invoke();
        }
    }
}