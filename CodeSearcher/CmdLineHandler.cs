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

        /// <summary>
        /// Option class for parsing purposes
        /// The options of the command line parser only supports characters 
        /// as shortnames. A shortcut starts with a single - and a descriptive 
        /// attribute starts with a double -
        /// 
        /// To support a stable interface the shortcuts are modeled by an own option.
        /// In this setting it is possible to add --ip and --indexPath which is
        /// an open issue.
        /// </summary>
        private class Options
        {
            [Option('m', "mode", HelpText = "Select the mode of the tool. \n 'index' to index directory \n 'search' to search within already indexed directory \n 'auto' to use CLI")]
            public string Mode { get; set; }

            [Option("indexPath", HelpText = "Location where the index is or should be stored (mandatory), short --ip")]
            public string IndexPathLong { get; set; }

            [Option("ip", Hidden = true)]
            public string IndexPathShort { get; set; }

            [Option("sourcePath", HelpText = "Location of files, which should be indexed (mandatory in case of 'mode=index')")]
            public string SourcePathLong { get; set; }

            [Option("sp", Hidden = true)]
            public string SourcePathShort { get; set; }

            [Option("fileExtensions", Min = 1, Separator = ',', HelpText = "Extensions of files to index (optional in case of 'mode=index', default is '.cs,.xml,.csproj')")]
            public IEnumerable<string> FileExtentionLong { get; set; } 

            [Option("fe", Hidden = true, Min = 1, Separator = ',')]
            public IEnumerable<string> FileExtentionShort { get; set; } 
            
            //hpp|hitsPerPage
            [Option("hpp", Default = -1, Hidden = true)]
            public int HitsPerPageShort { get; set; }
            
            [Option("hitsPerPage", Default = -1, HelpText = "Amount of findings to show at once (optional, default = -1; -1 means all)")]
            public int HitsPerPageLong { get; set; }

            [Option("hits", Default = -1, Hidden = true)]
            public int NumberOfHitsToShowShort { get; set; }
            
            [Option("numerOfHits", Default = -1, HelpText = "Amount of files with findings (optional, default is 1000)")]
            public int NumberOfHitsToShowLong { get; set; }

            [Option("sw", Hidden = true)]
            public string SearchWordShort { get; set; }

            [Option("searchWord", HelpText = "word to look for into index (mandatory in case of 'mode=search')")]
            public string SearchWordLong { get; set; }

            [Option('e', "export", Default = false, HelpText = "Indicates wheater results should be exported to temp file (optional, default = false)")]
            public bool Export { get; set; }

            [Option("wc", Hidden = true)]
            public bool WildCardSearchShort { get; set; }

            [Option("wildcard", HelpText = "Use wildcard search query, which is possible slower (optional, default = false)" )]
            public bool WildCardSearchLong { get; set; }
        }

        public bool Parse(string[] cmdArgs)
        {
            bool result = false;

            var parser = new Parser(settings =>
            {
                settings.HelpWriter = m_WriteProvider();
                settings.AutoHelp = false;
                settings.CaseSensitive = false;
            });
            

            var parserOutput = parser.ParseArguments<Options>(cmdArgs);

            parserOutput.WithParsed
            (
                o => result = InternalParse(o)
            );
            bool parserError = false;
            parserOutput.WithNotParsed(e => parserError = e.Any());


            if(!result && !parserError)
            {
                parser.ParseArguments<Options>(new string[] { "--help" });
            }

            return result;
        }

        private bool InternalParse(Options options) 
        {
            var mode = options.Mode?.TrimStart('=');
            if(mode == "i" || mode == "index") 
            {
                return InternalParseIndex(options);
            }
            else if(mode == "s" || mode == "search") 
            {
                return InternalParseSearch(options);
            }
            else if(mode == "a" || mode == "auto") 
            {
                m_ProgramMode = ProgramModes.Auto;
                return true;
            }
            else 
            {
                return false;
            }
        }
        
        private bool InternalParseSearch(Options options)
        {
            m_ProgramMode = ProgramModes.Search;

            if(!ParseIndexPath(options))
            {

                return false;
            }

            string searchWord;
            if (!string.IsNullOrEmpty(options.SearchWordLong)) 
            {
                searchWord = options.SearchWordLong;
            } 
            else if(!string.IsNullOrEmpty(options.SearchWordShort)) 
            {
                searchWord = options.SearchWordShort;
            }
            else 
            {
                return false;
            }

            SearchWord = searchWord;
            
            if(options.NumberOfHitsToShowLong != -1) 
            {
                NumberOfHits = options.NumberOfHitsToShowLong;
            }
            else if(options.NumberOfHitsToShowShort != -1)
            {
                NumberOfHits = options.NumberOfHitsToShowShort;
            }
            else
            {
                NumberOfHits = 1000;
                m_Logger.Info("Maximum hits to show will be 1000");
            }

            ExportToFile = options.Export;
                 
            if(options.HitsPerPageLong != -1) 
            {
                HitsPerPage = options.HitsPerPageLong;
            }
            else if(options.HitsPerPageShort != -1) 
            {
                HitsPerPage = options.HitsPerPageShort;
            }
            else
            {
                m_Logger.Info("Maximum hits per page will be shown");
                HitsPerPage = -1;
            }
            
            // ugly but needed to support a bool flag with to names.
            WildCardSearch = options.WildCardSearchLong||options.WildCardSearchShort;
            
            return true;
        }

        private bool ParseIndexPath(Options options)
        {
            if (!string.IsNullOrWhiteSpace(options.IndexPathLong))
            {
                IndexPath = options.IndexPathLong;
            }
            else if(!string.IsNullOrWhiteSpace(options.IndexPathShort)) 
            {
                IndexPath = options.IndexPathShort;
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool InternalParseIndex(Options options)
        {
            m_ProgramMode = ProgramModes.Index;

            if(!ParseIndexPath(options)) 
            {
                return false;
            }
            
            if (!string.IsNullOrWhiteSpace(options.SourcePathLong)) 
            {
                SourcePath = options.SourcePathLong;
            }
            else if(!string.IsNullOrWhiteSpace(options.SourcePathShort))             
            {
                SourcePath = options.SourcePathShort;
            }
            else 
            {
                return false; 
            }
            
            if(options.FileExtentionLong.Any())
            {
                m_FileExtentions = options.FileExtentionLong.ToList();
            } 
            else if(options.FileExtentionShort.Any()) 
            {
                m_FileExtentions = options.FileExtentionShort.ToList();
            }
            else
            {
                m_FileExtentions = new string[] { ".cs", ".xml", ".csproj" };
            }
            
            return true; 
        }
    }
}