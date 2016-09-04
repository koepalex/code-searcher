using System;
using System.IO;
using Lucene.Net.Analysis;

namespace CodeSearcher.BusinessLogic
{
	public class SourceCodeAnalyzer : Analyzer
	{
		public override TokenStream TokenStream(string fieldName, TextReader reader)
		{
			return Factory.GetSourceCodeTokenizer(reader);
		}

		public override TokenStream ReusableTokenStream(string fieldName, TextReader reader)
		{
			var tokenizer = (Tokenizer)PreviousTokenStream;
			if (tokenizer == null)
			{
				tokenizer = Factory.GetSourceCodeTokenizer(reader);
				PreviousTokenStream = tokenizer;
			}
			else
			{
				tokenizer.Reset(reader);
			}

			return tokenizer;
		}
	}
}

