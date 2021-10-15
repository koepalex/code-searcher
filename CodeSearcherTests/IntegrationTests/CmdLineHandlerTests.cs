using NUnit.Framework;
using System;
using System.IO;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    [Category("SafeForCI")]
    public class CmdLineHandlerTests 
    {
        private static string SourcePath => "SourcePath";
        private static string IndexPath => "IndexPath";
        private static string FileExtensions => "FileExtensions";
        private static string SearchedWord => "SearchedWord";
        private static string ProgramMode => "ProgramMode";
        private static string NumberOfHits => "NumberOfHits";
        private static string HitsPerPage => "HitsPerPage";
        private static string ExportToFile => "ExportToFile";
        private static string WildcardSearch => "WildcardSearch";

        [TestCase("-m=auto")]
        [TestCase("-m=a")]
        public void GetProgramMode_ParameterAuto_ProgramModeAuto(string parameter)
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);

            var result = sut.Parse(new []{parameter});
            
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Auto));
        }

        [TestCase("-m=index")]
        [TestCase("-m=i")]
        public void GetProgramMode_ParameterIndex_ProgramModeIndex(string parameter) 
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            var result = sut.Parse(new string[] {parameter, "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("index"));
            Assert.That(sut[IndexPath], Is.EqualTo("SomeIndexPath"));
            Assert.That(sut[SourcePath], Is.EqualTo("SomeSourcePath"));

            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Index));
        }

        [Test]
        public void FileExtensions_IndexModeWithOptionalArgument_ReturnsFileExtention()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--fe=txt", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[FileExtensions], Is.EqualTo("txt"));
        }

        [Test]
        public void FileExtensions_IndexModeWithoutOptionalExtentionParemeter_ReturnsDefault()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[FileExtensions], Is.EqualTo(".cs,.xml,.csproj"));
        }

        [Test]
        public void GetExtention_ParseInputWithExtentionParameter_ReturnsSplittedExtentionList() 
        {
            var extention = ".txt, .doc, .tex";

            CmdLineHandler sut = new CmdLineHandler(()=>null);
            sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", $"--fe={extention}", "--sp=SomeSourcePath" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.EquivalentTo(extention.Split(',')));
        }
        
        [Test]
        public void GetExtention_ParseInputWithoutExtentionParameter_ReturnsDefaultList() 
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.EquivalentTo(".cs,.xml,.csproj".Split(',')));
        }

        [Test]
        public void GetExtention_ParseNonIndexInput_Null() 
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            sut.Parse(new string[] {"-m=a" });
            
            Assert.That(sut.GetFileExtensionsAsList(), Is.Null);
        }

        [TestCase("-m=search")]
        [TestCase("-m=s")]
        public void GetProgramMode_ParameterSearch_ProgramModeSearch(string mode)
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {mode , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut.GetProgramMode, Is.EqualTo(ProgramModes.Search));
        }

        [Test]
        public void Parse_SearchModeWithoutSearchWord_Failed()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--ip=SomeIndexPath"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_SearchModeWithoutSearchWord_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(consoleRequest);

            sut.Parse(new string[] { "-m=search", "--ip=SomeIndexPath" });

            Assert.True(executed);
        }

        [Test]
        public void Parse_SearchModeWithoutIndexPath_Failed()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla"});
            
            Assert.False(result);
        }

        [Test]
        public void Parse_SearchModeWithoutIndexPath_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(consoleRequest);

            sut.Parse(new string[] { "-m=search", "--sw=bla" });

            Assert.True(executed);
        }
        
        [TestCase("-m=search")]
        [TestCase("-m=s")]
        public void IndexPath_SearchModeWithIndexPath_ParsedPath(string mode)
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {mode , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[IndexPath], Is.EqualTo("SomeIndexPath"));
        }

        [TestCase("-m=search")]
        [TestCase("-m=s")]
        public void SearchedWord_SearchModeWithIndexPath_ParsedPath(string mode)
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {mode , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[SearchedWord], Is.EqualTo("bla"));
        }
        
        [Test]
        public void HitsPerPage_ParseStringWithOptionalArgumentHitsPerPage_GivenArgumentsValue()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hpp=100"});
            
            Assert.True(result);
            Assert.That(sut[HitsPerPage], Is.EqualTo("100"));
        }

        [Test]
        public void HitsPerPage_ParseStringWithoutOptionalArgumentHitsPerPage_DefaultMinusOne()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[HitsPerPage], Is.EqualTo("-1"));
        }

        [Test]
        public void Parse_OptionalArgumentHitsPerPageNotANumber_Throws()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            Assert.Throws<FormatException>(()=> sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hpp=hallo"}));
        }

        [Test]
        public void NumberOfHits_ParseInputWithoutOptionalArgumentNumberOfHits_GivenArgumentsValue()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--hits=100","--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[NumberOfHits], Is.EqualTo("100"));
        }

        [Test]
        public void NumberOfHits_ParseInputWithoutOptionalArgumentNumberOfHits_Default()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[NumberOfHits], Is.EqualTo("1000"));
        }

        [Test]
        public void Parse_NumberOfHitsParameterIsNotANumber_Throws()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            Assert.Throws<FormatException>(()=> sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hits=hallo"}));
        }

        [Test]
        public void ExportToFile_ParseInputWithOptionalArgument_ExportArgument()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--export"});
            
            Assert.True(result);
            Assert.That(sut[ExportToFile], Is.EqualTo("True"));
        }

        [Test]
        public void ExportToFile_ParseInputWithExport_Null()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[ExportToFile], Is.Null);
        }
      
        [Test]
        public void WildcardSearch_ParseInputOptionalArgumentWildCard_GivenArgument()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--wildcard"});
            
            Assert.True(result);
            Assert.That(sut[WildcardSearch], Is.EqualTo("True"));
        }

        [Test]
        public void WildcardSearch_ParseInputWithOptionalArgumentWildCard_Null()
        {
            CmdLineHandler sut = new CmdLineHandler(()=>null);
            
            var result = sut.Parse(new string[] {"-m=search", "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[ExportToFile], Is.Null);
        }

        [Test]
        public void Parse_EmptyInput_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(consoleRequest);

            sut.Parse(Array.Empty<string>());

            Assert.True(executed);
        }

        [Test]
        public void Parse_QuestionMark_PrintsHelp()
        {
            bool executed = false;
            Func<TextWriter> consoleRequest = () => { executed = true; return null; };

            CmdLineHandler sut = new CmdLineHandler(consoleRequest);

            sut.Parse(new []{ "-?"});

            Assert.True(executed);
        }
    }
}
