using CodeSearcher.BusinessLogic;
using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
using CodeSearcher.Tests.Helper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    [Category("SafeForCI")]
    class CodeSearcherLogicTests
    {
        private string SourcePath { get; set; }
        private string IndexPath { get; set; }
        private string Index { get; set; }
        
        private string MetaFolder { get; set; }

        private ICodeSearcherLogger Logger => new TestLogger();


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SourcePath = TestHelper.GetPathToIntegrationTestData("040_CodeSearcherLogic");
            IndexPath = Path.Combine(SourcePath, FolderNames.DefaultLuceneIndexName);
            Index = Path.Combine(SourcePath, "FolderWithIndex");
            MetaFolder = Path.Combine(SourcePath, "Meta");

            if (!Directory.Exists(MetaFolder))
            {
                Directory.CreateDirectory(MetaFolder);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(MetaFolder))
            {
                Directory.Delete(MetaFolder, true);
                Directory.CreateDirectory(MetaFolder);
            }
        }

        [Test]
        public void Test_CreateLogic_Expect_NotNull()
        {
            var logic = Factory.Get().GetCodeSearcherLogic(Logger, () => IndexPath, () => Index, () => new List<string>() { ".md" });
            Assert.NotNull(logic);
        }

        [Test]
        public void Test_IndexFolder_Expect_IgnoreCodeSearcherFolder()
        {
            // "fake" lucene files: folder .code-searcher only contains .md and parent folder contain additional css file 
            var logic = Factory.Get().GetCodeSearcherLogic(Logger, () => IndexPath, () => Index, () => new List<string>() { ".md", ".css" });

            logic.CreateNewIndex(
                () => { },
                (file) => {
                    Assert.That(Path.GetExtension(file), Is.Not.EqualTo(".md"));
                    Assert.That(Path.GetExtension(file), Is.EqualTo(".css"));
                },
                (count, timeSpan) => {
                    Assert.That(count, Is.EqualTo(1));
                });
        }
    }
}
