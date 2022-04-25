namespace StatSystem
{
    public interface IStatLinkable
    {
        int StatLinkerValue { get; }

        void AddLinker(StatLinker linker);
        void ClearLinkers();
        void UpdateLinkers();
    }
}