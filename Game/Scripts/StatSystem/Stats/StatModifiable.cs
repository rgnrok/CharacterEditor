using System;
using System.Collections.Generic;

namespace StatSystem
{
    public class StatModifiable : Stat, IStatModifiable, IStatValueChange
    {
        private List<StatModifier> _statMods;
        private int _statModValue;

        public event Action OnValueChange;

        public override int StatValue
        {
            get { return base.StatValue + StatModifierValue; }
        }

        public int StatModifierValue
        {
            get { return _statModValue; }
        }

        public StatModifiable(int value): base(value)
        {
            _statModValue = 0;
            _statMods = new List<StatModifier>();
        }

        public void AddModifier(StatModifier mod)
        {
            _statMods.Add(mod);
            mod.OnValueChange += OnModValueChange;
        }

        public void RemoveModifier(StatModifier mod)
        {
            _statMods.Remove(mod);
            mod.OnValueChange -= OnModValueChange;
        }

        public void ClearModifiers()
        {
            foreach (var mod in _statMods)
            {
                mod.OnValueChange -= OnModValueChange;
            }
            _statMods.Clear();
        }

        public void UpdateModifiers()
        {
            _statModValue = 0;

            var orderGroup = SortAndGroupModifiers();
            foreach (var group in orderGroup.Values)
            {
                float sum = 0, max = 0;
                foreach (var mod in group)
                {
                    if (mod.Stacks)
                    {
                        sum += mod.Value;
                    }
                    else
                    {
                        if (mod.Value > max)
                        {
                            max = mod.Value;
                        }
                    }
                }

                _statModValue += group[0].ApplyModifier(
                    StatBaseValue + _statModValue,
                    sum > max ? sum : max
                 );
            }
            TriggerValueChange();
        }

        private Dictionary<int, List<StatModifier>> SortAndGroupModifiers()
        {
            var groups = new Dictionary<int, List<StatModifier>>();

            _statMods.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var mod in _statMods)
            {
                if (!groups.ContainsKey(mod.Order)) groups[mod.Order] = new List<StatModifier>();

                groups[mod.Order].Add(mod);
            }
            return groups;
        }

        protected void TriggerValueChange()
        {
            if (OnValueChange != null)
            {
                OnValueChange();
            }
        }

        public void OnModValueChange()
        {
            UpdateModifiers();
        }
    }
}