using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic.SearchResults
{
    internal class FindingInFile : IFindingInFile
    {
        /// <inheritdoc />
        public int LineNumber { get; set; }
        
        /// <inheritdoc />
        public int Position { get; set; }
        
        /// <inheritdoc />
        public int Length { get; set; }
    }
}
