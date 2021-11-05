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

		public static readonly RoutedUICommand SearchInIndex = new RoutedUICommand
		(
			"Search In Index",
			"Search In Index",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F2)
			}
		);
	}
}
