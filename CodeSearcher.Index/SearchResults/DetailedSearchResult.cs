using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic.SearchResults
{
    internal class DetailedSearchResult : IDetailedSearchResult
    {
        /// <inheritdoc />
        public string Filename { get; set; }
        
        /// <inheritdoc />
        public IFindingInFile[] Findings { get; set; }
    }
}
