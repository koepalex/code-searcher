using System;
using System.Threading.Tasks;

namespace CodeSearcher.Interfaces
{
	/// <summary>
	/// Interface for engine to handle the search index
	/// </summary>
    public interface IIndexer : IDisposable
    {
		/// <summary>
		/// Building the search index
		/// </summary>
		/// <returns>Task object which perform the building of the index</returns>
        Task CreateIndex();

		/// <summary>
		/// Updates the search index
		/// </summary>
		/// <returns>Task object which perform the update of the index</returns>
		Task UpdateIndex();

		/// <summary>
		/// Occurs when indexer process file.
		/// </summary>
        event IndexerProcessFileDelegate IndexerProcessFile; 
    }
}
