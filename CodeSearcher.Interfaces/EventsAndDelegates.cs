using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.Interfaces
{
    public class IndexerProcessFileEventArgs : EventArgs
    {
        public String FileName { get; set; }
    }


    public delegate void IndexerProcessFileDelegate (object sender, IndexerProcessFileEventArgs args);
}
