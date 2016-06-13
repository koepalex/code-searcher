using CodeSearcher.BusinessLogic.Indexer;
using CodeSearcher.BusinessLogic.InternalInterfaces;
using CodeSearcher.BusinessLogic.Io;
using CodeSearcher.BusinessLogic.Searcher;
using CodeSearcher.BusinessLogic.SearchResults;
using CodeSearcher.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.Ninject
{
    class CodeSearcherNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISearchResultContainer>().To(typeof(SearchResultContainer));
            Bind<ISearchResult>().To(typeof(SearchResult));
            Bind<IIndexer>().To(typeof(DefaultIndexer));
            Bind<ISearcher>().To(typeof(DefaultSearcher));
            Bind<IFileReader>().To(typeof(FileReader));
        }
    }
}
