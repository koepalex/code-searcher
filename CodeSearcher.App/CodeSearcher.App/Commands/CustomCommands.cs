namespace CodeSearcher.App.Commands
{
	using System.Windows.Input;

    public static class CustomCommands
    {
		public static readonly RoutedUICommand SearchInTextArea = new RoutedUICommand
		(
			"Search In Text Area",
			"Search In Text Area",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F, ModifierKeys.Control)
			}
		);
	}
}
