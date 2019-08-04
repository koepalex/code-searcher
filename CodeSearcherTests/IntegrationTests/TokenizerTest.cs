using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeSearcher.BusinessLogic.OwnTokenizer;
using NUnit.Framework;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    public class TokenizerTest
    {
        #region Constants and Enums
        private const string FooClass = "020_SourceCode_FooClass";
        private const string FooClassFile = "class.cs";
        #endregion
        #region Test Classes
        internal class TokenizerForTesting : SourceCodeTokenizer
        {
            public TokenizerForTesting(TextReader input) : base(input)
            {
            }

            internal void SkipLine()
            {
                input.ReadLine();
            }

            internal string ReadToEnd()
            {
                return input.ReadToEnd();
            }

            internal void CheckForTokenAtPositions(IList<int> expectedPosition)
            {
                var chars = this.input.ReadLine().ToCharArray();

                Assert.That(expectedPosition.Any(n => n <= chars.Count()));

                for (int i = 0; i < chars.Length; i++)
                {
                    if (expectedPosition.Contains(i + 1))
                    {
                        Assert.IsFalse(IsTokenChar(chars[i]));
                    }
                }
            }
        }
        #endregion

        #region Setup & TearDown
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData("014_BigBinaryFile");
            var fullPath = Path.Combine(pathToSearch, TestHelper.BigFileName);

            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(pathToSearch);
                TestHelper.CreateBigDummyFile(fullPath);
            }
        }
        #endregion

        #region Tests

        #region 020_SourceCode_FooClass
        [Test]
        public void Test_Detect_Semikolon_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                tok.CheckForTokenAtPositions(new List<int> { 13 });
            });
        }

        [Test]
        public void Test_Detect_Dot_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                1.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 13 });
            });
        }

        [Test]
        public void Test_Detect_Blank_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                3.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 10 });
            });
        }

        [Test]
        public void Test_Detect_OpeningBrace_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                4.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 0 });
            });
        }

        [Test]
        public void Test_Detect_CloseningBrace_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                11.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 3 });
            });
        }

        [Test]
        public void Test_Detect_Tab_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                5.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 0 });
            });
        }

        [Test]
        public void Test_Detect_Equal_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                7.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 22 });
            });
        }

        [Test]
        public void Test_Detect_Minus_Expect_TokenizerChar()
        { 
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                7.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 24 });
            });
        }

        [Test]
        public void Test_Detect_Plus_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                24.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 8 });
            });
        }

        [Test]
        public void Test_Detect_OpeningBracket_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                9.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 13 });
            });
        }

        [Test]
        public void Test_Detect_ClosingBracket_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                9.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 14 });
            });
        }

        [Test]
        public void Test_Detect_DoublePoint_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                9.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 15 });
            });
        }

        [Test]
        public void Test_Detect_Underscore_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                7.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 15 });
            });
        }

        [Test]
        public void Test_Detect_QoutationMark_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                39.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 42 });
            });
        }

        [Test]
        public void Test_Detect_ExcalmationMark_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                39.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 54 });
            });
        }

        [Test]
        public void Test_Detect_Tilde_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                24.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 25 });
            });
        }

        [Test]
        public void Test_Detect_Slash_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                44.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 9 });
            });
        }

        [Test]
        public void Test_Detect_SingleQuote_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                44.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 17 });
            });
        }

        [Test]
        public void Test_Detect_Hash_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                45.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 9 });
            });
        }

        [Test]
        public void Test_Detect_BackSlash_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                46.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 35 });
            });
        }

        [Test]
        public void Test_Detect_At_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                59.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 37 });
            });
        }

        [Test]
        public void Test_Detect_LeftAngleBracket_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                61.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 24 });
            });
        }

        [Test]
        public void Test_Detect_RightAngleBracket_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                61.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 31 });
            });
        }

        [Test]
        public void Test_Detect_Dollar_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                63.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 45 });
            });
        }

        [Test]
        public void Test_Detect_OpeningSquareBracket_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                63.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 40 });
            });
        }

        [Test]
        public void Test_Detect_ClosingSquareBracket_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                63.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 41 });
            });
        }

        [Test]
        public void Test_Detect_Or_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                66.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 41 });
            });
        }

        [Test]
        public void Test_Detect_And_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                67.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 42 });
            });
        }

        [Test]
        public void Test_Detect_XOr_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                73.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 23 });
            });
        }

        [Test]
        public void Test_Detect_QuestionMark_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                69.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 47 });
            });
        }

        [Test]
        public void Test_Detect_Multiply_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                78.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 23 });
            });
        }

        [Test]
        public void Test_Detect_Modulo_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                78.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 27 });
            });
        }

        [Test]
        public void Test_Detect_Comma_Expect_TokenizerChar()
        {
            LoadFileAndTokenizer(FooClass, FooClassFile, (tok) => {
                63.SkipLines(tok);
                tok.CheckForTokenAtPositions(new List<int> { 60 });
            });
        }
        #endregion

        #region 014_BigBinaryFile
        [Test]
        public void Test_BinaryFile_Expect_NoCrash()
        {
            LoadFileAndTokenizer("014_BigBinaryFile", TestHelper.BigFileName, (tok) => {
                var text = tok.ReadToEnd();
                Console.WriteLine(text);
            });
        }
        #endregion
        #endregion

        #region Private Implementation
        private void LoadFileAndTokenizer(string path, string fileName, Action<TokenizerForTesting> action)
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData(path);
            string fullPath = Path.Combine(pathToSearch, fileName);

            using (StreamReader reader = File.OpenText(fullPath))
            {
                var tokenizer = new TokenizerForTesting(reader);

                action(tokenizer);
            }
        }
        #endregion
    }
}
