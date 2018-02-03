using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2015To2017.Utilities
{
	public static class DictionaryHelper
	{
		public static TValue GetValue<TKey, TValue>(
			this IDictionary<TKey, TValue> targetDicitonary,
			TKey key,
			TValue defaultValue = default(TValue))
		{
			if (key == null)
			{
				return defaultValue;
			}
			if (targetDicitonary.ContainsKey(key))
			{
				return targetDicitonary[key];
			}
			else
			{
				return defaultValue;
			}
		}
	}
}
