using NDesk.Options;
using NDesk.Options.Extensions;
using System;
using System.Collections.Generic;
using CodeSearcher.Interfaces;
using System.IO;

namespace CodeSearcher
{
    internal class CmdLineHandler : ICmdLineHandler
    {
        private readonly IDictionary<String, String> m_Arguments;
        private readonly Func<TextWriter> m_WriterProvider;
        public String SourcePath => "SourcePath";
        public String IndexPath => "IndexPath";
        internal const  string m_FileExtensions = "FileExtensions";
        public String SearchedWord => "SearchedWord";
        internal const String m_ProgramMode = "ProgramMode";
        public String NumberOfHits => "NumberOfHits";
        public String HitsPerPage => "HitsPerPage";
        public String ExportToFile => "ExportToFile";
        public String WildcardSearch => "WildcardSearch";

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
            m_WriterProvider = writerProvider;
            
            if(m_WriterProvider == null) 
            {
                m_WriterProvider = () => Console.Out;
            } 

            m_Arguments = new Dictionary<String, String>();
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
            if (this[m_FileExtensions] == null) 
            {
                return null;
            }

            var extensions = new List<String>();

            foreach (var extension in m_Arguments[m_FileExtensions].Split(new[] { ',' }))
            {
                extensions.Add(extension);
            }

            return extensions;
        }

        public bool Parse(string[] cmdArgs)
        {
            bool argumentsOk = true;
            var os = new OptionSet();
            var showHelp = os.AddSwitch("h|?|help", "Shows the help");
            var mode = os.AddVariable<String>("m|mode", "Select the mode of the tool. \n 'index' to index directory \n 'search' to search within already indexed directory \n 'auto' to use CLI");
            var idxPath = os.AddVariable<String>("ip|indexPath", "Location where the index is or should be stored (mandatory)");
            var srcPath = os.AddVariable<String>("sp|sourcePath", "Location of files, which should be indexed (mandatory in case of 'mode=index')");
            var fileExtensions = os.AddVariable<String>("fe|fileExtensions", "Extensions of files to index (optional in case of 'mode=index', default is '.cs,.xml,.csproj')");
            var searchedWord = os.AddVariable<String>("sw|searchedWord", "word to look for into index (mandatory in case of 'mode=search')");
            var numberOfHitsToShow = os.AddVariable<Int32>("hits|numerOfHits", "Amount of files with findings (optional, default is 1000)");
            var hitsPerPage = os.AddVariable<Int32>("hpp|hitsPerPage", "Amount of findings to show at once (optional, default = -1; -1 means all)");
            var export = os.AddSwitch("e|export", "Indicates wheater results should be exported to temp file (optional, default = false)");
            var wildcardSearch = os.AddSwitch("wc|wildcard", "Use wildcard search query, which is possible slower (optional, default = false)");

            try
            {
                for (int i = 0; i < cmdArgs.Length; i++)
                {
                    cmdArgs[i] = cmdArgs[i].Trim();
                }
                os.Parse(cmdArgs);
            }
            catch (OptionException)
            {
                PrintUsage(os);
                argumentsOk = false;
            }

            if (showHelp)
            {
                PrintUsage(os);
            }
            else if (argumentsOk)
            {
                if (String.IsNullOrWhiteSpace(mode))
                {
                    PrintUsage(os);
                    argumentsOk = false;
                }
                else
                {
                    if (mode == "index" || mode == "i")
                    {
                        argumentsOk = SetArgumentsForIndexing(os, idxPath, srcPath, fileExtensions);
                    }
                    else if (mode == "search" || mode == "s")
                    {
                        argumentsOk = SetArgumentsForSearching(os, idxPath, searchedWord, numberOfHitsToShow, hitsPerPage, export, wildcardSearch);
                    }
                    else if (mode == "auto" || mode == "a")
                    {
                        m_Arguments[m_ProgramMode] = mode;
                        argumentsOk = true;
                    }
                    else
                    {
                        PrintUsage(os);
                        argumentsOk = false;
                    }
                }
            }

            return argumentsOk;

        }

        private bool SetArgumentsForIndexing(OptionSet os, Variable<string> idxPath, Variable<string> srcPath, Variable<string> fileExtensions)
        {
            bool argumentsOk = true;
            m_Arguments[m_ProgramMode] = "index";
            if (!String.IsNullOrWhiteSpace(idxPath))
            {
                m_Arguments[IndexPath] = idxPath;

                if (!String.IsNullOrWhiteSpace(srcPath))
                {
                    m_Arguments[SourcePath] = srcPath;

                    if (!String.IsNullOrWhiteSpace(fileExtensions))
                    {
                        m_Arguments[m_FileExtensions] = fileExtensions;
                    }
                    else
                    {
                        m_Arguments[m_FileExtensions] = ".cs,.xml,.csproj";
                    }
                }
                else
                {
                    PrintUsage(os);
                    argumentsOk = false;
                }
            }
            else
            {
                PrintUsage(os);
                argumentsOk = false;
            }

            return argumentsOk;
        }

        private bool SetArgumentsForSearching(OptionSet os, Variable<string> idxPath, Variable<string> searchedWord, Variable<int> numberOfHitsToShow, Variable<int> hitsPerPage, Switch export, Switch wildcardSearch)
        {
            bool argumentsOk = true;
            m_Arguments[m_ProgramMode] = "search";

            if (!String.IsNullOrWhiteSpace(idxPath))
            {
                m_Arguments[IndexPath] = idxPath;
                if (!String.IsNullOrWhiteSpace(searchedWord))
                {
                    m_Arguments[SearchedWord] = searchedWord;
                }
                else
                {
                    PrintUsage(os);
                    argumentsOk = false;
                }
                if (numberOfHitsToShow > 0)
                {
                    m_Arguments[NumberOfHits] = numberOfHitsToShow.Value.ToString();
                }
                else
                {
                    m_Arguments[NumberOfHits] = 1000.ToString();
                }

                if (hitsPerPage == -1 || hitsPerPage > 0)
                {
                    m_Arguments[HitsPerPage] = hitsPerPage.Value.ToString();
                }
                else
                {
                    m_Arguments[HitsPerPage] = "-1";
                }

                if (export.Enabled)
                {
                    m_Arguments[ExportToFile] = true.ToString();
                }

                if (wildcardSearch.Enabled)
                {
                    m_Arguments[WildcardSearch] = true.ToString();
                }
            }
            else
            {
                PrintUsage(os);
                argumentsOk = false;
            }

            return argumentsOk;
        }

        private void PrintUsage(OptionSet os)
        {
            var writer = m_WriterProvider();
            if (writer != null)
            {
                writer.WriteLine("CodeSearcher - Usage");
                os.WriteOptionDescriptions(writer);
            }
        }
    }
}
