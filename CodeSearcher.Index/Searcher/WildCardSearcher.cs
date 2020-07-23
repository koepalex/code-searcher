namespace CodeSearcher.BusinessLogic.Searcher
{
    using CodeSearcher.Interfaces;
    using Lucene.Net.Documents;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    using System;
    using System.Collections.Generic;

    internal class WildcardSearcher : ISearcher
    {
        private Directory m_IndexDirectory;
        private IndexSearcher m_Searcher;

        public WildcardSearcher(String idxPath)
        {
            if (String.IsNullOrWhiteSpace(idxPath)) throw new ArgumentNullException("idxPath");
            m_IndexDirectory = FSDirectory.Open(idxPath);
            m_Searcher = new IndexSearcher(m_IndexDirectory);
        }

        public void SearchFileContent(String pattern, int maxNumberOfHits, Action<ISearchResultContainer> action)
        {
            if (String.IsNullOrWhiteSpace(pattern)) throw new ArgumentNullException("pattern");
            if (action == null) throw new ArgumentNullException("action");
            if (maxNumberOfHits < 1) throw new ArgumentException("maxNumberOfHits need to be bigger or equal to 1");

            var query = Factory.GetLuceneWildcardSearchQuery("content", pattern.ToLowerInvariant());

            var topDocs = m_Searcher.Search(query, maxNumberOfHits);

            var listOfFindings = ExtractDataOfTopDocs(topDocs);

            var resultContainer = Factory.GetResultContainer(listOfFindings);

            action(resultContainer);
        }

        private IList<Tuple<float, Document>> ExtractDataOfTopDocs(TopDocs topDocs)
        {
            var listOfFindings = new List<Tuple<float, Document>>(topDocs.ScoreDocs.Length);

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var document = m_Searcher.Doc(scoreDoc.Doc);

                var finding = Tuple.Create<float, Document>(
                    scoreDoc.Score,
                    document);

                listOfFindings.Add(finding);
            }

            return listOfFindings;
        }

        public void Dispose()
        {
            m_IndexDirectory.Dispose();
            m_Searcher.Dispose();
        }
    }
}
