using NUnit.Framework;
using System;

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

        [Test]
        public void The_CommandLineHandler_Recognized_Mode_Auto()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=auto" });
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("auto"));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Mode_Auto_Via_ShortCut()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=a" });
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("a"));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Mode_Index()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("index"));
            Assert.That(sut[IndexPath], Is.EqualTo("SomeIndexPath"));
            Assert.That(sut[SourcePath], Is.EqualTo("SomeSourcePath"));
        }

        [Test]
        public void The_CommandLineHandler_Mode_Index_Supports_OptionArgument_FileExtention()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--fe=txt", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[FileExtensions], Is.EqualTo("txt"));
        }

        [Test]
        public void The_CommandLineHandler_Mode_Index_Supports_OptionArgument_FileExtention_AsCommaSeparated()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--fe=.txt,.tex", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[FileExtensions], Is.EqualTo(".txt,.tex"));
        }


       [Test]
        public void The_CommandLineHandler_Mode_Index_OptionArgument_FileExtention_HasDefault()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=index", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            Assert.True(result);
            Assert.That(sut[FileExtensions], Is.EqualTo(".cs,.xml,.csproj"));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Mode_Search()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("search"));
            Assert.That(sut[SearchedWord], Is.EqualTo("bla"));
            Assert.That(sut[IndexPath], Is.EqualTo("SomeIndexPath"));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Mode_Index_Via_Shortcut()
        {
            CmdLineHandler sut = new CmdLineHandler();
            var result = sut.Parse(new string[] {"-m=i", "--ip=SomeIndexPath", "--sp=SomeSourcePath" });
            
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("index"));
            Assert.That(sut[IndexPath], Is.EqualTo("SomeIndexPath"));
            Assert.That(sut[SourcePath], Is.EqualTo("SomeSourcePath"));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Mode_Search_Via_ShortCut()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=s" , "--sw=bla", "--fe=xml", "--ip=."});
            
            Assert.True(result);
            Assert.That(sut[ProgramMode], Is.EqualTo("search"));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Optional_Argument_HitsPerPage()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hpp=100"});
            
            Assert.True(result);
            Assert.That(sut[HitsPerPage], Is.EqualTo("100"));
        }

        [Test]
        public void The_CommandLineHandler_OptionalArgument_HitsPerPage_Default_MinusOne()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[HitsPerPage], Is.EqualTo("-1"));
        }


        [Test]
        public void The_CommandLineHandler_Throws_Exception_NotANumber_HitsPerPage_Argument()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            Assert.Throws<FormatException>(()=> sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hpp=hallo"}));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_Optional_Argument_NumberOfHits()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--hits=100","--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[NumberOfHits], Is.EqualTo("100"));
        }

        [Test]
        public void The_CommandLineHandler_OptionalArgument_NumberOfHits_Default()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[NumberOfHits], Is.EqualTo("1000"));
        }

        [Test]
        public void The_CommandLineHandler_Throws_Exception_NotANumber_NumberOfHits_Argument()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            Assert.Throws<FormatException>(()=> sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--hits=hallo"}));
        }

        [Test]
        public void The_CommandLineHandler_Recognized_OptionalArgument_Export()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--export"});
            
            Assert.True(result);
            Assert.That(sut[ExportToFile], Is.EqualTo("True"));
        }

        [Test]
        public void The_CommandLineHandler_OptionalArgument_Export_Default_Null()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[ExportToFile], Is.Null);
        }
      

        [Test]
        public void The_CommandLineHandler_Recognized_OptionalArgument_WildCard()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search" , "--sw=bla", "--ip=SomeIndexPath", "--wildcard"});
            
            Assert.True(result);
            Assert.That(sut[WildcardSearch], Is.EqualTo("True"));
        }

        [Test]
        public void The_CommandLineHandler_OptionalArgument_WildCard_Default_Null()
        {
            CmdLineHandler sut = new CmdLineHandler();
            
            var result = sut.Parse(new string[] {"-m=search", "--sw=bla", "--ip=SomeIndexPath"});
            
            Assert.True(result);
            Assert.That(sut[ExportToFile], Is.Null);
        }
    }
}
