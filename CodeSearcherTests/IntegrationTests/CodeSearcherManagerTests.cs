using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Tests.Helper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    [Category("SafeForCI")]
    public class CodeSearcherManagerTests
    {
        private string SourcePath { get; set; }
        private string Index1 { get; set; }
        private string Index2 { get; set; }
        private string Index3 { get; set; }
        private string MetaFolder { get; set; }

        private ICodeSearcherLogger Logger => new TestLogger();
        

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SourcePath = TestHelper.GetPathToIntegrationTestData("030_CodeSearcherManager");
            Index1 = Path.Combine(SourcePath, "Folder1");
            Index2 = Path.Combine(SourcePath, "Folder2");
            Index3 = Path.Combine(SourcePath, "Folder3");
            MetaFolder = Path.Combine(SourcePath, "Meta");
            
            if(!Directory.Exists(MetaFolder))
            {
                Directory.CreateDirectory(MetaFolder);
            } 
        }


        [TearDown]
        public void TearDown ()
        {
            if (Directory.Exists(MetaFolder))
            {
                Directory.Delete(MetaFolder, true);
                Directory.CreateDirectory(MetaFolder);
            }
        }

        [Test]
        [Order(1)]
        public void Test_CreateManager_Expect_NotNull()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            Assert.NotNull(mgr);
        }

        [Test]
        [Order(2)]
        public void Test_ReadDefaultMetaPath_Expect_Appdata()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            var metaPath = mgr.ManagementInformationPath;

            Assert.That(metaPath, Is.SubPathOf(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));
        }

        [Test]
        [Order(3)]
        public void Test_WriteMetaPath_Expect_PathUpdated()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            var metaPath = mgr.ManagementInformationPath;

            Assert.That(metaPath, Is.SamePath(MetaFolder));
        }


        [Test]
        [Order(4)]
        public void Test_ReadAllIndex_Expect_BeEmpty()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            
            var indexes = mgr.GetAllIndexes();
            Assert.That(indexes, Is.Empty);
        }

        [Test]
        [Order(5)]
        public void Test_CreateIndex_Expect_BeUniqueId()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;

            var id = mgr.CreateIndex(Index1, new List<string> { ".txt" });
            Assert.That(id, Is.Not.Zero);
        }

        [Test]
        [Order(6)]
        public void Test_CreateIndexTwice_Expect_Exception()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;

            var id = mgr.CreateIndex(Index2, new List<string> { ".txt" });
            Assert.Catch<NotSupportedException>(() => {
               id = mgr.CreateIndex(Index2, new List<string> { ".txt" });
            });
        }

        [Test]
        [Order(6)]
        public void Test_CreateIndex_Expect_MetaWritten()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;

            mgr.CreateIndex(Index3, new List<string> { ".txt" });
            var overviewFile = Path.Combine(MetaFolder, "IndexOverview.json");
            Assert.That(File.Exists(overviewFile), "Overview doesn't exist");
            var content = File.ReadAllText(overviewFile);
            Assert.That(content.Contains(".txt"), "file extensions are missing");
            Assert.That(content.Contains(".code-searcher"), "path to index folder is missing");
        }

        [Test]
        [Order(7)]
        public void Test_LoadCreatedIndex_Expect_CanBeLoaded()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            var id = mgr.CreateIndex(Index2, new List<string> { ".txt" });
            mgr = null;
            GC.Collect();
            mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;

            var indexes = mgr.GetAllIndexes();
            Assert.That(indexes, Is.Not.Empty);
            Assert.That(indexes.First().ID, Is.EqualTo(id));
        }

        [Test]
        [Order(8)]
        public void Test_GetIndexByID_Expect_ReturnIndex()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            _ = mgr.CreateIndex(Index1, new List<string> { ".txt" });
            _ = mgr.CreateIndex(Index2, new List<string> { ".txt" });
            var referenceIndex = mgr.CreateIndex(Index3, new List<string> { ".txt" });

            var current = mgr.GetIndexById(referenceIndex);
            Assert.That(current, Is.Not.Null);
            Assert.That(current.ID, Is.EqualTo(referenceIndex));
        }

        [Test]
        [Order(9)]
        public void Test_GetIndexByUnkownID_Expect_Null()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            
            var current = mgr.GetIndexById(-1);
            Assert.That(current, Is.Null);
        }

        [Test]
        [Order(10)]
        public void Test_DeleteIndex_Expect_OK()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            _ = mgr.CreateIndex(Index1, new List<string> { ".txt" });
            var referenceIndex = mgr.CreateIndex(Index3, new List<string> { ".txt" });

            var current = mgr.GetIndexById(referenceIndex);
            Assert.That(current, Is.Not.Null);

            mgr.DeleteIndex(referenceIndex);
            
            current = mgr.GetIndexById(referenceIndex);
            Assert.That(current, Is.Null);
        }

        [Test]
        [Order(11)]
        public void Test_DeleteIndexWithUnkownID_Expect_Exception()
        {
            var mgr = Factory.GetCodeSearcherManager(Logger);
            mgr.ManagementInformationPath = MetaFolder;
            
            Assert.Catch<NotSupportedException>(() => { mgr.DeleteIndex(-1); });
        }
    }
}
