using CodeSearcher.Interfaces;

namespace CodeSearcher.Tests.Helper
{
    internal class TestLogger : ICodeSearcherLogger
    {
        public void Debug(string message, params object[] parameter)
        {
            System.Diagnostics.Debug.Print(message, parameter);
        }

        public void Error(string message, params object[] parameter)
        {
            System.Diagnostics.Debug.Fail(string.Format(message, parameter));
        }

        public void Info(string message, params object[] parameter)
        {
            System.Diagnostics.Debug.Print(message, parameter);
        }
    }
}
