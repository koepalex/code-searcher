using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.Interfaces
{
    public interface IIndexer : IDisposable
    {
        Task CreateIndex();
        event IndexerProcessFileDelegate IndexerProcessFile; 
    }
}
