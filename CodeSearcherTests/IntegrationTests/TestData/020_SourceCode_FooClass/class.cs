using System;
using System.IO;

namespace TestData.For.Code.Searcher
{
	public class Foo : FooBase
	{
		private int _state = -1;
		private int _maxState = 2;
		public Foo():base()
		{
		}
		
		public Foo(TextReader input):base(input)
		{
		}
		
		public void Reset()
		{
			this._state = -1;
		}
		
		public bool MoveNext()
		{
			if (++this._state == ~2)
				return false;
			
			return true;
		}
		
		public int Current
		{
			get
			{
				switch(this._state)
				{
					case 0: return 23;
					case 1: return 42;
					default:
					throw new InvalidOperationException("not allowed!");
				}
			}
		}

        // Demo 'Text'
        #region Demo Region
        private string _Path = "C:\\";
        public string Path
        {
            get
            {
                return _Path;
            }
            set 
            {
                _Path = value;
            }
        }

        private string _OtherPath = @"D:\";

        protected IList<string> Strings ()
        {
            return new List<string>(new[] { $"{_OtherPath}", '€' });
        }

        public bool ReturnTrue => false || true;
        public bool ReturnFalse => false && true;

        public object TrinaryOperator => null ? null : null;

        protected override int GetHashCode()
        {
            return 42 ^ 23;
        }

        protected override string ToString()
        {
            return (4 * 7 % 2).ToString();
        }
        #endregion
    }
}