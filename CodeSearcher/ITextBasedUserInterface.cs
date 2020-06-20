namespace CodeSearcher
{
    /// <summary>
    /// Interface to encapsulate Text based UI interactions, to decouple code and improve testability
    /// </summary>
    internal interface ITextBasedUserInterface
    {
        /// <summary>
        /// Writes the specified string value, to the standard output stream.
        /// </summary>
        /// <param name="message">value to write </param>
        void Write(string message);
        /// <summary>
        /// Writes the specified string value, followed by the current line terminator, to
        //  the standard output stream.
        /// </summary>
        /// <param name="message">value to write </param>
        void WriteLine(string message);
        /// <summary>
        /// Reads the next line of characters from the standard input stream.
        /// </summary>
        /// <returns>
        /// The next line of characters from the input stream, or null if no 
        /// more lines are available
        /// </returns>
        string ReadLine();
        /// <summary>
        /// Clears the text buffer and corresponding console window of display information.
        /// </summary>
        void Clear();
        /// <summary>
        /// Indicates if implementation should loop until correct answer was given
        /// </summary>
        /// <returns>true if should wait otherwise false</returns>
        bool ShouldLoop();
        /// <summary>
        /// Set the value if menu should wait until user enter valid value, or stop loop
        /// </summary>
        /// <param name="value">Value that should be returned by ShouldLoop method</param>
        void SetShouldLoop(bool value);
    }
}
