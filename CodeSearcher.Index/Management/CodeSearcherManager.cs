using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.BusinessLogic.Serialization;
using CodeSearcher.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeSearcher.BusinessLogic.Management
{
    internal class CodeSearcherManager : ICodeSearcherManager
    {
        private readonly ICodeSearcherLogger m_Logger;
        private IList<ICodeSearcherIndex> m_Indexes;
        private string m_MetaInformationPath;
        private const string s_LuceneFolder = FolderNames.DefaultLuceneIndexName;
        private const string s_OverviewFile = FileNames.IndexOverview;

        public CodeSearcherManager(ICodeSearcherLogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ManagementInformationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FolderNames.ManagementFolder);
            ReadMetaFilesFromDisk();
        }

        /// <inheritdoc/>
        public string ManagementInformationPath { get
            {
                return m_MetaInformationPath;
            } 
            set 
            { 
                m_MetaInformationPath = value;
                ReadMetaFilesFromDisk();
            } 
        }

        /// <inheritdoc/>
        public int CreateIndex(string sourcePath, IEnumerable<string> fileExtensionsEnumeration)
        {
            if (sourcePath is null)
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }
            if (fileExtensionsEnumeration is null)
            {
                throw new ArgumentNullException(nameof(fileExtensionsEnumeration));
            }

            var fileExtensions = fileExtensionsEnumeration.ToList();
            if (fileExtensions.Count == 0)
            {
                throw new ArgumentNullException(nameof(fileExtensionsEnumeration));
            }

            var index = BuildIndexObject(sourcePath, fileExtensions);
            if (m_Indexes.Any(i => i.Equals(index)))
            {
                throw new NotSupportedException("Index for same path and filetype already exist");
            }
            m_Indexes.Add(index);

            if(!Directory.Exists(index.IndexPath))
            {
                Directory.CreateDirectory(index.IndexPath);
            }

            var logic = Factory.Get().GetCodeSearcherLogic(
                m_Logger,
                () => index.IndexPath,
                () => index.SourcePath,
                () => index.FileExtensions);

            logic.CreateNewIndex(
                () => { m_Logger.Info($"Start creating index of folder {index.SourcePath} stored in {index.IndexPath}"); },
                (file) => { m_Logger.Info($"File processed {file}"); },
                (count, timeSpan) => { m_Logger.Info($"Creating index finished, number of files: {count}, time needed: {timeSpan.TotalMilliseconds}ms"); });

            WriteMetaFilesToDisk();

            return index.ID;
        }


        /// <inheritdoc/>
        public void DeleteIndex(int indexId)
        {
            var index = m_Indexes.FirstOrDefault(i => i.ID == indexId);
            if (index == null)
            {
                throw new NotSupportedException("Index not exist");
            }

            m_Indexes.Remove(index);
        }

        /// <inheritdoc/>
        public IEnumerable<ICodeSearcherIndex> GetAllIndexes()
        {
            return m_Indexes;
        }

        /// <inheritdoc/>
        public ICodeSearcherIndex GetIndexById(int indexId)
        {
            var index = m_Indexes.FirstOrDefault(i => i.ID == indexId);
            return index;
        }

        /// <inheritdoc/>
        public IEnumerable<IDetailedSearchResult> SearchInIndex(int indexId, string searchWord)
        {
            var index = GetIndexById(indexId);

            var exporter = Factory.GetVirtualResultExporter();

            Factory.Get().GetCodeSearcherLogic(
                m_Logger,
                getIndexPath: () => index.IndexPath,
                getSourcePath: () => index.SourcePath,
                getFileExtension: () => index.FileExtensions
            ).SearchWithinExistingIndex(
                startCallback: () => { },
                getSearchWord: () => (searchWord, true),
                getMaximumNumberOfHits: () => 100,
                getHitsPerPage: () => -1,
                getExporter: () => (true, exporter),
                getSingleResultPrinter: () => null,
                finishedCallback: (timeSpan) => { },
                endOfSearchCallback: () => { },
                wildcardSearch: true
            );

            return exporter.DetailedResult;
        }

        private ICodeSearcherIndex BuildIndexObject(string sourcePath, IList<string> fileExtensions)
        {
            return Factory.GetCodeSearcherIndex(
                sourcePath,
                Path.Combine(sourcePath, s_LuceneFolder),
                fileExtensions);
        }

        private void WriteMetaFilesToDisk()
        {
            var path = Path.Combine(ManagementInformationPath, s_OverviewFile);
            Directory.CreateDirectory(ManagementInformationPath);
            using var stream = File.OpenWrite(path);
            using var sw = new StreamWriter(stream);
            try
            {
                m_Logger.Info($"Writing Index Overview to file: {path}");
                var json = JsonConvert.SerializeObject(m_Indexes, GetSerializationSettings());
                sw.Write(json);
                sw.Flush();
            }
            catch (Exception e)
            {
                m_Logger.Error($"Exception {e.Message} with callstack {e.StackTrace}");
                throw;
            }
        }

        private void ReadMetaFilesFromDisk()
        {
            var path = Path.Combine(ManagementInformationPath, s_OverviewFile);
            if (File.Exists(path))
            {
                try
                {
                    m_Logger.Info($"Reading Index Overview from {path}");
                    m_Indexes = JsonConvert.DeserializeObject<IList<ICodeSearcherIndex>>(File.ReadAllText(path), GetSerializationSettings());
                }
                catch (Exception e)
                {
                    m_Logger.Error($"Exception {e.Message} with callstack {e.StackTrace}");
                    throw;
                }
            }
            else
            {
                m_Logger.Info("No Index-Overview available, use empty list");
                m_Indexes = new List<ICodeSearcherIndex>();
            }
        }

        private JsonSerializerSettings GetSerializationSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CodeSearcherIndexConverter());
            return settings;
        }
    }
}
