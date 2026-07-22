namespace Controls
{
    [System.Flags]
    public enum ReactiveInputPhase
    {
        Started = 1 << 0,
        Performed = 1 << 1,
        Canceled = 1 << 2,
        All = Started | Performed | Canceled
    }
}