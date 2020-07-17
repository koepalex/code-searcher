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
        public void GoToSelectedIndexMenu(ICodeSearcherManager manager, ICodeSearcherIndex selectedIndex, ITextBasedUserInterface tui)
        {
            Program.ShowSelectedIndexMenu(manager, selectedIndex, tui, this);
            tui.SetShouldLoop(false);
        }

        /// <inheritdoc />
        public void GoToCreateNewIndexMenu(ICodeSearcherManager manager, ITextBasedUserInterface tui)
        {
            Program.ShowCreateNewIndexMenu(manager, tui, this);
        }

        /// <inheritdoc />
        public void GoToShowAllIndexesMenu(ICodeSearcherManager manager, ITextBasedUserInterface tui)
        {
            Program.ShowAllIndexesMenu(manager, tui, this);
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
