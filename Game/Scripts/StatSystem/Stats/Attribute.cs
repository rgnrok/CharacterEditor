using System.Collections.Generic;

namespace StatSystem
{
    public class Attribute : StatModifiable, IStatScalable, IStatLinkable
    {
        private List<StatLinker> _statLinkers;

        public int StatLevelValue { get; private set; }

        public int StatLinkerValue { get; private set; }

        public override int StatValue
        {
            get { return base.StatValue + StatLevelValue + StatLinkerValue; }
        }

        public virtual void ScaleStat(int level)
        {
            StatLevelValue = level;
            TriggerValueChange();
        }

        public void AddLinker(StatLinker linker)
        {
            _statLinkers.Add(linker);
            linker.OnValueChange += OnLinkerValueChange;
        }

        public void RemoveLinker(StatLinker linker)
        {
            _statLinkers.Remove(linker);
            linker.OnValueChange -= OnLinkerValueChange;
        }


        public void ClearLinkers()
        {
            foreach (var linker in _statLinkers)
            {
                linker.OnValueChange -= OnLinkerValueChange;
            }
            _statLinkers.Clear();
        }

        public void UpdateLinkers()
        {
            StatLinkerValue = 0;
            foreach (StatLinker link in _statLinkers)
            {
                StatLinkerValue += link.Value;
            }

            TriggerValueChange();
        }

        public Attribute(int value): base(value)
        {
            _statLinkers = new List<StatLinker>();
        }

        private void OnLinkerValueChange()
        {
            UpdateLinkers();
        }
    }
}