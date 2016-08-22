using CodeSearcher.BusinessLogic.Io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.InternalInterfaces
{
    internal interface IFileReader
    {
        Task ReadFilesAsync(String srcPath, Action<IList<FileStructure>> action);
		Task ReadFilesAsync(IEnumerable<string> files, Action<IList<FileStructure>> action);
    }
}
