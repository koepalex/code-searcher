namespace CodeSearcher.Interfaces
{
    public interface ICodeSearcherLogger
    {
        void Debug(string message, params object[] parameter);
        void Error(string message, params object[] parameter);
        void Info(string message, params object[] parameter);
    }
}
