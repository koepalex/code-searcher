using System;
using System.Collections.Generic;

namespace CodeSearcher.Interfaces
{
	public interface IChangeSet
	{
		IEnumerable<string> ModifiedFiles { get; }
	}
}

