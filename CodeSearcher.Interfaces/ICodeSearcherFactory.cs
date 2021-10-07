﻿using CodeSearcher.BusinessLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Interface to create requried CodeSearcher objects
    /// </summary>
    public interface ICodeSearcherFactory
    {
        /// <summary>
        /// Return instance of management interface, <see cref="ICodeSearcherManager"/>
        /// </summary>
        /// <param name="logger">Instance of logger <see cref="ICodeSearcherLogger"/></param>
        /// <returns>Instance of ICodeSearcherManager</returns>
        ICodeSearcherManager GetCodeSearcherManager(ICodeSearcherLogger logger);

        /// <summary>
        /// Return instance of CodeSearcher businesslogc; <see cref="ICodeSearcherLogic"/>
        /// </summary>
        /// <param name="logger">Instance of logger <see cref="ICodeSearcherLogger"/></param>
        /// <param name="getIndexPath">Callback to inject the path to store lucene index files</param>
        /// <param name="getSourcePath">Callback to inject the path with files to analyze</param>
        /// <param name="getFileExtension">Callback to inject extension of files to look for</param>
        /// <returns>Instance of ICodeSearcherLogic</returns>
        ICodeSearcherLogic GetCodeSearcherLogic(
            ICodeSearcherLogger logger,
            Func<string> getIndexPath,
            Func<string> getSourcePath,
            Func<IList<string>> getFileExtension);

        /// <summary>
        /// Return instance of default exporter, <see cref="IResultExporter"/>
        /// </summary>
        /// <param name="exportWriter">Writer of writable stream to export findings into</param>
        /// <returns>Instance of IResultExporter</returns>
        /// <remarks>
        /// Don't forget to Dispose result exporter
        /// </remarks>
        IResultExporter GetDefaultResultExporter(StreamWriter exportWriter);

        /// <summary>
        /// Return instance of exporter that handle wildcard search findings, <see cref="IResultExporter"/>
        /// </summary>
        /// <param name="exportWriter">Writer of writable stream to export findings into</param>
        /// <returns>Instance of IResultExporter</returns>
        /// <remarks>
        /// Don't forget to Dispose result exporter
        /// </remarks>
        IResultExporter GetWildcardResultExporter(StreamWriter exportWriter);

        

        /// <summary>
        /// Return instance of class that can serialize / deserialize ICodeSearcherIndex to and from JSON
        /// </summary>
        /// <returns>Instance of JsonConverter</returns>
        JsonConverter GetCodeSearcherIndexJsonConverter();

        /// <summary>
        /// Return instance of class that can serialize / deserialize IDetailedSearchResult to and from JSON
        /// </summary>
        /// <returns>Instance of JsonConverter</returns>
        JsonConverter GetDetailedResultJsonConverter();

        /// <summary>
        /// Return instance of class that can serialize / deserialize IFindingInFile to and from JSON
        /// </summary>
        /// <returns>Instance of JsonConverter</returns>
        JsonConverter GetFindingsInFileJsonConverter();
    }
}
