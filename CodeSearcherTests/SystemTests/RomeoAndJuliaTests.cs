using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeSearcher.Tests.SystemTests
{
    [TestFixture]
    [Category("NotSafeForCI")]
    class RomeoAndJuliaTests
    {
        private string m_IndexFolder;
        private string m_SourcePath;
        private const string m_BookFolderName = "Romeo and Julia";
        private const string m_BookFileName = "Romeo and Julia.txt";

        [OneTimeSetUp]
        public void OnTimeSetup()
        {
            // create new folder for lucene index
            var tempFolder = Path.GetTempPath();
            m_IndexFolder = Path.Combine(tempFolder, $"IndexFolder{DateTime.UtcNow.Ticks.ToString()}");

            if (Directory.Exists(m_IndexFolder))
            {
                Directory.Delete(m_IndexFolder, true);
            }
            Directory.CreateDirectory(m_IndexFolder);
            m_SourcePath = TestHelper.GetPathToSystemTestData(m_BookFolderName);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(m_IndexFolder))
            {
                Directory.Delete(m_IndexFolder, true);
            }
        }

        #region Test Classes

        private class TestResultExporterAdapter : IResultExporter
        {
            private MemoryStream _stream;
            private IResultExporter _exporter;
            private IList<string> _reference;

            internal TestResultExporterAdapter(IList<string> reference)
            {
                _stream = new MemoryStream();
                var writer = new StreamWriter(_stream);
                _exporter = Factory.Get().GetWildcardResultExporter(writer);
                _reference = reference;
            }

            public void Dispose()
            {
                _exporter?.Dispose();
                _stream?.Close();
            }

            public void Export(ISearchResultContainer searchResultContainer, string searchedWord)
            {
                _exporter.Export(searchResultContainer, searchedWord);
            }

            internal void Verify()
            {
                var reader = new StreamReader(_stream);
                _stream.Seek(0, SeekOrigin.Begin);
                int counter = 0;
                string line;
                //skip first line
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    counter++;

                    if (!_reference.Contains(line))
                    {
                        Assert.Fail($"line {line} not found!");
                    }
                }
            }
        }
        #endregion

        #region Tests

        [Test]
        [Order(1)]
        [NonParallelizable]
        public void Test_CreateIndexOfRomeoAndJulia()
        {
            var logic = GetCodeSearchLogic();
            logic.CreateNewIndex(
                () => { },
                (fileName) =>
                {
                    Assert.That(fileName.EndsWith(m_BookFileName));
                },
                (numberOfFiles, timeSpan) =>
                {
                    Assert.That(numberOfFiles, Is.EqualTo(1));
                });
        }

        [Test]
        [Order(2)]
        [NonParallelizable]
        public void Test_SearchWildcard_QuestionMark()
        {
            var logic = GetCodeSearchLogic();
            string searchWord = "Sam??on";

            var printerStub = new Mock<ISingleResultPrinter>();
            printerStub.SetupAllProperties();

            var exporter = new TestResultExporterAdapter(new List<string>
            {
                "87;Sampson und) Gregorio(, Capulets Bediente.",
                "111;(Sampson und Gregorio, zween Bediente der Capulets, treten mit",
                "118;Gregorio (zu Sampson.)",
                "121;Sampson.",
                "128;Sampson.",
                "137;Sampson.",
                "144;Sampson.",
                "150;Sampson (zu Gregorio leise.)",
                "156;Sampson (laut.)",
                "166;Sampson.",
                "173;Sampson.",
                "176;Gregorio (zu Sampson leise.)",
                "179;Sampson (laut.)",
                "185;Sampson.",
            });

            logic.SearchWithinExistingIndex(
                startCallback: () => { },
                getSearchWord: () => { return (searchWord, true); },
                getMaximumNumberOfHits: () => { return 1000; },
                getHitsPerPage: () => { return -1; },
                getExporter: () => { return (true, exporter); },
                getSingleResultPrinter: () => { return printerStub.Object; },
                finishedCallback: (timeSpan) => { },
                endOfSearchCallback: () => {
                    exporter?.Verify();
                    exporter?.Dispose();
                },
                wildcardSearch: true);
        }

        [Test]
        [Order(3)]
        [NonParallelizable]
        public void Test_SearchWildcard_Star()
        {
            var logic = GetCodeSearchLogic();
            string searchWord = "Mo*ue";

            var printerStub = new Mock<ISingleResultPrinter>();
            printerStub.SetupAllProperties();

            var exporter = new TestResultExporterAdapter(new List<string>
            {
                "78;Montague und Capulet, die Haeupter von zween edlen Geschlechtern,",
                "80;Romeo, Montaguens Sohn.",
                "88;Abraham, ein Bedienter von Montague.",
                "92;Lady Montague.",
                "113; tapfer gegen die Montaegues zu halten; ihre ganze Unterredung ist",
                "204;wie die Hoelle, wie alle Montaegues und dich--wehr dich, H**",
                "212;Zu Boden mit den Capulets!  Zu Boden mit den Montaegues!  (Der alte",
                "222;Meinen Degen, sag ich; da kommt der alte Montague, und fuchtelt mir",
                "225;(Der alte Montague, und Lady Montague.)",
                "227;Montague.",
                "230;Lady Montague.",
                "242;(Der alte Montague, Lady Montague, und Benvolio bleiben zuruek.)",
                "293;Montague.",
                "308;Montague.",
                "314;Montague.",
                "331;Montague.",
                "509;Montague ist so gut gebunden als ich; er hat die nemliche Straffe",
                "667;reiche Capulet, und wenn ihr keiner vom Haus der Montaegues seyd, so",
                "1108;Der Stimme nach sollte diess ein Montague seyn--hol mir einen Degen,",
                "1118;Oheim, hier ist einer unsrer Feinde, ein Montague; ein Bube der",
                "1307;Er heisst Romeo, er ist ein Montague, der einzige Sohn von unserm",
                "1471;wenn du gleich kein Montague waerest--Was ist Montague?--Es ist",
                "1496;Romeo, und ein Montague?",
                "1551;liebenswuerdiger Montague, ich bin zu zaertlich; du koenntest deswegen",
                "1605;Moechtest du dein Herz wieder zurueknehmen?  Warum das, meine Liebe?",
                "1908;Moerder eines seidnen Knopfs, ein Duellist, ein Duellist!  Ein Mann,",
                "1938;(bon jour); das ist ein franzoesischer guter Morgen fuer eure",
                "1943;Guten Morgen--meine Freunde: Was fuer einen Streich spielt' ich euch",
                "2576;gehorche.  (Der Prinz, Montague, Capulet, ihre Weiber, u.  s.  w.",
                "2591;des moerdrischen Montague gerochen werden.",
                "2620;Er ist ein Verwandter von den Montaguen, die Freundschaft macht ihn",
                "2631;Lady Montague.",
                "2769;Wolltet ihr gut von dem Moerder euers Verwandten reden?",
                "4410;Diss ist der verbannte uebermuethige Montague, der den Vetter meiner",
                "4415;verdammlichen Arbeit, nichtswuerdiger Montague: Willt du deine Wuth",
                "4600;Capulets, wekt die Montaguen auf--Und ihr andere sucht--Die",
                "4658;sich verfehlt; sieh, die Scheide ligt auf dem Rueken des Montaguen,",
                "4664;Alter zu Grabe laeutet.  (Montague zu den Vorigen.)",
                "4667;Komm, Montague--und sieh hier deinen einzigen Sohn und Erben--",
                "4669;Montague.",
                "4769;Grab zu ligen--Wo sind diese Feinde?  Capulet!  Montague!  Seht hier",
                "4678;Montague.",
                "4777;O Bruder Montague, gieb mir deine Hand; das ist meiner Tochter",
                "4780;Montague.",
                "4791;Dieser Morgen bringt uns einen duestern Frieden, und die Sonne",
            });

            logic.SearchWithinExistingIndex(
                startCallback: () => { },
                getSearchWord: () => { return (searchWord, true); },
                getMaximumNumberOfHits: () => { return 1000; },
                getHitsPerPage: () => { return -1; },
                getExporter: () => { return (true, exporter); },
                getSingleResultPrinter: () => { return printerStub.Object; },
                finishedCallback: (timeSpan) => { },
                endOfSearchCallback: () => {
                    exporter?.Verify();
                    exporter?.Dispose();
                },
                wildcardSearch: true);
        }
        #endregion

        #region Private Implementation
        private ICodeSearcherLogic GetCodeSearchLogic()
        {
            var loggerStub = new Mock<ICodeSearcherLogger>();

            return Factory.Get().GetCodeSearcherLogic(
                loggerStub.Object,
                () => m_IndexFolder,
                () => m_SourcePath,
                () => new List<string> { ".txt" });
        }
        #endregion
    }
}
