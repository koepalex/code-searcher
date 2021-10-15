﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using CodeSearcher.BusinessLogic;
using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CodeSearcher
{
    internal class Program
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

            var mode = m_CmdHandler.GetProgramMode();
            if(mode == ProgramModes.None) 
            {
                mode= ReadProgramMode();
            }
            
            ICodeSearcherLogic logic = GetCodeSearcherLogic();
            var manager = Factory.Get().GetCodeSearcherManager(new LoggerAdapter(m_Logger));
            
            switch (mode)
            {
                case ProgramModes.Index:
                    CreateIndex(logic); break;
                case ProgramModes.Search:
                    SearchInExistingIndex(logic); break;
                case ProgramModes.Auto:
                    var tui = new TextBasedUserInterface();
                    var nav = new MenuNavigator();
                    ShowConsoleMainMenu(manager, tui, nav); 
                    break;
            }

            Console.WriteLine("Programm finished");
            Console.ReadKey();
        }


        private static ICodeSearcherLogic GetCodeSearcherLogic()
        {
            return Factory.Get().GetCodeSearcherLogic(
                new LoggerAdapter(m_Logger),
                getIndexPath: () =>
                {
                    var idxPath = m_CmdHandler[m_CmdHandler.IndexPath] ?? ReadIndexPath();
                    return idxPath;
                },
                getSourcePath: () =>
                {
                    var srcPath = m_CmdHandler[m_CmdHandler.SourcePath] ?? ReadSourcePath();
                    return srcPath;
                },
                getFileExtension: () =>
                {
                    var fileExtensions = m_CmdHandler.GetFileExtensionsAsList() ?? ReadFileExtensions();
                    return fileExtensions;
                });
        }
        
        private static void CreateIndex(ICodeSearcherLogic logic)
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
        
        private static void SearchInExistingIndex(ICodeSearcherLogic logic)
        {
            string exportFileName = string.Empty;
            IResultExporter exporter = null;
            if (bool.TryParse(m_CmdHandler[m_CmdHandler.WildcardSearch], out bool wildcardSearch))
            {
                m_Logger.Info("Using Wildcard Searcher");
                wildcardSearch = true;
            }
            else
            {
                m_Logger.Info("Using Default Searcher");
                wildcardSearch = false;
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

                word?.Trim();

                return (word, exit);
            },
            getMaximumNumberOfHits: () =>
            {
                if (!int.TryParse(m_CmdHandler[m_CmdHandler.NumberOfHits], out int numberOfHits))
                {
                    m_Logger.Info("Maximum hits to show will be 1000");
                    numberOfHits = 1000;
                }
                return numberOfHits;
            },
            getHitsPerPage: () =>
            {
                if (!int.TryParse(m_CmdHandler[m_CmdHandler.HitsPerPage], out int hitsPerPage))
                {
                    m_Logger.Info("Maximum hits per page will be shown");
                    hitsPerPage = -1;
                }

                return hitsPerPage;
            },
            getExporter: () =>
            {
                if (!bool.TryParse(m_CmdHandler[m_CmdHandler.ExportToFile], out bool export))
                {
                    m_Logger.Info("Results will not be exported");
                    export = false;
                }

                if (export)
                {
                    exportFileName = Path.GetTempFileName();
                    var exportStreamWriter = File.CreateText(exportFileName);
                    exporter = wildcardSearch
                        ? Factory.Get().GetWildcardResultExporter(exportStreamWriter)
                        : Factory.Get().GetDefaultResultExporter(exportStreamWriter);
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

        internal static void ShowConsoleMainMenu(ICodeSearcherManager manager, ITextBasedUserInterface tui, IMenuNavigator nav)
        {
            do
            {
                tui.Clear();
                tui.WriteLine("[1] Create New Index");
                tui.WriteLine("[2] Show all Indexes");
                tui.WriteLine("[3] Exit");
                tui.WriteLine("Please choose: ");
                var answer = tui.ReadLine();
                if (int.TryParse(answer, out int selection))
                {
                    if (1.Equals(selection)) //Create New Index
                    {
                        nav.GoToCreateNewIndexMenu(manager, tui);
                    }
                    else if (2.Equals(selection)) //Show All Indexes
                    {
                        nav.GoToShowAllIndexesMenu(manager, tui);
                    }
                    else if (3.Equals(selection)) //Exit
                    {
                        nav.ExitMenu();
                    }
                }
            } while (nav.MenuLoopActive());
        }

        internal static void ShowCreateNewIndexMenu(ICodeSearcherManager manager, ITextBasedUserInterface tui, IMenuNavigator nav)
        {
            string answer;
            // Source path
            string sourcePath = null;
            do
            {
                tui.WriteLine("Please enter Path with Sources to Index:");
                answer = tui.ReadLine();
                if (Directory.Exists(answer))
                {
                    sourcePath = answer;
                    tui.WriteLine($"Path with files to Index: {sourcePath}");
                    break;
                }
                tui.WriteLine("Path do not exist!");
            } while (tui.ShouldLoop());

            // file extensions
            var extensions = new List<String>();
            tui.WriteLine("Please select file extension to index (form .ext1,.ext2)");
            tui.WriteLine("Leave empty to use (.cs,.xml,.csproj) ");
            answer = tui.ReadLine();
            if (string.IsNullOrWhiteSpace(answer))
            {
                extensions.Add(".cs");
                extensions.Add(".xml");
                extensions.Add(".csproj");
            }
            else
            {
                foreach (var extension in answer.Split(new[] { ',' }))
                {
                    extensions.Add(extension);
                }
            }

            tui.WriteLine("Looking for files with extensions: ");
            extensions.ForEach((ext) => tui.WriteLine($"File Extension: {ext}"));

            if (!string.IsNullOrWhiteSpace(sourcePath) && extensions.Count > 0)
            {
                var id = manager.CreateIndex(sourcePath, extensions);
                tui.WriteLine($"New Index created with ID: {id}");
                do
                {
                    tui.WriteLine("[1] Search in new created index");
                    tui.WriteLine("[2] Back to main menu");
                    tui.WriteLine("Please choose: ");
                    answer = tui.ReadLine();
                    if (int.TryParse(answer, out int selection))
                    {
                        if (1.Equals(selection))
                        {
                            var selectedIndex = manager.GetIndexById(id);
                            nav.GoToSelectedIndexMenu(manager, selectedIndex, tui);
                        }
                        else if (2.Equals(selection))
                        {
                            nav.GoToMainMenu(tui);
                        }
                    }
                } while (tui.ShouldLoop());
            }
        }

        internal static void ShowAllIndexesMenu(ICodeSearcherManager manager, ITextBasedUserInterface tui, IMenuNavigator nav)
        {
            string answer;
            do
            {
                tui.Clear();
                var indexes = manager.GetAllIndexes().ToList();
                int count = 0;
                foreach (var index in indexes)
                {
                    tui.WriteLine($"[{++count}] - ID {index.ID} - SourcePath {index.SourcePath}");
                }

                if (indexes.Count == 0)
                {
                    tui.WriteLine("There are currently no folders indexed!");
                }

                tui.WriteLine($"[{++count}] Return to main menu");
                tui.WriteLine("Please choose: ");
                answer = tui.ReadLine();
                if (int.TryParse(answer, out int selection))
                {
                    if (indexes.Count > 0 && selection < count)
                    {
                        var selectedIndex = indexes[selection - 1];
                        nav.GoToSelectedIndexMenu(manager, selectedIndex, tui);
                    }
                    else
                    {
                        nav.GoToMainMenu(tui);
                    }
                }
            } while (tui.ShouldLoop());
        }

        internal static void ShowSelectedIndexMenu(ICodeSearcherManager manager, ICodeSearcherIndex selectedIndex, ITextBasedUserInterface tui, IMenuNavigator nav)
        {
            string answer;

            do
            {
                tui.WriteLine($"ID:\t\t{selectedIndex.ID}");
                tui.WriteLine($"Source:\t\t{selectedIndex.SourcePath}");
                tui.Write($"File Extensions:\t\t");
                foreach (var extension in selectedIndex.FileExtensions)
                {
                    tui.Write($"{extension}, ");
                }
                tui.WriteLine();
                tui.WriteLine($"Created on: {selectedIndex.CreatedTime.ToString("yyyy-MM-dd H:mm:ss")}");
                tui.WriteLine("[1] Search in Index");
                tui.WriteLine("[2] Delete Index");
                tui.WriteLine("[3] Return to main menu");
                tui.WriteLine("Please choose: ");
                answer = tui.ReadLine();
                if (int.TryParse(answer, out int selection))
                {
                    if (1.Equals(selection))
                    {
                        var logic = GetCodeSearcherLogicByIndex(selectedIndex);
                        logic.SearchWithinExistingIndex(
                            startCallback: () => { },
                            getSearchWord: () =>
                            {
                                bool exit = ReadWordToSearch(out string word);
                                word?.Trim();
                                return (word, exit);
                            },
                            getMaximumNumberOfHits: () => { return 200; },
                            getHitsPerPage: () => { return 50; },
                            getExporter: () => { return (false, null); },
                            getSingleResultPrinter: () => { return new WildcardResultPrinter(); },
                            finishedCallback: (timeSpan) =>
                            {
                                Console.WriteLine("Press any key to continue");
                                Console.ReadKey();
                            },
                            endOfSearchCallback: () => { },
                            wildcardSearch: true
                        );
                    }
                    else if (2.Equals(selection))
                    {
                        manager.DeleteIndex(selectedIndex.ID);
                        tui.WriteLine($"Index with ID {selectedIndex.ID} deleted!");
                    }
                    else if (3.Equals(selection))
                    {
                        nav.GoToMainMenu(tui);
                    }
                }
            } while (tui.ShouldLoop());
        }

        private static ICodeSearcherLogic GetCodeSearcherLogicByIndex(ICodeSearcherIndex selectedIndex)
        {
            return Factory.Get().GetCodeSearcherLogic(
                new LoggerAdapter(m_Logger),
                getIndexPath: () => selectedIndex.IndexPath,
                getSourcePath: () => selectedIndex.SourcePath,
                getFileExtension: () => selectedIndex.FileExtensions
            );
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

        private static ProgramModes ReadProgramMode()
        {
            ProgramModes result;
            bool success;
            do
            {
                Console.WriteLine("Please select what you like to do:");
                Console.WriteLine("[1] Index a new Source directory");
                Console.WriteLine("[2] Search within already Indexed Files");
                Console.WriteLine();
                var answer = Console.ReadLine();
                Console.WriteLine();

                result = (ProgramModes)Enum.Parse(typeof(ProgramModes), answer, false);
                success = result != ProgramModes.None;

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

                _ = Int32.TryParse(answer, out result);
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
            Console.Clear();
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
