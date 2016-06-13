using CodeSearcher.Interfaces;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.SearchResults
{
    internal class SearchResultContainer : ISearchResultContainer
    {
        public int NumberOfHits { get; private set; }
        private IList<Tuple<float, Document>> FoundDocuments { get; set; }

        public SearchResultContainer(IList<Tuple<float, Document>> listOfFindings)
        {
            NumberOfHits = listOfFindings.Count;
            FoundDocuments = listOfFindings;
        }

        public IEnumerator<ISearchResult> GetEnumerator()
        {
            foreach (var tuple in FoundDocuments)
            {
                var result = Factory.GetResult(tuple.Item1, tuple.Item2);
                yield return result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
