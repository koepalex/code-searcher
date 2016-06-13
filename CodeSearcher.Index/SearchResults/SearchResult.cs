using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.SearchResults
{
    internal class SearchResult : ISearchResult
    {
        private float m_SearchScore;
        private Document m_Document;

        public SearchResult(float searchScore, Document document)
        {
            m_SearchScore = searchScore;
            m_Document = document;
        }

        public string FileName
        {
            get 
            {
                return m_Document.Get(Names.FileNameFieldName);
            }
        }

        public float SearchScore
        {
            get
            {
                return m_SearchScore;
            }
        }
    }
}
