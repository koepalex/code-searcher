namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Enable result visualization
    /// </summary>
    public interface ISingleResultPrinter
    {
        /// <summary>
        /// Should visualize the content of file as well as find and highlight all occurances 
        /// of the word to lookup
        /// </summary>
        /// <param name="fileName">Fullpath of the file, that contain the word to look up</param>
        /// <param name="searchedWord">Word to look up</param>
        void Print(string fileName, string searchedWord);
        
        /// <summary>
        /// Number of occurences of word to look up, showed (at once) 
        /// </summary>
        /// <remarks>
        /// -1 mean show all
        /// </remarks>
        int NumbersToShow { get; set; }
    }
}
