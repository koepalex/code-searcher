using System;
using System.Collections.Generic;

namespace CodeSearcher.WebServer
{
	public class ResultModel
	{
		public int NumberOfHits { get; set; }
		public string SearchPattern { get; set; }
		//public int NumberOfHits { get; set; }
		public IEnumerable<ResultItems> Results { get; set; }
	}

	public class ResultItems
	{
		public string FilePath { get; set; }
		public IEnumerable<TextElements> Lines { get; set; }
	}

	public class TextElements
	{
		public int LineNumber { get; set; }
		public string Text { get; set; }

		public static TextElements Empty
		{
			get
			{
				return new TextElements
				{
					LineNumber = -1,
					Text = "<no text>"
				};
			}
		}
	}
}

