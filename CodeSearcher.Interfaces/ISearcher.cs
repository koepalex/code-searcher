using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.Interfaces
{
    public interface ISearcher : IDisposable
    {
        void SearchFileContent(String pattern, int maximumNumberOfHits, Action<ISearchResultContainer> action);
    }
}
