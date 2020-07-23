namespace CodeSearcher.BusinessLogic.InternalInterfaces
{
    /// <summary>
    /// Controlability interface to modify Factory behaviour
    /// </summary>
    internal interface ITestFactory
    {
        /// <summary>
        /// To replace CodeSearcherLogic with mock/stub
        /// </summary>
        ICodeSearcherLogic CodeSearcherLogic { set; }
    }
}
