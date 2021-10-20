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
        [TestCase("-m=auto")]
        [TestCase("-m=a")]
        public void GetProgramMode_ParameterAuto_ProgramModeAuto(string parameter)
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);

            var result = sut.Parse(new []{parameter});
            
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Auto));
        }

        [TestCase("-m=index")]
        [TestCase("-m=i")]
        public void GetProgramMode_ParameterIndex_ProgramModeIndex(string parameter) 
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            var result = sut.Parse(new string[] {parameter, "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
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
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--fe=txt", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut.GetFileExtensionsAsList, Is.EquivalentTo(new [] {"txt"}));
        }

        [Test]
        public void FileExtensions_IndexModeWithoutOptionalExtentionParemeter_ReturnsDefault()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut.GetFileExtensionsAsList, Is.EquivalentTo(new [] {".cs", ".xml", ".csproj"}));
        }

        [Test]
        public void GetExtention_ParseInputWithExtentionParameter_ReturnsSplittedExtentionList() 
        {
            var extention = ".txt, .doc, .tex";

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", $"--fe={extention}", "--sp=SomeSourcePath" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.EquivalentTo(extention.Split(',')));
        }
        
        [Test]
        public void GetExtention_ParseInputWithoutExtentionParameter_ReturnsDefaultList() 
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.EquivalentTo(".cs,.xml,.csproj".Split(',')));
        }

        [Test]
        public void GetExtention_ParseNonIndexInput_Null() 
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            sut.Parse(new string[] {"-m=a" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.Null);
        }

        [TestCase("-m=search")]
        [TestCase("-m=s")]
        public void GetProgramMode_ParameterSearch_ProgramModeSearch(string mode)
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {mode , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Search));
        }

        [Test]
        public void Parse_SearchModeWithoutSearchWord_Failed()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--ip=SomeIndexPath"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_SearchModeWithoutSearchWord_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), consoleRequest);

            sut.Parse(new string[] { "-m=search", "--ip=SomeIndexPath" });

            Assert.True(executed);
        }

        [Test]
        public void Parse_SearchModeWithoutIndexPath_Failed()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_SearchModeWithoutIndexPath_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), consoleRequest);

            sut.Parse(new string[] { "-m=search", "--sw=bla" });

            Assert.True(executed);
        }
        
        [TestCase("-m=search")]
        [TestCase("-m=s")]
        public void IndexPath_SearchModeWithIndexPath_ParsedPath(string mode)
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {mode , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.IndexPath, Is.EqualTo("SomeIndexPath"));
        }

        [TestCase("-m=search")]
        [TestCase("-m=s")]
        public void SearchedWord_SearchModeWithIndexPath_ParsedPath(string mode)
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {mode , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.SearchWord, Is.EqualTo("bla"));
        }
        
        [Test]
        public void HitsPerPage_ParseStringWithOptionalArgumentHitsPerPage_GivenArgumentsValue()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hpp=100"});
            
            Assert.True(result);
            Assert.That(sut.HitsPerPage, Is.EqualTo(100));
        }

        [Test]
        public void HitsPerPage_ParseStringWithoutOptionalArgumentHitsPerPage_DefaultMinusOne()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.HitsPerPage, Is.EqualTo(-1));
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
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--hits=100","--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.NumberOfHits, Is.EqualTo(100));
        }

        [Test]
        public void NumberOfHits_ParseInputWithoutOptionalArgumentNumberOfHits_Default()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
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
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--export"});
            
            Assert.True(result);
            Assert.That(sut.ExportToFile, Is.True);
        }

        [Test]
        public void ExportToFile_ParseInputWithExport_Null()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.ExportToFile, Is.False);
        }
      
        [Test]
        public void WildcardSearch_ParseInputOptionalArgumentWildCard_GivenArgument()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--wildcard"});
            
            Assert.True(result);
            Assert.That(sut.WildCardSearch, Is.True);
        }

        [Test]
        public void WildcardSearch_ParseInputWithOptionalArgumentWildCard_Null()
        {
            CmdLineHandler sut = new CmdLineHandler(Mock.Of<ILogger>(), ()=>null);
            
            var result = sut.Parse(new string[] {"-m=search", "--sw=bla", "--ip=SomeIndexPath"});
            
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
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", wildCard});
            
            Assert.True(result);
            Assert.That(sut.WildCardSearch, Is.True);
        }
    }
}
