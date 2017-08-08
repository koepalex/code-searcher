using NDesk.Options;
using NDesk.Options.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher
{
    internal class CmdLineHandler 
    {
        private IDictionary<String, String> m_Arguments;

        public const String SourcePath = "SourcePath";
        public const String IndexPath = "IndexPath";
        public const String FileExtensions = "FileExtensions";
        public const String SearchedWord = "SearchedWord";
        public const String ProgramMode = "ProgramMode";

        public String this [String name]
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

		public CmdLineHandler()
		{
			m_Arguments = new Dictionary<String, String>();   
		}

        public int GetProgramModeAsInt()
        {
            if (m_Arguments.ContainsKey(ProgramMode))
            {
                if (m_Arguments[ProgramMode] == "index" || m_Arguments[ProgramMode] == "i")
                    return 1;
                if (m_Arguments[ProgramMode] == "search" || m_Arguments[ProgramMode] == "s")
                    return 2;
            }
            return -1;
        }

        public IList<String> GetFileExtensionsAsList()
        {
            var extensions = new List<String>();

            foreach (var extension in m_Arguments[FileExtensions].Split(new [] {','}))
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
            var mode = os.AddVariable<String>("m|mode", "Select the mode of the tool. \n 'index' to index directory \n 'search' to search within already indexed directory");
            var idxPath = os.AddVariable<String>("ip|indexPath", "Location where the index is or should be stored (mandatory)");
            var srcPath = os.AddVariable<String>("sp|sourcePath", "Location of files, which should be indexed (mandatory in case of 'mode=index')");
			var fileExtensions = os.AddVariable<String>("fe|fileExtensions", "Extensions of files to index (optional in case of 'mode=index', default is '.cs,.xml,.csproj')");
			var searchedWord = os.AddVariable<String>("sw|searchedWord", "word to look for into index (mandatory in case of 'mode=search')");

            try
            {
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
            else
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
                        m_Arguments[ProgramMode] = "index";
                        if (!String.IsNullOrWhiteSpace(idxPath))
                        {
                            m_Arguments[IndexPath] = idxPath;

                            if (!String.IsNullOrWhiteSpace(srcPath))
                            {
                                m_Arguments[SourcePath] = srcPath;

                                if (!String.IsNullOrWhiteSpace(fileExtensions))
                                {
                                    m_Arguments[FileExtensions] = fileExtensions;
                                }
                                else
                                {
                                    m_Arguments[FileExtensions] = ".cs,.xml,.csproj";
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
                    }
                    else if (mode == "search" || mode == "s")
                    {
                        m_Arguments[ProgramMode] = "search";

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
                }
            }

            return argumentsOk;

        }

        private void PrintUsage(OptionSet os)
        {
            Console.WriteLine("CodeSearcher - Usage");
            os.WriteOptionDescriptions(Console.Out);
        }
    }
}
