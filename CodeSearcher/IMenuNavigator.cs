using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;

namespace CodeSearcher
{
    /// <summary>
    /// Interface to navigate between menus
    /// </summary>
    internal interface IMenuNavigator
    {
        /// <summary>
        /// Jumping to the menu for a specifiy code-searcher index
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="manager"></param>
        /// <param name="selectedIndex"></param>
        void GoToSelectedIndexMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ICodeSearcherIndex selectedIndex, ITextBasedUserInterface tui);
        
        /// <summary>
        /// Jumping to the main menu
        /// </summary>
        /// <param name="tui"></param>
        void GoToMainMenu(ITextBasedUserInterface tui);
    }
}
