using System.Collections.Generic;
using System.Linq;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal abstract class BuildPropertyHelper
	{
		public static bool IsEmpty<T>(IEnumerable<T> coll)
		{
			if (coll != null)
			{
				return !coll.Any();
			}
			return true;
		}

		public static bool OrderInsensitiveSequenceEqual<T>(IEnumerable<T> coll1, IEnumerable<T> coll2)
		{
			if (IsEmpty(coll1) != IsEmpty(coll2))
			{
				return false;
			}
			if (IsEmpty(coll1))
			{
				return true;
			}
			return coll1.OrderBy((T t) => t).SequenceEqual(coll2.OrderBy((T t) => t));
		}
	}
}
