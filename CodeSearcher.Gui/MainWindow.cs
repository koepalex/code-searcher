using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		idxTextbox.SetSizeRequest(560, -1);
		searchTextbox.SetSizeRequest(595, -1);

		//int clientSizeWidth, clientSizeHeight;
		//this.GetSizeRequest(out clientSizeWidth, out clientSizeHeight);
		scrolledWindow.SetSizeRequest(650, 380);

		for (int i = 0; i < 10; i++)
		{
			var vbox = new VBox();

			var label = new Entry("foo.bar " + i.ToString());
			label.IsEditable = false;
			label.ModifyText(StateType.Normal, new Gdk.Color(0, 0, 150));
			var table = new TreeView();
			var lineNumberColumn = new TreeViewColumn();
			lineNumberColumn.Title = "Linenumber";

			var lineNumberCell = new CellRendererText();
			lineNumberColumn.PackStart(lineNumberCell, true);

			lineNumberColumn.AddAttribute(lineNumberCell, "text", 0);

			table.AppendColumn(lineNumberColumn);

			var store = new ListStore(typeof(string));
			store.AppendValues(42.ToString());

			table.Model = store;

			vbox.Add(label);
			vbox.Add(table);

			resultVBox.Add(vbox);

			resultVBox.ShowAll();
		}



	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}
}
