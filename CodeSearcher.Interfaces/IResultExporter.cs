namespace CodeSearcher.Interfaces
{
    public interface IResultExporter
    {
        void Export(ISearchResultContainer searchResultContainer, string fileName, string searchedWord);
    } 
}