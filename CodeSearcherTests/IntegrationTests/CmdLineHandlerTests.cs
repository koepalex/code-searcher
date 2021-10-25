using NLog;
using NUnit.Framework;
using System;
using System.IO;
using Moq;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    [Category("SafeForCI")]
    public class CmdLineHandlerTests 
    {
        [Test]
        public void GetProgramMode_ParameterAuto_ProgramModeAuto()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);

            var result = sut.Parse(new []{"auto"});
            
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Auto));
        }

        [TestCase("-i")]
        [TestCase("--indexPath")]
        public void GetProgramMode_ParameterIndex_ProgramModeIndex(string indexPath) 
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            var result = sut.Parse(new string[] {"index", $"{indexPath}", "SomeIndexPath", "-s", "SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Index));
            Assert.That(sut.IndexPath, Is.EqualTo("SomeIndexPath"));
            Assert.That(sut.SourcePath, Is.EqualTo("SomeSourcePath"));

            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Index));
        }

        [Test]
        public void FileExtensions_IndexModeWithOptionalArgument_ReturnsFileExtention()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            var result = sut.Parse(new string[] {"index", "--indexPath", "SomeIndexPath", "-f", "txt", "--SourcePath", "SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut.GetFileExtensionsAsList, Is.EquivalentTo(new [] {"txt"}));
        }

        [Test]
        public void FileExtensions_IndexModeWithoutOptionalExtentionParemeter_ReturnsDefault()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            var result = sut.Parse(new string[] {"index", "-i", "SomeIndexPath", "-s", "SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut.GetFileExtensionsAsList, Is.EquivalentTo(new [] {".cs", ".xml", ".csproj"}));
        }

        [Test]
        public void GetExtention_ParseInputWithExtentionParameter_ReturnsSplittedExtentionList() 
        {
            var extention = ".txt, .doc, .tex";
            
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            sut.Parse(new string[] {"index", "-i", "SomeIndexPath", "--fileExtensions", $"{extention}", "--sourcePath", "SomeSourcePath" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.EquivalentTo(new []{".txt", ".doc", ".tex"}));
        }
        
        [Test]
        public void GetExtention_ParseInputWithoutExtentionParameter_ReturnsDefaultList() 
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            sut.Parse(new string[] {"index", "--indexpath", "SomeIndexPath", "-s", "SomeSourcePath" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.EquivalentTo(".cs,.xml,.csproj".Split(',')));
        }

        [Test]
        public void GetExtention_ParseNonIndexInput_Null() 
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            sut.Parse(new string[] {"auto" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.Null);
        }

        [Test]
        public void GetProgramMode_ParameterSearch_ProgramModeSearch()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Search));
        }

        [Test]
        public void Parse_SearchModeWithoutSearchWord_Failed()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-i", "SomeIndexPath"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_SearchModeWithoutSearchWord_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), consoleRequest);

            sut.Parse(new string[] { "search", "--indexpath", "SomeIndexPath" });

            Assert.True(executed);
        }

        [Test]
        public void Parse_SearchModeWithoutIndexPath_Failed()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_SearchModeWithoutIndexPath_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), consoleRequest);

            sut.Parse(new string[] { "search", "--searchwOrd", "bla" });

            Assert.True(executed);
        }
        
        [Test]
        public void IndexPath_SearchModeWithIndexPath_ParsedPath()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search", "-s", "bla", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.IndexPath, Is.EqualTo("SomeIndexPath"));
        }

        [Test]
        public void SearchedWord_SearchModeWithIndexPath_ParsedPath()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search", "-s", "bla", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.SearchWord, Is.EqualTo("bla"));
        }
        
        [Test]
        public void HitsPerPage_ParseStringWithOptionalArgumentHitsPerPage_GivenArgumentsValue()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "--IndexPath", "SomeIndexPath", "-p", "100"});
            
            Assert.True(result);
            Assert.That(sut.HitsPerPage, Is.EqualTo(100));
        }

        [Test]
        public void HitsPerPage_ParseStringWithoutOptionalArgumentHitsPerPage_Fails()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "-i", "index", "-p", "SomeIndexPath"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_OptionalArgumentHitsPerPageNotANumber_Fails()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hpp=hallo"});

            Assert.That(result, Is.False);
        }

        [Test]
        public void NumberOfHits_ParseInputWithoutOptionalArgumentNumberOfHits_GivenArgumentsValue()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "-n", "100", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.NumberOfHits, Is.EqualTo(100));
        }

        [Test]
        public void NumberOfHits_ParseInputWithoutOptionalArgumentNumberOfHits_Default()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.NumberOfHits, Is.EqualTo(1000));
        }

        [Test]
        public void Parse_NumberOfHitsParameterIsNotANumber_Throws()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);

            var result = sut.Parse(new string[] { "-m=search", "--sw=bla", "--ip=SomeIndexPath", "--hits=hallo" });
            
            Assert.That(result, Is.False);
        }

        [Test]
        public void ExportToFile_ParseInputWithOptionalArgument_ExportArgument()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "--indexPath", "SomeIndexPath", "--export"});
            
            Assert.True(result);
            Assert.That(sut.ExportToFile, Is.True);
        }

        [Test]
        public void ExportToFile_ParseInputWithExport_Null()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "--searchWord", "bla", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.ExportToFile, Is.False);
        }
      
        [Test]
        public void WildcardSearch_ParseInputOptionalArgumentWildCard_GivenArgument()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "--searchword", "bla", "--indexPath", "SomeIndexPath", "--wildcard"});
            
            Assert.True(result);
            Assert.That(sut.WildCardSearch, Is.True);
        }

        [Test]
        public void WildcardSearch_ParseInputWithOptionalArgumentWildCard_Null()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search", "-s", "bla", "-i", "SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.ExportToFile, Is.False);
        }

        [Test]
        public void Parse_EmptyInput_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), consoleRequest);

            sut.Parse(Array.Empty<string>());

            Assert.True(executed);
        }

        [Test]
        public void Parse_QuestionMark_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), consoleRequest);

            sut.Parse(new []{ "-?"});

            Assert.True(executed);
        }

        [TestCase("--wildcard")]
        [TestCase("--wildCard")]
        [TestCase("--Wildcard")]
        [TestCase("--WildCard")]
        [TestCase("--WILDCARD")]
        public void Parse_CaseMixedInput_SameResult(string wildCard)
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"search" , "-s", "bla", "-i", "SomeIndexPath", wildCard});
            
            Assert.True(result);
            Assert.That(sut.WildCardSearch, Is.True);
        }
    }
}
