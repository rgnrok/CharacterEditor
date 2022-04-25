
    public class FSMTransition
    {
        public int Id { get; private set; }
        public IExitableState From { get; private set; }
        public IExitableState To { get; private set; }

        public FSMTransition(int id, IExitableState from, IExitableState to)
        {
            Id = id;
            From = from;
            To = to;
        }
    }
