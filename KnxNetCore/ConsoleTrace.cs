using System;

namespace KnxNetCore
{
    public class ConsoleTrace : ITrace
    {
        public void Write(TraceSeverities logSeverity, string message, params object[] args)
        {
            if (logSeverity == TraceSeverities.Debug)
            {
                return;
            }
            Console.WriteLine(logSeverity + ": " + string.Format(message, args));
        }
    }
}
