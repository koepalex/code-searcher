using System;
using System.IO;

namespace CodeSearcher.Interfaces
{
    public interface IResultExporter : IDisposable
    {
        void Export(ISearchResultContainer searchResultContainer, string searchedWord);
    } 
}