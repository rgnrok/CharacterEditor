using System;

namespace StatSystem
{
    public abstract class StatLinker : IStatValueChange
    {
        private Stat _stat;
        public event Action OnValueChange;

        public StatLinker(Stat stat)
        {
            Stat = stat;

            IStatValueChange iValueChange = Stat as IStatValueChange;
            if (iValueChange != null)
            {
                iValueChange.OnValueChange += OnLinkedStatValueChange;
            }
        }

        public Stat Stat { get; private set; }

        public abstract int Value { get; }

        private void OnLinkedStatValueChange()
        {
            if (OnValueChange != null)
            {
                OnValueChange();
            }
        }
    }
}
