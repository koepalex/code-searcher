namespace CodeSearcher.Interfaces
{
    public interface ISingleResultPrinter
    {
        void Print(string fileName, string searchedWord);
        int NumbersToShow { get; set; }
    }
}
