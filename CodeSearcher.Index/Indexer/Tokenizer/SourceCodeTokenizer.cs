using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Util;

namespace CodeSearcher.BusinessLogic
{
	public class SourceCodeTokenizer : CharTokenizer
	{
		private static HashSet<char> s_CharList;
		#region c'tors

		static SourceCodeTokenizer()
		{
			s_CharList = new HashSet<char>();
			//white space for simple word splitting
			s_CharList.Add(' ');
			s_CharList.Add('\\');
			s_CharList.Add('\n');
			s_CharList.Add('\t');
			//math operators
			s_CharList.Add('+');
			s_CharList.Add('-');
			s_CharList.Add('*');
			s_CharList.Add('/');
			s_CharList.Add('=');
			//brackets
			s_CharList.Add('(');
			s_CharList.Add(')');
			s_CharList.Add('{');
			s_CharList.Add('}');
			s_CharList.Add('[');
			s_CharList.Add(']');
			s_CharList.Add('<');
			s_CharList.Add('>');
			//coding operators
			s_CharList.Add('!');
			s_CharList.Add('?'); //also for ?? or .?
			s_CharList.Add(':');
			//bit operators
			s_CharList.Add('&'); //also for &&
			s_CharList.Add('|'); //also for ||
			s_CharList.Add('^');
			// strings
			s_CharList.Add('"');
			s_CharList.Add('\'');
			//misc
			s_CharList.Add(';');
			s_CharList.Add('.');
			s_CharList.Add(',');
			s_CharList.Add('@');
			s_CharList.Add('_');
		}

		public SourceCodeTokenizer(TextReader input) : base(input)
		{
		}


		public SourceCodeTokenizer(AttributeFactory factory, TextReader input) : base(factory, input)
		{
		}



		public SourceCodeTokenizer(AttributeSource source, TextReader input) : base(source, input)
		{
		}
		#endregion

		protected override bool IsTokenChar(char c)
        {
			return !s_CharList.Contains(c);
		}
	}
}

