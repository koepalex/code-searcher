using System.Collections.Generic;

namespace CodeSearcher.Interfaces
{
	/// <summary>
	/// Interface which encapsulate files which has been modified since creating the index and need to be updated
	/// </summary>
	public interface IChangeSet
	{
		/// <summary>
		/// files which has been modified since creating the index
		/// </summary>
		/// <value>Enumerationof the modified files.</value>
		IEnumerable<string> ModifiedFiles { get; }
	}
}

