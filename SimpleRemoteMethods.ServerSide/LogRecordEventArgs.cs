using System;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Information about log item
    /// </summary>
    public class LogRecordEventArgs: EventArgs
    {
        public LogRecordEventArgs(LogType type, Exception exception)
        {
            Type = type;
            Exception = exception;
        }

        public LogRecordEventArgs(LogType type, string message)
        {
            Type = type;
            Message = message;
        }

        /// <summary>
        /// Log info type
        /// </summary>
        public LogType Type { get; }

        /// <summary>
        /// Exception was thrown
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Custom message
        /// </summary>
        public string Message{ get; }
    }
}
