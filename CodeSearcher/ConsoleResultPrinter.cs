using CodeSearcher.Interfaces;
using System;
using System.IO;
using System.Text;

namespace CodeSearcher
{
    internal class ConsoleResultPrinter : ISingleResultPrinter
    {
        private static int m_NumbersToShow = -1;
        public int NumbersToShow { get; set; }

        internal ConsoleResultPrinter()
        {
            NumbersToShow = 40;
        }

        public void Print(string fileName, string searchedWord)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("file: {0}", fileName);
            Console.ResetColor();

            var lines = File.ReadAllLines(fileName, Encoding.Default);

            for (int i = 0; i < lines.Length; i++)
            {
                var lineToLower = lines[i].ToLowerInvariant().Trim();
                var searchWordLowerCase = searchedWord.ToLowerInvariant();
                if (lineToLower.Contains(searchWordLowerCase))
                {
                    var line = lines[i].Trim();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("line {0}: ", i+1);
                    Console.ResetColor();

                    //TODO handle multiple hits in one line
                    var indexOfWord = lineToLower.IndexOf(searchWordLowerCase);
                    Console.Write(line.Substring(0, indexOfWord));

                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(searchedWord);
                    Console.ResetColor();

                    int afterHit = indexOfWord + searchedWord.Length;
                    int toEnd = line.Length - afterHit;
                    if (toEnd >= 0)
                    {
                        Console.WriteLine(line.Substring(afterHit, toEnd));
                    }
                    else
                    {
                        Console.WriteLine();
                    }

                    if (NumbersToShow != -1 && (m_NumbersToShow + 1) == NumbersToShow)
                    {
                        Console.WriteLine("... press any key to show more results ...");
                        Console.ReadKey();
                        m_NumbersToShow = -1;
                    }
                    m_NumbersToShow++;
                }
            }
        }
    }
}
