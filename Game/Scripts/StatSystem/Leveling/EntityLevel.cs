using System;

namespace StatSystem
{
    public abstract class EntityLevel
    {
        private int _level = 1;
        private int _levelMin = 1;
        private int _levelMax = 20;

        private int _expCurrent = 0;
        private int _expRequired = 0;

        public event Action<int> OnExpGain;

        public event Action<int, int> OnLevelChange;
        public event Action<int, int> OnLevelUp;

        public abstract int GetExpRequiredForLevel(int level);

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public int LevelMax
        {
            get { return _levelMax; }
            private set { _levelMax = value; }
        }

        public int LevelMin
        {
            get { return _levelMin; }
            private set { _levelMin = value; }
        }

        public int ExpRequired
        {
            get { return _expRequired; }
            private set { _expRequired = value; }
        }

        public int ExpCurrent
        {
            get { return _expCurrent; }
            private set { _expCurrent = value; }
        }


        public void ModifyExp(int amount)
        {
            ExpCurrent += amount;

            if (OnExpGain != null)
            {
                OnExpGain(amount);
            }

            CheckCurrentExp();
        }

        protected void CheckCurrentExp()
        {
            int oldLevel = Level;

            while (true)
            {
                if (ExpCurrent > ExpRequired)
                {
                    ExpCurrent -= ExpRequired;
                    IncreaseCurrentLevel();
                }
                else
                {
                    break;
                }
            }
        }

        protected void IncreaseCurrentLevel()
        {
            int oldLevel = Level++;

            if (Level > LevelMax)
            {
                Level = LevelMax;
                ExpCurrent = GetExpRequiredForLevel(Level);
            }

            ExpRequired = GetExpRequiredForLevel(Level);
            if (OnLevelUp != null)
            {
                OnLevelUp(Level, oldLevel);
            }

            if (oldLevel != Level && OnLevelChange != null)
            {
                OnLevelChange(Level, oldLevel);
            }
        }

        public void SetLevel(int targetLevel)
        {
            SetLevel(targetLevel, true);
        }

        public void SetLevel(int targetLevel, bool clearExp)
        {
            int oldLevel = Level;

            Level = targetLevel;
            ExpRequired = GetExpRequiredForLevel(Level);

            if (clearExp)
            {
                ExpCurrent = 0;
            }
            else
            {
                CheckCurrentExp();
            }

        }
    }
}
