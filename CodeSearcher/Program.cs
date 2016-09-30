using CodeSearcher.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using CodeSearcher.BusinessLogic.Common;

namespace CodeSearcher
{
    class Program
    {
        private static CmdLineHandler m_CmdHandler;
        private static ILogger m_Logger;
        private static long m_FileCounter;

        static void Main(string[] args)
        {
            m_CmdHandler = new CmdLineHandler();
            if (!m_CmdHandler.Parse(args)) return;

            AttachGlobalExceptionHandler();
            SetUpLogger();

            Console.WriteLine("Welcome to CodeSearcher");

            int mode = m_CmdHandler[CmdLineHandler.ProgramMode] != null
                ? m_CmdHandler.GetProgramModeAsInt()
                : ReadProgramMode();

            if (mode == 1)
            {
                CreateNewIndex();
            } 
            else
            {
                SearchWithinExistingIndex();
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

        private static void CreateNewIndex()
        {
            ShowCreateNewIndexHeader();
            var idxPath = m_CmdHandler[CmdLineHandler.IndexPath] != null
                ? m_CmdHandler[CmdLineHandler.IndexPath]
                : ReadIndexPath();
            
            var srcPath = m_CmdHandler[CmdLineHandler.SourcePath] != null
                ? m_CmdHandler[CmdLineHandler.SourcePath]
                : ReadSourcePath();

            var fileExtensions = m_CmdHandler[CmdLineHandler.FileExtensions] != null
                ? m_CmdHandler.GetFileExtensionsAsList()
                : ReadFileExtensions();

            using (var indexer = Factory.GetIndexer(idxPath, srcPath, fileExtensions))
            {
                indexer.IndexerProcessFile += (sender, args) =>
                {
                    WriteFileName(args.FileName);
                };

                var timeSpan = RunActionWithTimings("Create New Index", () =>
                {
                    var task = indexer.CreateIndex();
                    task.Wait();
                });

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine();
                Console.WriteLine(">> building search index finished!");
                Console.WriteLine("{0} files indexed", m_FileCounter);
                Console.WriteLine(">> action take : {0:00}:{1:00}:{2:00}.{3:000}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
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

        private static void WriteFileName(String name)
        {
            //m_Logger.Trace("* Processed file: {0}", name);
            Interlocked.Increment(ref m_FileCounter);
            //Console.Out.WriteLineAsync(name);
            AsyncLogger.WriteLine(name);
        }

        private static IList<String> CollectedUserEnteredFileExtensions()
        {
            throw new NotImplementedException();
        }

        private static TimeSpan RunActionWithTimings(String name, Action action)
        {
            var sw = new Stopwatch();
            m_Logger.Debug("> start running action: {0}", name);
            
            sw.Start();
            action();
            sw.Stop();

            m_Logger.Debug(string.Empty);
            m_Logger.Debug("> action has finised and took:");
            m_Logger.Debug(">> Complete {0} ticks", sw.ElapsedTicks);

            var timespan = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
            m_Logger.Debug(">> {0:00}:{1:00}:{2:00}.{3:000}",
                timespan.Hours,
                timespan.Minutes,
                timespan.Seconds,
                timespan.Milliseconds);

            m_Logger.Debug(string.Empty);

            return timespan;
        }

        private static void SearchWithinExistingIndex()
        {
            ShowSearchWithinIndexHeader();
            var idxPath = m_CmdHandler[CmdLineHandler.IndexPath] != null
                ? m_CmdHandler[CmdLineHandler.IndexPath]
                : ReadIndexPath();

            bool exit = false;

            using(var searcher = Factory.GetSearcher(idxPath))
            {
                string word;

                do
                {
                    if (m_CmdHandler[CmdLineHandler.SearchedWord] != null)
                    {
                        word = m_CmdHandler[CmdLineHandler.SearchedWord];
                        exit = true;
                    }
                    else
                    {
                        exit = ReadWordToSearch(out word);
                        if (exit) break;
                    }

                    var timeSpan = RunActionWithTimings("Search For " + word, () =>
                    {
                        searcher.SearchFileContent(word, 1000, (searchResultContainer) =>
                        {
                            Console.WriteLine("Found {0} hits", searchResultContainer.NumberOfHits);
                            foreach(var result in searchResultContainer)
                            {
                                var resultPrinter = new ResultPrinter(word, result.FileName);
                                resultPrinter.PrintFileInformation();
                            }
                        });
                    });

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(">> searching completed!");
                    Console.WriteLine(">> action take : {0:00}:{1:00}:{2:00}.{3:000}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();

                    if (exit) break;

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                } while(true);
            }
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

            return String.Compare("#exit", word, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
