using NUnit.Framework;
using CodeSearcher.BusinessLogic;
using Moq;
using CodeSearcher.Interfaces;
using System;
using System.Collections.Generic;

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
    }
}
