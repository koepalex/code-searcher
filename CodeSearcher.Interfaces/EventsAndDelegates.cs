using System;

namespace CodeSearcher.Interfaces
{
	/// <summary>
	/// Custom event argument in case the indexer has processed one file
	/// </summary>
    public class IndexerProcessFileEventArgs : EventArgs
    {
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
        public String FileName { get; set; }
    }

	/// <summary>
	/// Delegate for custom event IndexerProcessFile
	/// </summary>
    public delegate void IndexerProcessFileDelegate (object sender, IndexerProcessFileEventArgs args);
}
