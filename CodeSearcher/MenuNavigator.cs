using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;

namespace CodeSearcher
{
    internal class MenuNavigator : IMenuNavigator
    {
        private bool m_MenuLoopActive;

        public MenuNavigator()
        {
            m_MenuLoopActive = true;
        }

        /// <inheritdoc />
        public void GoToSelectedIndexMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ICodeSearcherIndex selectedIndex, ITextBasedUserInterface tui)
        {
            Program.ShowSelectedIndexMenu(logic, manager, selectedIndex, tui, this);
            tui.SetShouldLoop(false);
        }

        /// <inheritdoc />
        public void GoToCreateNewIndexMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ITextBasedUserInterface tui)
        {
            Program.ShowCreateNewIndexMenu(logic, manager, tui, this);
        }

        /// <inheritdoc />
        public void GoToShowAllIndexesMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ITextBasedUserInterface tui)
        {
            Program.ShowAllIndexesMenu(logic, manager, tui, this);
        }

        /// <inheritdoc />
        public void GoToMainMenu(ITextBasedUserInterface tui)
        {
            tui.SetShouldLoop(false);
        }

        /// <inheritdoc />
        public void ExitMenu()
        {
            m_MenuLoopActive = false;
        }

        /// <inheritdoc />
        public bool MenuLoopActive()
        {
            return m_MenuLoopActive;
        }
    }
}
