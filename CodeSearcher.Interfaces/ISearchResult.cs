using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.Interfaces
{
    public interface ISearchResult
    {
        String FileName { get; }
        float SearchScore { get; }
    }
}
