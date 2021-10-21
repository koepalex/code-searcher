using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using NLog;
namespace CodeSearcher
{
    internal class CmdLineHandler : ICmdLineHandler
    {
        [Verb("index", HelpText = "Creates an index of a given source directory")]
        private class IndexOptions
        {
            [Option('s', "sourcePath", Required = true, HelpText = "Location of files, which should be indexed")]
            public string SourcePath { get;set; }

            [Option('i', "indexPath", Required = true, HelpText = "Location where the index should be stored")]
            public string IndexPath { get; set; }

            [Option('f', "fileExtensions", Default = new [] {".cs", ".xml", ".csproj" }, Min = 1, Separator = ',', HelpText = "Extensions of files to index (default is '.cs,.xml,.csproj')")]
            public IEnumerable<string> FileExtention { get; set; } 
        }

        [Verb("search", HelpText = "Searchs for a given search word in a given index")]
        private class SearchOptions 
        {
            [Option('i', "indexPath", Required = true, HelpText = "Location where the index is stored")]
            public string IndexPath { get; set; }

            [Option('p', "hitsPerPage", Default = -1, HelpText = "Amount of findings to show at once (-1 means all)")]
            public int HitsPerPage { get; set; }
            
            [Option('n', "numberOfHits", Default = 1000, HelpText = "Amount of files with findings")]
            public int NumberOfHitsToShow { get; set; }

            [Option('s', "searchWord", Required = true, HelpText = "word to look for into index")]
            public string SearchWord { get; set; }

            [Option('e', "export", Default = false, HelpText = "Indicates whether results should be exported to temp file")]
            public bool Export { get; set; }

            [Option('w', "wildcard", Default = false, HelpText = "Use wildcard search query, which is possible slower" )]
            public bool WildCardSearch { get; set; }
        }

        [Verb("auto", HelpText = "Auto mode to use the tool interactive")]
        private class AutoOptions 
        {

        }

        private readonly Func<TextWriter> m_WriteProvider;
        private readonly ILogger m_Logger;
        private ProgramModes m_ProgramMode = ProgramModes.None;
        private IList<string> m_FileExtentions;
        public string SourcePath { get; private set; }
        public string IndexPath { get; private set; }
        public bool WildCardSearch { get; private set; }
        public bool ExportToFile { get; private set; }
        public string SearchWord { get; private set; }
        public int NumberOfHits { get; private set; }
        public int HitsPerPage { get; private set; }

        public CmdLineHandler(ILogger logger, Func<TextWriter> writerProvider)
        {
            m_WriteProvider = writerProvider;
            m_Logger = logger;
        }

        public ProgramModes GetProgramMode()
        {
            return m_ProgramMode;
        }

        public IList<String> GetFileExtensionsAsList()
        {
            return m_FileExtentions;
        }

        public bool Parse(string[] cmdArgs)
        {
            bool result = false;

            var parser = new Parser(settings =>
            {
                settings.HelpWriter = m_WriteProvider();
                settings.CaseSensitive = false;
            });

            parser.ParseArguments<IndexOptions, SearchOptions, AutoOptions>(cmdArgs)
                .WithParsed<IndexOptions>(o => result = InternalParseIndex(o))
                .WithParsed<SearchOptions>(o => result = InternalParseSearch(o))
                .WithParsed<AutoOptions>(o => result = InternalParseAuto(o));

            return result;
        }

        private bool InternalParseAuto(AutoOptions _) 
        {
            m_ProgramMode = ProgramModes.Auto;
            return true;
        }
        
        private bool InternalParseSearch(SearchOptions options)
        {
            m_ProgramMode = ProgramModes.Search;

            IndexPath = options.IndexPath;
            SearchWord = options.SearchWord;

            NumberOfHits = options.NumberOfHitsToShow;
            if (NumberOfHits == 1000)
            {
                m_Logger.Info("Maximum hits to show will be 1000");
            }
            
            HitsPerPage = options.HitsPerPage;
            if(HitsPerPage == -1) 
            {
                m_Logger.Info("Maximum hits per page will be shown");
            }

            ExportToFile = options.Export;
            WildCardSearch = options.WildCardSearch;

            return true;
        }

        private bool InternalParseIndex(IndexOptions options)
        {
            m_ProgramMode = ProgramModes.Index;

            IndexPath = options.IndexPath;
            SourcePath = options.SourcePath;
            m_FileExtentions = options.FileExtention.Select(e=>NormalizeExtention(e)).ToList();
            
            return true; 
        }

        private static string NormalizeExtention(string e)
        {
            return e.Trim();
        }
    }
}