using CodeSearcher.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeSearcher
{
    internal class WildcardResultPrinter : ISingleResultPrinter
    {
        private static int m_NumbersToShow = -1;
        public int NumbersToShow { get; set; }

        internal WildcardResultPrinter()
        {
            NumbersToShow = 40;
        }

        public void Print(string fileName, string wildcardPattern)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("file: {0}", fileName);
            Console.ResetColor();

            var lines = File.ReadAllLines(fileName, Encoding.Default);

            for (int i = 0; i < lines.Length; i++)
            {
                
                if (Regex.IsMatch(lines[i], WildcardResolver(wildcardPattern)))
                {
                    //TODO handle multiple hits in one line
                    var match = Regex.Match(lines[i], WildcardResolver(wildcardPattern));
                    
                    var line = lines[i].Trim();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("line {0}: ", i + 1);
                    Console.ResetColor();

                    var matchedValue = match.Value.Trim();
                    var indexOfWord = line.IndexOf(matchedValue);
                    Console.Write(line.Substring(0, indexOfWord));

                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(match.Value);
                    Console.ResetColor();

                    int afterHit = indexOfWord + matchedValue.Length;
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

        private static String WildcardResolver(String value)
        {
            return Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*");
        }
    }

}
