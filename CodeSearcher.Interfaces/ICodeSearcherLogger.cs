namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Simplified Logging interface, can be used for different purposes
    /// ASP.NET Logging
    /// Console Logging
    /// ...
    /// </summary>
    public interface ICodeSearcherLogger
    {
        /// <summary>
        /// Write message into Debug Stream
        /// </summary>
        /// <param name="message">text (Format string) to write</param>
        /// <param name="parameter">Additional (Format) values</param>
        void Debug(string message, params object[] parameter);
        
        /// <summary>
        /// Write message into Error Stream
        /// </summary>
        /// <param name="message">text (Format string) to write</param>
        /// <param name="parameter">Additional (Format) values</param>
        void Error(string message, params object[] parameter);
        
        /// <summary>
        /// Write message into Standard Stream
        /// </summary>
        /// <param name="message">text (Format string) to write</param>
        /// <param name="parameter">Additional (Format) values</param>
        void Info(string message, params object[] parameter);
    }
}
