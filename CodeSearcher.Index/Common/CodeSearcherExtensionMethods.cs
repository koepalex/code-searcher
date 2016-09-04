using System;
using System.Collections.Generic;

namespace CodeSearcher.BusinessLogic
{
	public static class CodeSearcherExtensionMethods
	{
		internal static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> elements, int batchSize)
		{
			var batchedElements = new T[batchSize];
			int counter = 0;
			foreach (T element in elements)
			{
				batchedElements[counter++] = element;

				if (counter == batchSize)
				{
					yield return batchedElements;
					Array.Clear(batchedElements, 0, batchSize);
					counter = 0;
				}
			}

			if (counter != 0)
			{
				yield return batchedElements;
			}
		}
	}
}

