using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace CodeSearcher
{
    internal class CmdLineHandler : ICmdLineHandler
    {
        private readonly IDictionary<String, String> m_Arguments;
        private readonly Func<TextWriter> m_WriteProvider;
        private const string m_FileExtensions = "FileExtensions";
        private const string m_ProgramMode = "ProgramMode";
        private IList<string> m_FileExtentions;
        public String SourcePath => "SourcePath";
        public String IndexPath => "IndexPath";
        public String SearchedWord => "SearchedWord";
        public String HitsPerPage => "HitsPerPage";
        public String ExportToFile => "ExportToFile";
        public String WildcardSearch => "WildcardSearch";
        public String NumberOfHits => "NumberOfHits";

        public String this[String name]
        {
            get
            {
                if (m_Arguments.ContainsKey(name))
                {
                    return m_Arguments[name];
                }
                return null;
            }
        }

        public CmdLineHandler(Func<TextWriter> writerProvider = null)
        {
            m_Arguments = new Dictionary<string, string>();
            m_WriteProvider = writerProvider;
        }

        public ProgramModes GetProgramMode()
        {
            if (m_Arguments.ContainsKey(m_ProgramMode))
            {
                if (m_Arguments[m_ProgramMode] == "index" || m_Arguments[m_ProgramMode] == "i")
                    return ProgramModes.Index;
                if (m_Arguments[m_ProgramMode] == "search" || m_Arguments[m_ProgramMode] == "s")
                    return ProgramModes.Search;
                if (m_Arguments[m_ProgramMode] == "auto" || m_Arguments[m_ProgramMode] == "a")
                    return ProgramModes.Auto;
            }
            return ProgramModes.None;
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
            [Option('m', "mode", Required = true, HelpText = "Select the mode of the tool. \n 'index' to index directory \n 'search' to search within already indexed directory \n 'auto' to use CLI")]
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
            public IEnumerable<string> FileExtentionLong { get; set;} 

            [Option("fe", Hidden = true, Min = 1, Separator = ',')]
            public IEnumerable<string> FileExtentionShort { get; set;} 
            
            //hpp|hitsPerPage
            [Option("hpp", Hidden = true)]
            public string HitsPerPageShort { get; set; }
            
            [Option("hitsPerPage", HelpText = "Amount of findings to show at once (optional, default = -1; -1 means all)")]
            public string HitsPerPageLong { get; set; }


            [Option("hits", Hidden = true)]
            public string NumberOfHitsToShowShort { get; set; }
            
            [Option("numerOfHits", HelpText = "Amount of files with findings (optional, default is 1000)")]
            public string NumberOfHitsToShowLong { get; set; }

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
            });

            var parserOut = parser.ParseArguments<Options>(cmdArgs).WithParsed
            (
                o => result = InternalParse(o)
            );

            if(!result) {
                parser.ParseArguments<Options>(new string[] { "--help" });
            }

            return result;
        }

        private bool InternalParse(Options options) 
        {
            var mode = options.Mode.TrimStart('=');
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
                m_Arguments[m_ProgramMode] = mode;
                return true;
            }
            else 
            {
                return false;
            }
        }

        private bool InternalParseSearch(Options options)
        {
            m_Arguments[m_ProgramMode] = "search";

            if(!ParseIndexPath(options))
            {
                return false;
            }

            string searchWord;
            if(!string.IsNullOrEmpty(options.SearchWordLong)) 
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
            m_Arguments[SearchedWord] = searchWord;

            string numberOfHits = (1000).ToString();
            if(!string.IsNullOrEmpty(options.NumberOfHitsToShowLong)) 
            {
                numberOfHits = options.NumberOfHitsToShowLong;
            }
            else if(!string.IsNullOrEmpty(options.NumberOfHitsToShowShort)) 
            {
                numberOfHits = options.NumberOfHitsToShowShort;
            }
            //really hard but needed to fulfil the interface
            m_Arguments[NumberOfHits] = Int32.Parse(numberOfHits).ToString();
            
            if(options.Export) 
            {
                m_Arguments[ExportToFile] = options.Export.ToString();
            }
            
            string hitsPerPage = (-1).ToString();
            if(!string.IsNullOrEmpty(options.HitsPerPageLong)) 
            {
                hitsPerPage = options.HitsPerPageLong;
            }
            else if(!string.IsNullOrEmpty(options.HitsPerPageShort)) 
            {
                hitsPerPage = options.HitsPerPageShort;
            }
            //really hard but needed to fulfil the interface
            m_Arguments[HitsPerPage] = Int32.Parse(hitsPerPage).ToString();
            
            // ugly but needed to support a bool flag with to names.
            if(options.WildCardSearchLong||options.WildCardSearchShort) 
            {
                m_Arguments[WildcardSearch] = true.ToString();
            }

            return true;
        }

        private bool ParseIndexPath(Options options)
        {
            string indexPath;
            if (!string.IsNullOrWhiteSpace(options.IndexPathLong))
            {
                indexPath = options.IndexPathLong;
            }
            else if(!string.IsNullOrWhiteSpace(options.IndexPathShort)) 
            {
                indexPath = options.IndexPathShort;
            }
            else
            {
                return false;
            }

            m_Arguments[IndexPath] = indexPath;

            return true;
        }

        private bool InternalParseIndex(Options options)
        {
            m_Arguments[m_ProgramMode] = "index";

            if(!ParseIndexPath(options)) 
            {
                return false;
            }

            string sourcePath;
            if (!string.IsNullOrWhiteSpace(options.SourcePathLong)) 
            {
                sourcePath = options.SourcePathLong;
            }
            else if(!string.IsNullOrWhiteSpace(options.SourcePathShort))             
            {
                sourcePath = options.SourcePathShort;
            }
            else 
            {
                return false; 
            }
            
            m_Arguments[SourcePath] = sourcePath;

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
            
            //for the interface stability to the tests
            m_Arguments[m_FileExtensions] = string.Join(',', m_FileExtentions);
            
            return true; 
        }
    }
}