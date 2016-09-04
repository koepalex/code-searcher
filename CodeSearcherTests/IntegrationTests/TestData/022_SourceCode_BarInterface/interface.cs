using System;
using System.IO;
using System.Collections;

namespace TestData.For.Code.Searcher
{
	public interface IMoney 
	{
		ulong Value { get; }
	}
	
	public interface IDrinkInfo
	{
		Guid ID { get;}
		string Name { get; }
		IMoney Price { get; }
		IEnumerable<KeyValuePair<string, string>> Attributes {get;}
	}
	
	public interface IDrink 
	{
		void Consume ();
	}
	
	public interface IBar : IDisposable
	{
		void GotTo(IPerson entity);
		IDrinkInfo Lookup(string drinkName);
		IDrink BuyDrink(IDrinkInfo info, IMoney money);
	}
	
	public abstract class Bar : IBar
	{
		protected IList<IPerson> _customers;
		protected IList<IMoney> _transactions;
		protected IDictionary<IDrinkInfo, Func<IDrink>> _drinks;
		
		public Bar()
		{
			_customers = new List<IPerson>();
			_transactions = new List<IMoney>();
			_drinks = new Dictionary<IDrinkInfo, Func<IDrink>>();
		}
		
		#region IBar implentation
		public void GotTo(IPerson entity)
		{
			_customers.Add(entity);
		}
		
		public abstract IDrinkInfo Lookup(string drinkName);
		
		public abstract IDrink BuyDrink(IDrinkInfo info, IMoney money); 
		#endregion
	}
}