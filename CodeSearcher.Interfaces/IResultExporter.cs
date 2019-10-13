using System.IO;

namespace CodeSearcher.Interfaces
{
    public interface IResultExporter
    {
        void Export(ISearchResultContainer searchResultContainer, string searchedWord, StreamWriter exportWriter);
    } 
}