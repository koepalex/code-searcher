using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;

namespace CodeSearcher
{
    internal class MenuNavigator : IMenuNavigator
    {
        /// <inheritdoc />
        public void GoToSelectedIndexMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ICodeSearcherIndex selectedIndex, ITextBasedUserInterface tui)
        {
            Program.ShowSelectedIndexMenu(logic, manager, selectedIndex);
            tui.SetShouldLoop(false);
        }

        /// <inheritdoc />
        public void GoToMainMenu(ITextBasedUserInterface tui)
        {
            tui.SetShouldLoop(false);
        }
    }
}
