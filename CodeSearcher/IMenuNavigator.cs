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

        /// <summary>
        /// Jumping to the menu to create a new index
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="manager"></param>
        /// <param name="tui"></param>
        void GoToCreateNewIndexMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ITextBasedUserInterface tui);
        
        /// <summary>
        /// Jumping to the menu that list all existing indexes
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="manager"></param>
        /// <param name="tui"></param>
        void GoToShowAllIndexesMenu(ICodeSearcherLogic logic, ICodeSearcherManager manager, ITextBasedUserInterface tui);

        /// <summary>
        /// Return from text based menu to main program (set MenuLoopActive to false)
        /// </summary>
        void ExitMenu();
        
        /// <summary>
        /// Indicates if the menu loop should be stopped
        /// </summary>
        /// <returns></returns>
        bool MenuLoopActive();
    }
}
