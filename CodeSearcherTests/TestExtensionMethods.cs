using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeSearcher.Tests.IntegrationTests.TokenizerTest;

namespace CodeSearcher.Tests
{
    internal static class TextExtensions
    {
        internal static void SkipLines(this int number, TokenizerForTesting tokenizer)
        {
            for (int i = 0; i < number; i++)
            {
                tokenizer.SkipLine();
            }
        }
    }
}
