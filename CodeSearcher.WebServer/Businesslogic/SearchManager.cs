using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeSearcher.BusinessLogic;

namespace CodeSearcher.WebServer
{
	public class SearchManager
	{
		public void LookupSearchResults(string idxPath, ResultModel resultModel)
		{
			var results = new List<ResultItems>(50);

			using (var searcher = Factory.GetSearcher(idxPath))
			{
				searcher.SearchFileContent(resultModel.SearchPattern, resultModel.MaximumNumberOfHits, resultContainer => 
				{
					resultModel.NumberOfHits = resultContainer.NumberOfHits;

					foreach (var item in resultContainer)
					{
						results.Add(ReadFileWithHit(item.FileName, resultModel.SearchPattern));
					}
				});
			}

			resultModel.Results = results;
		}

		private ResultItems ReadFileWithHit(string filePath, string searchPattern)
		{
			var linesWithHits = new List<TextElements>(20);
			try
			{
				var lines = File.ReadAllLines(filePath, Encoding.Default);

				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].Contains(searchPattern))
					{
						linesWithHits.Add(new TextElements()
						{
							LineNumber = i,
							Text = lines[i]
						});
					}
				}

			}
			catch 
			{
				linesWithHits.Add(TextElements.Empty);
			}

			return new ResultItems
			{
				FilePath = filePath,
				Lines = linesWithHits
			};
		}
	}
}

