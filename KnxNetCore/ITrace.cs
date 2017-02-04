namespace KnxNetCore
{
    public enum TraceSeverities
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public interface ITrace
    {
        void Write(TraceSeverities traceSeverity, string message, params object[] args);
    }
}