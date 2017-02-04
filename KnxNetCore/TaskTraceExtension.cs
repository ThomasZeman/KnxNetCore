using System.Threading.Tasks;

namespace KnxNetCore
{
    public static class TaskTraceExtension
    {
        public static void TraceOnException(this Task task, ITrace trace)
        {
            task.ContinueWith(_ => trace.Write(TraceSeverities.Error, _.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}