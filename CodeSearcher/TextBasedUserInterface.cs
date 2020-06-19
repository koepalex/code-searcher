using System;

namespace CodeSearcher
{
    internal class TextBasedUserInterface : ITextBasedUserInterface
    {
        private bool m_ShouldLoop;
        /// <inheritdoc />
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        /// <inheritdoc />
        public void Write(string message)
        {
            Console.Write(message);
        }

        /// <inheritdoc />
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        /// <inheritdoc />
        public bool ShouldLoop()
        {
            return m_ShouldLoop;
        }

        /// <inheritdoc />
        public void SetShouldLoop(bool value)
        {
            m_ShouldLoop = value;
        }
    }
}
