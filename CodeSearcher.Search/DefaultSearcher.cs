using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.Search
{
    public class DefaultSearcher
    {
        
        public DefaultSearcher(String idxPath)
        {
            if (String.IsNullOrWhiteSpace(idxPath)) throw new ArgumentNullException("idxPath");
            m_IndexDirectory = FSDirectory.Open(idxPath);
        }
    }
}
