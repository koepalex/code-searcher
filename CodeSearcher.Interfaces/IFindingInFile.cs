namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Describe a finding in existing file
    /// </summary>
    public interface IFindingInFile
    {
        /// <summary>
        /// number of line with finding
        /// </summary>
        int LineNumber { get; set; }
        
        /// <summary>
        /// start position of finding
        /// </summary>
        int Position { get; set; }
        
        /// <summary>
        /// length of finding
        /// </summary>
        int Length { get; set; }
    }
}
