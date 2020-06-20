using NUnit.Framework;
using CodeSearcher.BusinessLogic;
using Moq;
using CodeSearcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Configuration;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    [Category("SafeForCI")]
    class TextBasedUITests
    {
        [Test]
        public void Test_CreateNewIndex_EnteredWrongPath_Expect_ErrorMessage()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.Setup(tui => tui.ReadLine()).Returns(@"X:\DonotExist");
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerStub.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg == "Path do not exist!")), 
                Times.Once);
        }

        [Test]
        public void Test_CreateNewIndex_EnteredCorrectPath_Expect_SuccessMessage()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.Setup(tui => tui.ReadLine()).Returns(Environment.CurrentDirectory);
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerStub.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(Environment.CurrentDirectory))), 
                Times.AtLeastOnce);
        }

        [Test]
        public void Test_CreateNewIndex_WithoutExtension_Expect_DefaultExtensions()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(string.Empty);
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerStub.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".cs"))), 
                Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".xml"))),
                Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".csproj"))),
                Times.Once);
        }

        [Test]
        public void Test_CreateNewIndex_WithJsonAndCppExtension_Expect_JsonAndCppBeUsed()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(".cpp,.json");
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerStub.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".cpp"))),
                Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".json"))),
                Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".cs"))),
                Times.Never);
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".xml"))),
                Times.Never);
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(".csproj"))),
                Times.Never);
        }

        [Test]
        public void Test_CreateNewIndex_Expect_CallToManager()
        {
            const int id = 42;
            var logicStub = new Mock<ICodeSearcherLogic>();
            var navStub = new Mock<IMenuNavigator>();
            var managerMock = new Mock<ICodeSearcherManager>();
            managerMock.Setup(m => m.CreateIndex(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(id)
                .Verifiable();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(".ts");
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerMock.Object, tuiMock.Object, navStub.Object);

            managerMock.Verify();
            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(id.ToString()))),
                Times.Once);
        }

        [Test]
        public void Test_CreateNewIndexWithChoiceDetails_Expect_GoToDetailsMenu()
        {
            const int id = 42;
            var logicStub = new Mock<ICodeSearcherLogic>();
            var indexStub = new Mock<ICodeSearcherIndex>();
            
            var managerMock = new Mock<ICodeSearcherManager>();
            managerMock.Setup(m => m.CreateIndex(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(id)
                .Verifiable();
            managerMock.Setup(m => m.GetIndexById(id))
                .Returns(indexStub.Object)
                .Verifiable();
            
            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(".ts")
                .Returns(1.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            var navMock = new Mock<IMenuNavigator>();

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerMock.Object, tuiMock.Object, navMock.Object);

            managerMock.Verify();
            navMock.Verify(nav => nav.GoToSelectedIndexMenu(
                It.IsAny<ICodeSearcherLogic>(), 
                It.IsAny<ICodeSearcherManager>(), 
                It.IsAny<ICodeSearcherIndex>(), 
                It.IsAny<ITextBasedUserInterface>()), 
                Times.Once);
        }

        [Test]
        public void Test_CreateNewIndexWithChoiceBack_Expect_GoToMainMenu()
        {
            const int id = 42;
            var logicStub = new Mock<ICodeSearcherLogic>();
            var indexStub = new Mock<ICodeSearcherIndex>();

            var managerMock = new Mock<ICodeSearcherManager>();
            managerMock.Setup(m => m.CreateIndex(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(id)
                .Verifiable();

            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(".ts")
                .Returns(2.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            var navMock = new Mock<IMenuNavigator>();

            Program.ShowCreateNewIndexMenu(logicStub.Object, managerMock.Object, tuiMock.Object, navMock.Object);

            managerMock.Verify();
            navMock.Verify(nav => nav.GoToMainMenu(
                It.IsAny<ITextBasedUserInterface>()),
                Times.Once);
        }

        [Test]
        public void Test_ShowAllIndexesWithoutIndex_Expect_EmptyMessage()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var indexStub = new Mock<ICodeSearcherIndex>();

            var managerMock = new Mock<ICodeSearcherManager>();
            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(1.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();

            Program.ShowAllIndexesMenu(logicStub.Object, managerMock.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "There are currently no folders indexed!")), Times.Once);
        }

        [Test]
        public void Test_ShowAllIndexesWithoutIndex_Expect_GotoMainMenu()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var indexStub = new Mock<ICodeSearcherIndex>();

            var managerMock = new Mock<ICodeSearcherManager>();
            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(1.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowAllIndexesMenu(logicStub.Object, managerMock.Object, tuiMock.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToMainMenu(It.IsAny<ITextBasedUserInterface>()), Times.Once);
        }

        [Test]
        public void Test_ShowAllIndexesWithIndex_Expect_IndexOverview()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var indexMock1 = new Mock<ICodeSearcherIndex>();
            indexMock1.SetupGet(i => i.ID).Returns(1).Verifiable();
            indexMock1.SetupGet(i => i.SourcePath).Returns(@"C:\repo").Verifiable();

            var indexMock2 = new Mock<ICodeSearcherIndex>();
            indexMock2.SetupGet(i => i.ID).Returns(2).Verifiable();
            indexMock2.SetupGet(i => i.SourcePath).Returns(@"D:\repo").Verifiable();

            var managerMock = new Mock<ICodeSearcherManager>();
            managerMock.Setup(manager => manager.GetAllIndexes()).Returns(new List<ICodeSearcherIndex>() { indexMock1.Object, indexMock2.Object }).Verifiable();

            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(3.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();

            Program.ShowAllIndexesMenu(logicStub.Object, managerMock.Object, tuiMock.Object, navStub.Object);

            indexMock1.Verify();
            indexMock2.Verify();
            managerMock.Verify();
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[1] - ID 1 - SourcePath C:\\repo")));
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[2] - ID 2 - SourcePath D:\\repo")));
        }

        [Test]
        public void Test_ShowAllIndexesWithIndex_Expect_GoToDetailsMenu()
        {
            var logicStub = new Mock<ICodeSearcherLogic>();
            var indexStub1 = new Mock<ICodeSearcherIndex>();
            indexStub1.SetupGet(i => i.ID).Returns(1);
            indexStub1.SetupGet(i => i.SourcePath).Returns(@"C:\repo");

            var indexStub2 = new Mock<ICodeSearcherIndex>();
            indexStub2.SetupGet(i => i.ID).Returns(2);
            indexStub2.SetupGet(i => i.SourcePath).Returns(@"D:\repo");

            var managerStub = new Mock<ICodeSearcherManager>();
            managerStub.Setup(manager => manager.GetAllIndexes()).Returns(new List<ICodeSearcherIndex>() { indexStub1.Object, indexStub2.Object });

            var tuiStub = new Mock<ITextBasedUserInterface>();
            tuiStub.Setup(tui => tui.ReadLine())
                .Returns(2.ToString());
            tuiStub.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowAllIndexesMenu(logicStub.Object, managerStub.Object, tuiStub.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToSelectedIndexMenu(
                It.IsAny<ICodeSearcherLogic>(), 
                It.IsAny<ICodeSearcherManager>(), 
                It.Is<ICodeSearcherIndex>(i => i == indexStub2.Object), 
                It.IsAny<ITextBasedUserInterface>()), 
                Times.Once);
        }
    }
}
