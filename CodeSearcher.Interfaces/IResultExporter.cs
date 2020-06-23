using System;

namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Interface to write findings to file
    /// </summary>
    public interface IResultExporter : IDisposable
    {
        /// <summary>
        /// Dump the results (findings) into a file with a specific format
        /// </summary>
        /// <param name="searchResultContainer">Enumeration of all finding of word to look for within index</param>
        /// <param name="searchedWord">Word to look for</param>
        void Export(ISearchResultContainer searchResultContainer, string searchedWord);
    } 
}