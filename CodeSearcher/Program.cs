using System;
using System.Collections.Generic;
using System.IO;
using CodeSearcher.BusinessLogic;
using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CodeSearcher
{
    class Program
    {
        private static ICmdLineHandler m_CmdHandler;
        private static ILogger m_Logger;

        static void Main(string[] args)
        {
            m_CmdHandler = new CmdLineHandler();
            if (!m_CmdHandler.Parse(args)) return;

            AttachGlobalExceptionHandler();
            SetUpLogger();

            Console.WriteLine("Welcome to CodeSearcher");

            int mode = m_CmdHandler[m_CmdHandler.ProgramMode] != null
                ? m_CmdHandler.GetProgramModeAsInt()
                : ReadProgramMode();

            var logic = Factory.GetCodeSearcherLogic(
                new LoggerAdapter(m_Logger),
                getIndexPath: () =>
                {
                    var idxPath = m_CmdHandler[m_CmdHandler.IndexPath] != null
                        ? m_CmdHandler[m_CmdHandler.IndexPath]
                        : ReadIndexPath();
                    return idxPath;
                },
                getSourcePath: () =>
                {
                    var srcPath = m_CmdHandler[m_CmdHandler.SourcePath] != null
                        ? m_CmdHandler[m_CmdHandler.SourcePath]
                        : ReadSourcePath();
                    return srcPath;
                },
                getFileExtension: () =>
                {
                    var fileExtensions = m_CmdHandler[m_CmdHandler.FileExtensions] != null
                        ? m_CmdHandler.GetFileExtensionsAsList()
                        : ReadFileExtensions();
                    return fileExtensions;
                });

            if (mode == 1)
            {
                logic.CreateNewIndex(() =>
                {
                    ShowCreateNewIndexHeader();
                },
                (name) => 
                {
                    AsyncLogger.WriteLine(name);
                },
                (fileCount, timeSpan) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine();
                    Console.WriteLine(">> building search index finished!");
                    Console.WriteLine("{0} files indexed", fileCount);
                    Console.WriteLine(">> action take : {0:00}:{1:00}:{2:00}.{3:000}",
                            timeSpan.Hours,
                            timeSpan.Minutes,
                            timeSpan.Seconds,
                            timeSpan.Milliseconds);
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;
                });
            }
            else
            {
                string exportFileName = string.Empty;
                IResultExporter exporter = null;
                bool wildcardSearch;
                if (!bool.TryParse(m_CmdHandler[m_CmdHandler.WildcardSearch], out wildcardSearch))
                {
                    m_Logger.Info("Using Default Searcher");
                    wildcardSearch = false;
                }
                else
                {
                    m_Logger.Info("Using Wildcard Searcher");
                    wildcardSearch = true;
                }

                logic.SearchWithinExistingIndex(
                startCallback: () =>
                {
                    ShowSearchWithinIndexHeader();
                },
                getSearchWord: () =>
                {
                    string word;
                    bool exit;
                    if (m_CmdHandler[m_CmdHandler.SearchedWord] != null)
                    {
                        word = m_CmdHandler[m_CmdHandler.SearchedWord];
                        exit = true;
                    }
                    else
                    {
                        exit = ReadWordToSearch(out word);
                    }

                    word.Trim();

                    return (word, exit);
                },
                getMaximumNumberOfHits: () =>
                {
                    int numberOfHits;
                    if (!int.TryParse(m_CmdHandler[m_CmdHandler.NumberOfHits], out numberOfHits))
                    {
                        m_Logger.Info("Maximum hits to show will be 1000");
                        numberOfHits = 1000;
                    }
                    return numberOfHits;
                },
                getHitsPerPage: () =>
                {
                    int hitsPerPage;
                    if (!int.TryParse(m_CmdHandler[m_CmdHandler.HitsPerPage], out hitsPerPage))
                    {
                        m_Logger.Info("Maximum hits per page will be shown");
                        hitsPerPage = -1;
                    }

                    return hitsPerPage;
                },
                getExporter: () =>
                {
                    bool export;
                    if (!bool.TryParse(m_CmdHandler[m_CmdHandler.ExportToFile], out export))
                    {
                        m_Logger.Info("Results will not be exported");
                        export = false;
                    }
                    
                    if (export)
                    {
                        exportFileName = Path.GetTempFileName();
                        var exportStreamWriter = File.CreateText(exportFileName);
                        exporter = Factory.GetResultExporter(exportStreamWriter);
                    }

                    return (export, exporter);
                }, 
                getSingleResultPrinter: () =>
                {
                    if (wildcardSearch)
                    {
                        return new WildcardResultPrinter();
                    }
                    return new ConsoleResultPrinter();
                },
                finishedCallback: (timeSpan) => 
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(">> searching completed!");
                    Console.WriteLine(">> action take : {0:00}:{1:00}:{2:00}.{3:000}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                },
                endOfSearchCallback: () =>
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                },
                exportFinishedCallback: () =>
                {
                    exporter?.Dispose();
                    Console.WriteLine($"Export file written: {exportFileName}");
                },
                wildcardSearch);
            }

            Console.WriteLine("Programm finished");
            Console.ReadKey();
        }

        private static void SetUpLogger()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
            fileTarget.FileName = "${basedir}/log.txt";
            fileTarget.Layout = "${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Info, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Warn, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            m_Logger = LogManager.GetLogger("CodeSearcher");
        }

        private static void AttachGlobalExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exc = (Exception)args.ExceptionObject;
                var e = exc;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;

                while (e != null)
                {
                    m_Logger.Error(e, string.Empty);
                    e = e.InnerException;
                }

                Environment.FailFast("CodeSearcher: unhandled exception occurs", exc);
            };
        }

        private static int ReadProgramMode()
        {
            int result;
            bool success;
            do
            {
                Console.WriteLine("Please select what you like to do:");
                Console.WriteLine("[1] Index a new Source directory");
                Console.WriteLine("[2] Search within already Indexed Files");
                Console.WriteLine();
                var answer = Console.ReadLine();
                Console.WriteLine();

                success = Int32.TryParse(answer, out result);
                success = result == 1 || result == 2;

            } while (!success);

            return result;
        }

        private static void ShowCreateNewIndexHeader()
        {
            Console.WriteLine("*****************************");
            Console.WriteLine("Index of new Source Directory");
            Console.WriteLine("*****************************");
            Console.WriteLine();
        }

        private static String ReadIndexPath()
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("Please enter path to store index files:");
            var idxPath = Console.ReadLine();
            Console.WriteLine();

            return idxPath;
        }

        private static String ReadSourcePath()
        {
            Console.WriteLine("Please enter path to source directory which should be indexed:");
            var srcPath = Console.ReadLine();
            Console.WriteLine();
            return srcPath;
        }

        private static IList<String> ReadFileExtensions()
        {
            IList<String> fileExtensions;

            int result;
            bool success;
            do
            {
                Console.WriteLine("Please enter extension of files which should be indexed");
                Console.WriteLine("[1] for default types for .cs, .sln, .csproj, .xml");
                Console.WriteLine("[2] enter file extensions by your own");
                Console.WriteLine();
                var answer = Console.ReadLine();
                Console.WriteLine();

                success = Int32.TryParse(answer, out result);
                success = result == 1 || result == 2;

            } while (!success);


            if (result == 1)
            {
                fileExtensions = new List<String>() { ".cs", ".sln", ".csproj", ".xml" };
            }
            else
            {
                fileExtensions = CollectedUserEnteredFileExtensions();
            }

            return fileExtensions;
        }

        private static IList<String> CollectedUserEnteredFileExtensions()
        {
            return m_CmdHandler.GetFileExtensionsAsList();
        }

        private static void ShowSearchWithinIndexHeader()
        {
            Console.WriteLine("***************************");
            Console.WriteLine("Search within indexed Files");
            Console.WriteLine("***************************");
            Console.WriteLine();
        }

        private static bool ReadWordToSearch(out String word)
        {
            Console.WriteLine("Please enter word to search for:");
            Console.WriteLine("Enter #exit if you like to exit programm");
            Console.WriteLine();
            word = Console.ReadLine();
            Console.WriteLine();

            if (String.Compare("#exit", word, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                word = null;
                return true;
            }

            return false;
        }
    }
}
