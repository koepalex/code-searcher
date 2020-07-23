namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Describe details of search result
    /// </summary>
    public interface IDetailedSearchResult
    {
        /// <summary>
        /// Fullpath of file with findings
        /// </summary>
        string Filename { get; set; }
        
        /// <summary>
        /// All findingd within single file
        /// </summary>
        IFindingInFile[] Findings { get; set; }
    }
}
