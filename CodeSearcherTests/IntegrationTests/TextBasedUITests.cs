using NUnit.Framework;
using CodeSearcher.BusinessLogic;
using Moq;
using CodeSearcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Globalization;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    [Category("SafeForCI")]
    class TextBasedUITests
    {
        [Test]
        public void Test_CreateNewIndex_EnteredWrongPath_Expect_ErrorMessage()
        {
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.Setup(tui => tui.ReadLine()).Returns(@"X:\DonotExist");
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(managerStub.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg == "Path do not exist!")), 
                Times.Once);
        }

        [Test]
        public void Test_CreateNewIndex_EnteredCorrectPath_Expect_SuccessMessage()
        {
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.Setup(tui => tui.ReadLine()).Returns(Environment.CurrentDirectory);
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(managerStub.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(
                It.Is<string>((msg) => msg.EndsWith(Environment.CurrentDirectory))), 
                Times.AtLeastOnce);
        }

        [Test]
        public void Test_CreateNewIndex_WithoutExtension_Expect_DefaultExtensions()
        {
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(string.Empty);
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(managerStub.Object, tuiMock.Object, navStub.Object);

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
            var managerStub = new Mock<ICodeSearcherManager>();
            var navStub = new Mock<IMenuNavigator>();
            var tuiMock = new Mock<ITextBasedUserInterface>();

            tuiMock.SetupSequence(tui => tui.ReadLine())
                .Returns(Environment.CurrentDirectory)
                .Returns(".cpp,.json");
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);

            Program.ShowCreateNewIndexMenu(managerStub.Object, tuiMock.Object, navStub.Object);

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

            Program.ShowCreateNewIndexMenu(managerMock.Object, tuiMock.Object, navStub.Object);

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

            Program.ShowCreateNewIndexMenu(managerMock.Object, tuiMock.Object, navMock.Object);

            managerMock.Verify();
            navMock.Verify(nav => nav.GoToSelectedIndexMenu(
                It.IsAny<ICodeSearcherManager>(), 
                It.IsAny<ICodeSearcherIndex>(), 
                It.IsAny<ITextBasedUserInterface>()), 
                Times.Once);
        }

        [Test]
        public void Test_CreateNewIndexWithChoiceBack_Expect_GoToMainMenu()
        {
            const int id = 42;
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

            Program.ShowCreateNewIndexMenu(managerMock.Object, tuiMock.Object, navMock.Object);

            managerMock.Verify();
            navMock.Verify(nav => nav.GoToMainMenu(
                It.IsAny<ITextBasedUserInterface>()),
                Times.Once);
        }

        [Test]
        public void Test_ShowAllIndexesWithoutIndex_Expect_EmptyMessage()
        {
            var indexStub = new Mock<ICodeSearcherIndex>();

            var managerMock = new Mock<ICodeSearcherManager>();
            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(1.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();

            Program.ShowAllIndexesMenu(managerMock.Object, tuiMock.Object, navStub.Object);

            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "There are currently no folders indexed!")), Times.Once);
        }

        [Test]
        public void Test_ShowAllIndexesWithoutIndex_Expect_GotoMainMenu()
        {
            var indexStub = new Mock<ICodeSearcherIndex>();

            var managerMock = new Mock<ICodeSearcherManager>();
            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(1.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowAllIndexesMenu(managerMock.Object, tuiMock.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToMainMenu(It.IsAny<ITextBasedUserInterface>()), Times.Once);
        }

        [Test]
        public void Test_ShowAllIndexesWithIndex_Expect_IndexOverview()
        {
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

            Program.ShowAllIndexesMenu(managerMock.Object, tuiMock.Object, navStub.Object);

            indexMock1.Verify();
            indexMock2.Verify();
            managerMock.Verify();
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[1] - ID 1 - SourcePath C:\\repo")));
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[2] - ID 2 - SourcePath D:\\repo")));
        }

        [Test]
        public void Test_ShowAllIndexesWithIndex_Expect_GoToDetailsMenu()
        {
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

            Program.ShowAllIndexesMenu(managerStub.Object, tuiStub.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToSelectedIndexMenu(
                It.IsAny<ICodeSearcherManager>(), 
                It.Is<ICodeSearcherIndex>(i => i == indexStub2.Object), 
                It.IsAny<ITextBasedUserInterface>()), 
                Times.Once);
        }

        [Test]
        public void Test_SelectedIndex_Expect_ShowDetails()
        {
            const int id = 1127001;
            const string sourcePath = @"C:\Users\test\documents\sources";
            const string css = ".css";
            const string html = ".html";
            const string js = ".js";

            var indexStub1 = new Mock<ICodeSearcherIndex>();
            indexStub1.SetupGet(i => i.ID).Returns(id).Verifiable();
            indexStub1.SetupGet(i => i.SourcePath).Returns(sourcePath).Verifiable();
            indexStub1.SetupGet(i => i.FileExtensions).Returns(new List<string>() { css, html, js }).Verifiable();
            indexStub1.SetupGet(i => i.CreatedTime).Returns(DateTime.ParseExact("31.12.1999 23:59:59", "dd.MM.yyyy H:mm:ss", null)).Verifiable();
            
            var managerStub = new Mock<ICodeSearcherManager>();
            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(3.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();

            Program.ShowSelectedIndexMenu(managerStub.Object, indexStub1.Object, tuiMock.Object, navStub.Object);

            indexStub1.Verify();
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg.EndsWith(id.ToString()))), Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg.EndsWith(sourcePath))), Times.Once);
            tuiMock.Verify(tui => tui.Write(It.Is<string>(msg => msg.StartsWith(css))), Times.Once);
            tuiMock.Verify(tui => tui.Write(It.Is<string>(msg => msg.StartsWith(html))), Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg.EndsWith(sourcePath))), Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg.EndsWith("1999-12-31 23:59:59"))), Times.Once);
        }

        [Test]
        public void Test_SelectedIndex_Expect_DeleteIndex()
        {
            const int id = 1127001;
            const string sourcePath = @"C:\Users\test\documents\sources";
            const string css = ".css";
            const string html = ".html";
            const string js = ".js";

            var indexStub1 = new Mock<ICodeSearcherIndex>();
            indexStub1.SetupGet(i => i.ID).Returns(id);
            indexStub1.SetupGet(i => i.SourcePath).Returns(sourcePath);
            indexStub1.SetupGet(i => i.FileExtensions).Returns(new List<string>() { css, html, js });
            indexStub1.SetupGet(i => i.CreatedTime).Returns(DateTime.ParseExact("31.12.1999 23:59:59", "dd.MM.yyyy H:mm:ss", null)).Verifiable();

            var managerMock = new Mock<ICodeSearcherManager>();

            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(2.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();

            Program.ShowSelectedIndexMenu(managerMock.Object, indexStub1.Object, tuiMock.Object, navStub.Object);

            managerMock.Verify(manager => manager.DeleteIndex(It.Is<int>(idToDelete => idToDelete == id)));
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "Index with ID 1127001 deleted!")));
        }

        [Test]
        public void Test_SelectedIndex_Expect_SearchInIndex()
        {
            const int id = 1127001;
            const string sourcePath = @"C:\Users\test\documents\sources";
            const string css = ".css";
            const string html = ".html";
            const string js = ".js";

            var indexStub1 = new Mock<ICodeSearcherIndex>();
            indexStub1.SetupGet(i => i.ID).Returns(id);
            indexStub1.SetupGet(i => i.SourcePath).Returns(sourcePath);
            indexStub1.SetupGet(i => i.FileExtensions).Returns(new List<string>() { css, html, js });
            indexStub1.SetupGet(i => i.CreatedTime).Returns(DateTime.ParseExact("31.12.1999 23:59:59", "dd.MM.yyyy H:mm:ss", null)).Verifiable();

            var managerMock = new Mock<ICodeSearcherManager>();
            var logicMock = new Mock<ICodeSearcherLogic>();

            var tuiMock = new Mock<ITextBasedUserInterface>();
            tuiMock.Setup(tui => tui.ReadLine())
                .Returns(1.ToString());
            tuiMock.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();
            Factory.CodeSearcherLogic = logicMock.Object;

            Program.ShowSelectedIndexMenu( managerMock.Object, indexStub1.Object, tuiMock.Object, navStub.Object);

            logicMock.Verify(logic => logic.SearchWithinExistingIndex(
                It.IsAny<Action>(),
                It.IsAny<Func<(string, bool)>>(),
                It.IsAny<Func<int>>(),
                It.IsAny<Func<int>>(),
                It.IsAny<Func<(bool, IResultExporter)>>(),
                It.IsAny<Func<ISingleResultPrinter>>(),
                It.IsAny<Action<TimeSpan>>(),
                It.IsAny<Action>(),
                It.IsAny<Action>(),
                It.IsAny<bool>()
                ), Times.Once);
            tuiMock.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "ID:\t\t1127001")));
        }


        [Test]
        public void Test_SelectedIndex_Expect_GotoMainMenu()
        {
            const int id = 1127001;
            const string sourcePath = @"C:\Users\test\documents\sources";
            const string css = ".css";
            const string html = ".html";
            const string js = ".js";

            var indexStub1 = new Mock<ICodeSearcherIndex>();
            indexStub1.SetupGet(i => i.ID).Returns(id);
            indexStub1.SetupGet(i => i.SourcePath).Returns(sourcePath);
            indexStub1.SetupGet(i => i.FileExtensions).Returns(new List<string>() { css, html, js });
            indexStub1.SetupGet(i => i.CreatedTime).Returns(DateTime.ParseExact("31.12.1999 23:59:59", "dd.MM.yyyy H:mm:ss", null)).Verifiable();

            var managerMock = new Mock<ICodeSearcherManager>();

            var tuiStub = new Mock<ITextBasedUserInterface>();
            tuiStub.Setup(tui => tui.ReadLine())
                .Returns(3.ToString());
            tuiStub.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowSelectedIndexMenu(managerMock.Object, indexStub1.Object, tuiStub.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToMainMenu(It.IsAny<ITextBasedUserInterface>()));
        }

        [Test]
        public void Test_MainMenu_Expect_Exit()
        {
            var managerMock = new Mock<ICodeSearcherManager>();

            var tuiStub = new Mock<ITextBasedUserInterface>();
            tuiStub.Setup(tui => tui.ReadLine())
                .Returns(3.ToString());
            tuiStub.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowConsoleMainMenu(managerMock.Object, tuiStub.Object, navMock.Object);

            navMock.Verify(nav => nav.ExitMenu(), Times.Once);
            navMock.Verify(nav => nav.GoToShowAllIndexesMenu(
                It.IsAny<ICodeSearcherManager>(),
                It.IsAny<ITextBasedUserInterface>()),
                Times.Never);
            navMock.Verify(nav => nav.GoToCreateNewIndexMenu(
                It.IsAny<ICodeSearcherManager>(),
                It.IsAny<ITextBasedUserInterface>()),
                Times.Never);
        }

        [Test]
        public void Test_MainMenu_Expect_GotoAllIndexMenu()
        {
            var managerMock = new Mock<ICodeSearcherManager>();

            var tuiStub = new Mock<ITextBasedUserInterface>();
            tuiStub.Setup(tui => tui.ReadLine())
                .Returns(2.ToString());
            tuiStub.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowConsoleMainMenu(managerMock.Object, tuiStub.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToShowAllIndexesMenu(
                It.IsAny<ICodeSearcherManager>(), 
                It.IsAny<ITextBasedUserInterface>()), 
                Times.Once);
            navMock.Verify(nav => nav.GoToCreateNewIndexMenu(
                It.IsAny<ICodeSearcherManager>(),
                It.IsAny<ITextBasedUserInterface>()),
                Times.Never);
            navMock.Verify(nav => nav.ExitMenu(), Times.Never);
        }

        [Test]
        public void Test_MainMenu_Expect_GotoCreateNewIndexMenu()
        {
            var managerMock = new Mock<ICodeSearcherManager>();

            var tuiStub = new Mock<ITextBasedUserInterface>();
            tuiStub.Setup(tui => tui.ReadLine())
                .Returns(1.ToString());
            tuiStub.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navMock = new Mock<IMenuNavigator>();

            Program.ShowConsoleMainMenu(managerMock.Object, tuiStub.Object, navMock.Object);

            navMock.Verify(nav => nav.GoToCreateNewIndexMenu(
                It.IsAny<ICodeSearcherManager>(),
                It.IsAny<ITextBasedUserInterface>()),
                Times.Once);
            navMock.Verify(nav => nav.GoToShowAllIndexesMenu(
                It.IsAny<ICodeSearcherManager>(),
                It.IsAny<ITextBasedUserInterface>()),
                Times.Never);
            navMock.Verify(nav => nav.ExitMenu(), Times.Never);
        }

        [Test]
        public void Test_MainMenu_Expect_MenuItems()
        {
            var managerMock = new Mock<ICodeSearcherManager>();

            var tuiStub = new Mock<ITextBasedUserInterface>();
            tuiStub.Setup(tui => tui.ReadLine())
                .Returns(3.ToString());
            tuiStub.Setup(tui => tui.ShouldLoop()).Returns(false);
            var navStub = new Mock<IMenuNavigator>();

            Program.ShowConsoleMainMenu( managerMock.Object, tuiStub.Object, navStub.Object);

            tuiStub.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[1] Create New Index")), Times.Once);
            tuiStub.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[2] Show all Indexes")), Times.Once);
            tuiStub.Verify(tui => tui.WriteLine(It.Is<string>(msg => msg == "[3] Exit")), Times.Once);
            tuiStub.Verify(tui => tui.Clear(), Times.Once);
        }
    }
}
