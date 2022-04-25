namespace StatSystem
{
    public class Stat
    {
        public string StatName { get; set; }

        public int StatBaseValue { get; private set; }

        public virtual int StatValue
        {
            get { return StatBaseValue; }
        }

        public Stat(): this(string.Empty, 0)
        {
        }

        public Stat(int value): this(string.Empty, value)
        {
        }

        public Stat(string name, int value)
        {
            StatName = name;
            StatBaseValue = value;
        }
    }
}